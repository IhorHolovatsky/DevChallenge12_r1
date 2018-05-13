using System;
using System.Runtime.Serialization;

namespace CDN.Domain.Exceptions
{
    public class NoSuitableServerExcetion : Exception
    {
        public NoSuitableServerExcetion()
        {

        }

        public NoSuitableServerExcetion(string message) : base(message)
        {
        }

        public NoSuitableServerExcetion(string message, Exception ex) : base(message, ex)
        {
        }

        protected NoSuitableServerExcetion(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}