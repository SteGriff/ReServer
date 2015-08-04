using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReServer.Core.Exceptions
{
    public class ReserverConversionException : Exception
    {
        public ReserverConversionException(string message) : base(message) { }
    }
}
