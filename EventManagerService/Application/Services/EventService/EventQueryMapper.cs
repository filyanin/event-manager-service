using EventManagerService.Application.Interfaces.EventService;
using EventManagerService.Domain.Filters;
using EventManagerService.Domain.Interfaces.EventService;
using EventManagerService.Presentation.DTOs.EventService;

namespace EventManagerService.Application.Services.EventService
{
    public class EventQueryMapper : IEventQueryMapper
    {
        private IEventService _eventService;

        public EventQueryMapper(IEventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<OutputEventDTO> AddEventAsync(InputEventDTO newEvent)
        {
#pragma warning disable CS8629 // Тип значения, допускающего NULL, может быть NULL.
            var outputEvent = await _eventService.AddEventAsync(newEvent.Title,
                                                     (DateTime)newEvent.StartAt,
                                                     (DateTime)newEvent.EndAt,
                                                     (int)newEvent.TotalSeat,
                                                     newEvent.Description);
#pragma warning restore CS8629 // Тип значения, допускающего NULL, может быть NULL.

            return new OutputEventDTO(outputEvent);
        }

        public async Task DeleteEventAsync(Guid id)
        {
            await _eventService.DeleteEventAsync(id);
        }

        public async Task<PaginatedResult> GetAllEventAsync(EventsFilters filters, int page, int pageSize)
        {
            List<OutputEventDTO> resultList = new List<OutputEventDTO>();

            var tuple = await _eventService.GetAllEventAsync(filters, page, pageSize);

            foreach (var _event in tuple.Items)
            {
                resultList.Add(new OutputEventDTO(_event));
            }

            return new PaginatedResult() { Events = resultList, Total = tuple.Total, CurrentPageSize = resultList.Count, Page = page};
        }

        public async Task<OutputEventDTO> GetEventByIdAsync(Guid id)
        {
            var _event = await _eventService.GetEventByIdAsync(id);

            return new OutputEventDTO(_event);
        }

        public async Task UpdateEventAsync(Guid id, InputEventDTO updatedEvent)
        {
#pragma warning disable CS8629 // Тип значения, допускающего NULL, может быть NULL.
            await _eventService.UpdateEventAsync(
                id,
                updatedEvent.Title,
                (DateTime)updatedEvent.StartAt,
                (DateTime)updatedEvent.EndAt,
                updatedEvent.Description
                );

        }
    }
}
