using System.Collections.Generic;

namespace CDN.OriginServer.Api.Models
{
    public class ResponseErrors : List<ResponseError>
    {
        public void Add(string code, string message)
        {
            Add(new ResponseError(code, message));
        }

        public bool IsEmpty => Count == 0;
    }
}