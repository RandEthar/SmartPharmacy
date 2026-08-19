using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartPharmacy.PL.Controllers
{
    /// <summary>
    /// Cheap anonymous endpoint for the external uptime pinger. Shared hosting stops the
    /// application pool after a period with no traffic, which also stops the in-process
    /// Hangfire server, so a scheduled job only fires if something keeps the app awake.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new
        {
            status = "healthy",
            utc = DateTime.UtcNow
        });
    }
}
