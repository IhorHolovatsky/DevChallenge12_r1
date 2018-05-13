namespace CDN.Domain.Constants
{
    public static class RequestErrorCodes
    {
        public const string OBJECT_KEY_ALREADY_EXISTS = "ObjectAlreadyExists";
        public const string OBJECT_CHUNK_ALREADY_EXISTS = "ObjectChunkAlreadyExists";
        public const string OBJECT_KEY_NOT_FOUND = "UnknownObjectKey";
        public const string INVALID_OBJECT_KEY = "InvalidObjectKey";
        public const string UPLOAD_ID_NOT_FOUND = "UnknownUploadId";
    }
}