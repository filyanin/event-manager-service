namespace EventManagerService.Presentation.DTOs.Event
{
    public record PaginatedResult
    {
        public List<OutputEventDTO> Events { get; set; }
        public int Total {  get; set; }
        public int Page {  get; set; }
        public int CurrentPageSize { get; set; }

    }
}
