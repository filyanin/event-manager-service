using EventManagerService.Application.Interfaces;
using EventManagerService.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventManagerService.Application.Services
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
                    // Получаем только идентификаторы ожидaющих бронирований в отдельном scope через репозиторий
                    List<Guid> pendingBookingIds;
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                        pendingBookingIds = await bookingRepo.GetPendingIdsAsync(stoppingToken);
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
            DomainBooking? booking = null;
            try
            {
                // Имитация внешнего вызова - выполняется параллельно
                await Task.Delay(10000, stoppingToken);

                // Каждый ProcessBookingAsync использует собственный scope и репозитории
                using var scope = _serviceScopeFactory.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                // Загружаем бронь заново в пределах scope
                try
                {
                    booking = await bookingRepo.GetByIdAsync(bookingId);
                }
                catch (KeyNotFoundException)
                {
                    _logger.LogWarning("BookingNotFound");
                    return;
                }

                // Проверяем, существует ли событие
                DomainEvent? @event = null;
                try
                {
                    @event = await eventRepo.GetByIdAsync(booking.EventId);
                }
                catch (KeyNotFoundException)
                {
                    @event = null;
                }

                if (@event == null)
                {
                    // Событие было удалено - отклоняем бронь
                    booking.SetBookingRejected(DateTime.UtcNow);
                    await bookingService.RejectBookingAsync(bookingId);

                    _logger.LogWarning("EventNotFound");
                    return;
                }

                // Событие существует - подтверждаем бронь
                await bookingService.ConfirmBookingAsync(bookingId);
                _logger.LogInformation("BookingConfirmend");
                    
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("BookingProcessingCancelled");
            }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BookingProcessingErrorRejecting");
                    try
                    {
                        // Пытаемся отклонить бронь и вернуть места через бизнес-правила (BookingService) в собственном scope
                        using var scope = _serviceScopeFactory.CreateScope();
                        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                        try
                        {
                            await bookingService.RejectBookingAsync(bookingId);
                        }
                        catch (KeyNotFoundException)
                        {
                            _logger.LogWarning("BookingNotFoundWhenReleasing");
                        }
                    }
                    catch (Exception releaseEx)
                    {
                        _logger.LogError(releaseEx, "FailedToReleaseResources");
                    }
                }
        }
    }
}
