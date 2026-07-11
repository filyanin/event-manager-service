using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerService.Application.Exceptions
{
    public class SeatsReserveConcurencyException : Exception
    {
        public string Code { get; }
        public SeatsReserveConcurencyException(string code) : base(code)
        {
            Code = code;
        }

        public SeatsReserveConcurencyException(string code, Exception innerException) : base(code, innerException)
        {
            Code = code;
        }
    }
}
