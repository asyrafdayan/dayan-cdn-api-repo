using System.Runtime.Serialization;

namespace API.Exceptions
{
    public class UserExistsException : Exception
    {
        public UserExistsException()
        {
        }

        public UserExistsException(string? message) : base(message)
        {
        }

        public UserExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
