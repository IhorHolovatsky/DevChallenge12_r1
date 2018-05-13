using System;
using System.Runtime.Serialization;

namespace CDN.Domain.Exceptions
{
    public class UploadIdNotFoundException : Exception
    {
        public UploadIdNotFoundException()
        {

        }

        public UploadIdNotFoundException(string message) : base(message)
        {
        }

        public UploadIdNotFoundException(string message, Exception ex) : base(message, ex)
        {
        }

        protected UploadIdNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}