using System;
using System.Runtime.Serialization;

namespace CDN.Domain.Exceptions
{
    public class ObjectKeyAlreadyExistsException : Exception
    {
        public string ObjectKey { get; set; }

        public ObjectKeyAlreadyExistsException()
        {

        }

        public ObjectKeyAlreadyExistsException(string message) : base(message)
        {
        }

        public ObjectKeyAlreadyExistsException(string message, Exception ex) : base(message, ex)
        {
        }

        protected ObjectKeyAlreadyExistsException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}