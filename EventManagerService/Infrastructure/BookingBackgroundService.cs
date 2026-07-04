using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Models.Booking;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace EventManagerService.Infrastructure
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BookingBackgroundService> _logger;

        public BookingBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<BookingBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Получаем только идентификаторы ожидaющих бронирований в отдельном scope
                    List<Guid> pendingBookingIds;
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        pendingBookingIds = await db.Bookings
                            .Where(b => b.Status == Domain.Enum.BookingStatus.Pending)
                            .Select(b => b.Id)
                            .ToListAsync(stoppingToken);
                    }

                    if (pendingBookingIds.Count > 0)
                    {
                        // Для каждой брони создаём собственный scope внутри ProcessBookingAsync
                        var tasks = pendingBookingIds.Select(id => ProcessBookingAsync(id, stoppingToken));
                        await Task.WhenAll(tasks);
                    }

                    await Task.Delay(60000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("BookingBackgroundService cancellation requested");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in BookingBackgroundService");
                    await Task.Delay(60000, stoppingToken);
                }
            }
        }

        private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
        {
            Booking? booking = null;
            try
            {
                // Имитация внешнего вызова - выполняется параллельно
                await Task.Delay(10000, stoppingToken);

                // Каждый ProcessBookingAsync использует собственный scope и DbContext
                using var scope = _serviceScopeFactory.CreateScope();
                var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                // Загружаем бронь заново в пределах scope
                try
                {
                    booking = await bookingService.GetBookingByIdAsync(bookingId);
                }
                catch (KeyNotFoundException)
                {
                    _logger.LogWarning(new System.Resources.ResourceManager(typeof(EventManagerService.Properties.ErrorMessages)).GetString("BookingNotFoundWhenProcessing"), bookingId);
                    return;
                }

                // Проверяем, существует ли событие
                EventManagerService.Domain.Models.Event.Event? @event = null;
                try
                {
                    @event = await eventService.GetEventByIdAsync(booking.EventId);
                }
                catch (KeyNotFoundException)
                {
                    @event = null;
                }

                if (@event == null)
                {
                    // Событие было удалено - отклоняем бронь
                    booking.SetBookingRejected(DateTime.UtcNow);
                    await bookingService.RejectBookingAsync(booking.Id);
                    _logger.LogWarning(new System.Resources.ResourceManager(typeof(EventManagerService.Properties.ErrorMessages)).GetString("EventNotFoundBookingRejected"), booking.EventId, booking.Id);
                    return;
                }

                // Событие существует - подтверждаем бронь
                booking.SetBookingConfirmed(DateTime.UtcNow);
                await bookingService.ConfirmBookingAsync(booking.Id);
                _logger.LogInformation(new System.Resources.ResourceManager(typeof(EventManagerService.Properties.ErrorMessages)).GetString("BookingConfirmed"), booking.Id);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(new System.Resources.ResourceManager(typeof(EventManagerService.Properties.ErrorMessages)).GetString("BookingProcessingCancelled"), bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, new System.Resources.ResourceManager(typeof(EventManagerService.Properties.ErrorMessages)).GetString("BookingProcessingErrorRejecting"), bookingId);
                try
                {
                    // Пытаемся отклонить бронь и вернуть места — выполняем в собственном scope
                    using var scope = _serviceScopeFactory.CreateScope();
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    try
                    {
                        // Попытаться загрузить бронь в этом scope, если она ещё не загружена
                        var bookingToHandle = booking ?? await bookingService.GetBookingByIdAsync(bookingId);
                        try
                        {
                            var @event = await eventService.GetEventByIdAsync(bookingToHandle.EventId);
                            if (@event != null)
                            {
                                // Возвращаем место в пул
                                @event.ReleaseSeats();
                            }
                        }
                        catch { }

                        // Отклоняем бронь
                        bookingToHandle.SetBookingRejected(DateTime.UtcNow);
                        await bookingService.RejectBookingAsync(bookingToHandle.Id);
                    }
                        catch (KeyNotFoundException)
                        {
                            _logger.LogWarning(new System.Resources.ResourceManager(typeof(EventManagerService.Properties.ErrorMessages)).GetString("BookingNotFoundWhenReleasing"), bookingId);
                        }
                }
                catch (Exception releaseEx)
                {
                    _logger.LogError(releaseEx, new System.Resources.ResourceManager(typeof(EventManagerService.Properties.ErrorMessages)).GetString("FailedToReleaseResources"), bookingId);
                }
            }
        }
    }
}
