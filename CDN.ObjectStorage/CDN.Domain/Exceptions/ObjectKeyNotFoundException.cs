using System;
using System.Runtime.Serialization;

namespace CDN.Domain.Exceptions
{
    public class ObjectKeyNotFoundException : Exception
    {
        public ObjectKeyNotFoundException()
        {

        }

        public ObjectKeyNotFoundException(string message) : base(message)
        {
        }

        public ObjectKeyNotFoundException(string message, Exception ex) : base(message, ex)
        {
        }

        protected ObjectKeyNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}