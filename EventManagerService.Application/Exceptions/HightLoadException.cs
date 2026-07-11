using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Application.Exceptions
{
    public class HightLoadException : Exception
    {
        public string Code { get; }
        public HightLoadException(string code) : base(code)
        {
            Code = code;
        }

        public HightLoadException(string code, Exception innerException) : base(code, innerException)
        {
            Code = code;
        }
    }
}
