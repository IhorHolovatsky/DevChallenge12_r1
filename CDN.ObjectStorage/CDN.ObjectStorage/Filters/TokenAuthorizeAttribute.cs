using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CDN.Domain.Constants;
using CDN.OriginServer.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace CDN.OriginServer.Api.Filters
{
    public class TokenAuthorizeAttribute : ActionFilterAttribute
    {
        protected List<string> AllowedTokens { get; }

        private readonly IConfiguration _configuration;

        public TokenAuthorizeAttribute(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            AllowedTokens = _configuration.GetSection("Authentication:Tokens").Get<List<string>>();
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context,
                                                          ActionExecutionDelegate next)
        {
            if (!await Authorize(context))
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
            await base.OnActionExecutionAsync(context, next);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!Authorize(context).Result)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            base.OnActionExecuting(context);
        }

        private Task<bool> Authorize(ActionExecutingContext context)
        {
            var token = ParseToken(context.HttpContext);

            //for skipping token hint like Bearer gf7df7g98df7g89sdf
            token = token?.Contains(' ') ?? false
                 ? token.Split(' ')[1]
                 : token;

            return Task.FromResult(!string.IsNullOrEmpty(token)
                                   && AllowedTokens.Any(t => t.Equals(token)));
        }

        /// <summary>
        /// Get token from request headers / query string / cookie
        /// </summary>
        public virtual string ParseToken(HttpContext context)
        {
            var authHeader = context.Request.Headers[WebConstants.AUTHORIZATION_HEADER_NAME];

            //firstly try to find token in Authorization header
            var token = string.IsNullOrEmpty(authHeader)
                ? null
                : authHeader[0];

            var queryString = context.Request.Query[WebConstants.QUERY_STRING_TOKEN_NAME];

            //if no found, try to find token in Query String
            token = token ?? (string.IsNullOrEmpty(queryString)
                        ? null
                        : queryString[0]);

            var cookie = context.Request.Cookies[WebConstants.FORMS_COOKIE_NAME];

            //if no found, try to find token in Cookie
            token = token ?? (string.IsNullOrEmpty(cookie)
                        ? null
                        : cookie);

            return token;
        }
    }
}