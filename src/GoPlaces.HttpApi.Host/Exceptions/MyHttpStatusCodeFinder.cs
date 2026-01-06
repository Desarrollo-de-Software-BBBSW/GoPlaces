using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.ExceptionHandling;

namespace GoPlaces.HttpApi.Host.Exceptions
{
    // Heredamos del default para no perder el resto de los behaviours
    public class MyHttpStatusCodeFinder : DefaultHttpExceptionStatusCodeFinder
    {
        public MyHttpStatusCodeFinder(
            IOptions<AbpExceptionHttpStatusCodeOptions> options)
            : base(options)
        {
        }

        public override HttpStatusCode GetStatusCode(HttpContext httpContext, Exception exception)
        {
            if (exception is BusinessException be && !string.IsNullOrWhiteSpace(be.Code))
            {
                if (be.Code == "Rating.AlreadyExists")
                    return HttpStatusCode.Conflict; // 409

                if (be.Code == "Rating.ScoreOutOfRange")
                    return HttpStatusCode.BadRequest; // 400
            }

            if (exception is BusinessException)
                return HttpStatusCode.BadRequest;

            return base.GetStatusCode(httpContext, exception);
        }

    }
}
