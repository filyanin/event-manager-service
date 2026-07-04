using EventManagerService.Domain.Interfaces.BookingService;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Domain.Models.Booking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace EventManagerService.Infrastructure
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BookingBackgroundService> _logger;
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

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
                    using var scope = _serviceScopeFactory.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    var pendingBookings = await bookingService.GetBookingByStateAsync(Domain.Enum.BookingStatus.Pending);

                    if (pendingBookings.Count > 0)
                    {
                        var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
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

        private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
        {
            try
            {
                // Имитация внешнего вызова - выполняется параллельно
                await Task.Delay(10000, stoppingToken);

                // Захватываем семафор перед изменением состояния
                await _processingSemaphore.WaitAsync(stoppingToken);
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

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
                        _logger.LogWarning($"Event {booking.EventId} not found. Booking {booking.Id} rejected.");
                        return;
                    }

                    // Событие существует - подтверждаем бронь
                    booking.SetBookingConfirmed(DateTime.UtcNow);
                    await bookingService.ConfirmBookingAsync(booking.Id);
                    _logger.LogInformation($"Booking {booking.Id} confirmed successfully");
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"Booking {booking.Id} processing cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing booking {booking.Id}. Rejecting and releasing seats");
                try
                {
                    // Пытаемся отклонить бронь и вернуть места
                    await _processingSemaphore.WaitAsync(stoppingToken);
                    try
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                        try
                        {
                            var @event = await eventService.GetEventByIdAsync(booking.EventId);
                            if (@event != null)
                            {
                                // Возвращаем место в пул
                                @event.ReleaseSeats();
                            }
                        }
                        catch { }

                        // Отклоняем бронь
                        booking.SetBookingRejected(DateTime.UtcNow);
                        await bookingService.RejectBookingAsync(booking.Id);
                    }
                    finally
                    {
                        _processingSemaphore.Release();
                    }
                }
                catch (Exception releaseEx)
                {
                    _logger.LogError(releaseEx, $"Failed to release resources for booking {booking.Id}");
                }
            }
        }
    }
}
