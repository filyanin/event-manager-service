using EventManagerService.Domain.Models.DomainBooking;
using EventManagerService.Infrastructure.Interfaces.Repositories;

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
                var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                // Загружаем бронь заново в пределах scope
                try
                {
                    booking = await bookingRepo.GetByIdAsync(bookingId);
                }
                catch (KeyNotFoundException)
                {
                    _logger.LogWarning(new System.Resources.ResourceManager(typeof(EventManagerService.Properties.ErrorMessages)).GetString("BookingNotFoundWhenProcessing"), bookingId);
                    return;
                }

                // Проверяем, существует ли событие
                EventManagerService.Domain.Models.DomainEvent.DomainEvent? @event = null;
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
                    await bookingRepo.RejectAsync(booking.Id);
                    _logger.LogWarning(new System.Resources.ResourceManager(typeof(EventManagerService.Properties.ErrorMessages)).GetString("EventNotFoundBookingRejected"), booking.EventId, booking.Id);
                    return;
                }

                // Событие существует - подтверждаем бронь
                booking.SetBookingConfirmed(DateTime.UtcNow);
                await bookingRepo.ConfirmAsync(booking.Id);
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
                        var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                        var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                        try
                        {
                            // Попытаться загрузить бронь в этом scope, если она ещё не загружена
                            var bookingToHandle = booking ?? await bookingRepo.GetByIdAsync(bookingId);
                            try
                            {
                                var @event = await eventRepo.GetByIdAsync(bookingToHandle.EventId);
                                if (@event != null)
                                {
                                    // Возвращаем место в пул
                                    await eventRepo.ReleaseSeatsAsync(@event.Id);
                                }
                            }
                            catch { }

                            // Отклоняем бронь
                            bookingToHandle.SetBookingRejected(DateTime.UtcNow);
                            await bookingRepo.RejectAsync(bookingToHandle.Id);
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
