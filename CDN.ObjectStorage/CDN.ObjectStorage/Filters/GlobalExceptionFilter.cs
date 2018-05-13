using System;
using System.Net;
using System.Threading.Tasks;
using CDN.Domain.Constants;
using CDN.Domain.Exceptions;
using CDN.OriginServer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CDN.OriginServer.Api.Filters
{
    public class GlobalExceptionFilter : IAsyncExceptionFilter
    {

        public GlobalExceptionFilter()
        {
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (IsKnownException(context))
                return Task.CompletedTask;
        


            context.Result = new JsonResult(GetExceptionDetails(context.Exception))
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            return Task.CompletedTask;
        }

        #region Private methods

        /// <summary>
        /// Recursivelly get exception details
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static object GetExceptionDetails(Exception e)
        {
            if (e == null) return null;

            return new
            {
                Message = "An error has occured.",
                ExceptionMessage = e.Message,
                ExceptionType = e.GetType().FullName,
                e.StackTrace,
                InnerException = GetExceptionDetails(e.InnerException)
            };
        }

        private static bool IsKnownException(ExceptionContext context)
        {
            var exception = context.Exception;

            switch (exception)
            {
                case ObjectKeyAlreadyExistsException _:
                    context.Result = new JsonResult(new ResponseError(RequestErrorCodes.OBJECT_KEY_ALREADY_EXISTS, exception.Message));
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    return true;
                case ObjectChunkAlreadyExistsException _:
                    context.Result = new JsonResult(new ResponseError(RequestErrorCodes.OBJECT_CHUNK_ALREADY_EXISTS, exception.Message));
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    return true;
                case ObjectKeyNotFoundException _:
                    context.Result = new JsonResult(new ResponseError(RequestErrorCodes.OBJECT_KEY_NOT_FOUND, exception.Message));
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return true;
                case InvalidObjectKeyException _:
                    context.Result = new JsonResult(new ResponseError(RequestErrorCodes.INVALID_OBJECT_KEY, exception.Message));
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return true;
                case UploadIdNotFoundException _:
                    context.Result = new JsonResult(new ResponseError(RequestErrorCodes.UPLOAD_ID_NOT_FOUND, exception.Message));
                    context.HttpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    return true;
            }

            return false;
        }

        #endregion
    }
}