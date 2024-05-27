using API.Exceptions;
using API.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace API.Middleware
{
    internal sealed class ExceptionMiddleware : IExceptionHandler
    {

        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError("Exception : " + exception.Message);

            ResultDTO resDTO = new()
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Remark = exception.Message
            };

            if (exception is NotFoundException)
            {
                resDTO.StatusCode = (int)HttpStatusCode.NotFound;
            }

            httpContext.Response.StatusCode = resDTO.StatusCode;

            await httpContext.Response.WriteAsJsonAsync(resDTO, cancellationToken);

            return true;
        }
    }
}
