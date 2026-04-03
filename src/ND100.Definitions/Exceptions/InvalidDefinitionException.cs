using System;

namespace NDGen.Core.Exceptions
{
    public class InvalidDefinitionException : Exception
    {
        public InvalidDefinitionException(string message) : base(message)
        {
        }

        public InvalidDefinitionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 