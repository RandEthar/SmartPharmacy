using Hangfire.Dashboard;
using SmartPharmacy.DAL.Models;
using System.Net;

namespace SmartPharmacy.PL.Extentions
{
    /// <summary>
    /// Hangfire ships with an open dashboard by default, which would expose every background job
    /// (and the ability to trigger them) to anyone who knows the URL.
    /// </summary>
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly IWebHostEnvironment _environment;

        public HangfireDashboardAuthorizationFilter(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // The dashboard is a browser page and this API authenticates with a bearer header,
            // so in development it is reachable from the machine running the app only.
            if (_environment.IsDevelopment() && IsLocalRequest(httpContext))
            {
                return true;
            }

            return httpContext.User.Identity?.IsAuthenticated == true
                && httpContext.User.IsInRole(Roles.Admin);
        }

        private static bool IsLocalRequest(HttpContext httpContext)
        {
            var remoteIp = httpContext.Connection.RemoteIpAddress;
            if (remoteIp is null)
                return false;

            var localIp = httpContext.Connection.LocalIpAddress;
            return localIp is not null
                ? remoteIp.Equals(localIp)
                : IPAddress.IsLoopback(remoteIp);
        }
    }
}
