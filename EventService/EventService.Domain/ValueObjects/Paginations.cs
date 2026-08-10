namespace EventService.Domain.ValueObjects
{
    public record Paginations
    {
        private readonly int minPageSize = 10;
        private readonly int maxPageSize = 100;
        public int pageSize { get; init; }

        public int pageNumber { get; init; }

        public Paginations(int pageSize, int pageNumber)
        {
            if (pageSize < minPageSize || pageSize > maxPageSize)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"pageSize must be between {minPageSize} and {maxPageSize}");
            }
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "pageNumber must be >= 1");
            }
            this.pageSize = pageSize;
            this.pageNumber = pageNumber;
        }
    }
}
