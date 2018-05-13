using System;
using System.Runtime.Serialization;

namespace CDN.Domain.Exceptions
{
    public class InvalidObjectKeyException : Exception
    {
        public string ObjectKey { get; set; }

        public InvalidObjectKeyException()
        {

        }

        public InvalidObjectKeyException(string message) : base(message)
        {
        }

        public InvalidObjectKeyException(string message, Exception ex) : base(message, ex)
        {
        }

        protected InvalidObjectKeyException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}