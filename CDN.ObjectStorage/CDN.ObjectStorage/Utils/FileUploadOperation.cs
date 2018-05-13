using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CDN.OriginServer.Api.Utils
{
    public class FileUploadOperation : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.OperationId.Equals("ApiUploadByObjectKeyByVersionPost")
                || operation.OperationId.Equals("ApiMultipartByObjectKeyByVersionByUploadIdPut"))
            {
                //Just remove file parameter then readd
                operation.Parameters = operation.Parameters.Where(o => !o.Name.Equals("uploadedFile")).ToList();
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "uploadedFile",
                    In = "formData",
                    Description = "Upload File",
                    Required = true,
                    Type = "file"
                });
                operation.Consumes.Add("multipart/form-data");
            }
        }
    }
}
