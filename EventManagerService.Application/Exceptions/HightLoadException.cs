using System;
using System.Collections.Generic;
using System.Text;

using EventManagerService.Shared.Exceptions;

namespace EventManagerService.Application.Exceptions
{
    public class HightLoadException : AppException
    {
        public HightLoadException(string code) : base(code)
        {
        }

        public HightLoadException(string code, Exception innerException) : base(code, innerException)
        {
        }
    }
}
