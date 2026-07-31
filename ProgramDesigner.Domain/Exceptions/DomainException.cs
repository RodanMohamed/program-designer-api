using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Exceptions
{
   
    // This is distinct from request-shape validation, which happens in the Application layer.
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
