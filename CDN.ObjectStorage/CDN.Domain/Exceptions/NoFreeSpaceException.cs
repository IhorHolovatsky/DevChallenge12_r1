using System;
using System.Runtime.Serialization;

namespace CDN.Domain.Exceptions
{
    public class NoFreeSpaceException : Exception
    {
        public NoFreeSpaceException()
        {

        }

        public NoFreeSpaceException(string message) : base(message)
        {
        }

        public NoFreeSpaceException(string message, Exception ex) : base(message, ex)
        {
        }

        protected NoFreeSpaceException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}