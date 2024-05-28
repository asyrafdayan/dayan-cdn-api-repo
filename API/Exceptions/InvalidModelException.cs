using System.Runtime.Serialization;

namespace API.Exceptions
{
    public class InvalidModelException : Exception
    {
        public InvalidModelException()
        {
        }

        public InvalidModelException(string? message) : base(message)
        {
        }

        public InvalidModelException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
