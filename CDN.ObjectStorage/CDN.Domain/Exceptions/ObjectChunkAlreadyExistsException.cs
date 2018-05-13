using System;
using System.Runtime.Serialization;

namespace CDN.Domain.Exceptions
{
    public class ObjectChunkAlreadyExistsException : Exception
    {
        public string ObjectKey { get; set; }
        public string PartNumber { get; set; }

        public ObjectChunkAlreadyExistsException()
        {

        }

        public ObjectChunkAlreadyExistsException(string message) : base(message)
        {
        }

        public ObjectChunkAlreadyExistsException(string message, Exception ex) : base(message, ex)
        {
        }

        protected ObjectChunkAlreadyExistsException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}