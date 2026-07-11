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
                var ex = new DomainValidationException("InvalidPageSize");
                ex.Data["FirstParamName"] = nameof(pageSize);
                ex.Data["FirstParamValue"] = pageSize;
                ex.Data["SecondParamName"] = nameof(minPageSize);
                ex.Data["SecondParamValue"] = minPageSize;
                ex.Data["ThirdParamName"] = nameof(maxPageSize);
                ex.Data["ThirdParamValue"] = maxPageSize;
                throw ex;
            }
            if (pageNumber < 1)
            {
                var ex = new DomainValidationException("InvalidPageNumber");
                ex.Data["FirstParamName"] = nameof(pageNumber);
                ex.Data["FirstParamValue"] = pageNumber;
                throw ex;
            }
            this.pageSize = pageSize;
            this.pageNumber = pageNumber;
        }
    }
}
