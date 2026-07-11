using EventManagerService.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Domain.ValueObjects
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
                // Используем специальное исключение для диапазонов (min/max)
                var ex = new RangeValidationException(
                    "InvalidPageSize",
                    nameof(pageSize),
                    nameof(minPageSize),
                    nameof(maxPageSize),
                    pageSize,
                    minPageSize,
                    maxPageSize);

                throw ex;
            }
            if (pageNumber < 1)
            {
                var ex = new GreaterThenValidationException("InvalidPageNumber",
                    nameof(pageNumber),
                    "1",
                    pageNumber,
                    1);

                throw ex;
            }
            this.pageSize = pageSize;
            this.pageNumber = pageNumber;
        }
    }
}
