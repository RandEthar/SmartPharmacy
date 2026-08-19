using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.PL.Controllers
{
    /// <summary>
    /// The Hangfire dashboard authenticates with a cookie, but this API issues bearer tokens,
    /// so a browser navigating to /hangfire is always anonymous and always rejected. These
    /// endpoints expose the same schedule information over the authentication the API does have.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class BackgroundJobsController : ControllerBase
    {
        private readonly IRecurringJobManager _recurringJobManager;

        public BackgroundJobsController(IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
        }

        [HttpGet("recurring")]
        public IActionResult GetRecurringJobs()
        {
            using var connection = JobStorage.Current.GetConnection();

            var jobs = connection.GetRecurringJobs()
                .Select(job => new
                {
                    job.Id,
                    job.Cron,
                    job.TimeZoneId,
                    job.LastExecution,
                    job.NextExecution,
                    job.LastJobState
                });

            return Ok(jobs);
        }

        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            var statistics = JobStorage.Current.GetMonitoringApi().GetStatistics();

            return Ok(new
            {
                statistics.Enqueued,
                statistics.Scheduled,
                statistics.Processing,
                statistics.Succeeded,
                statistics.Failed,
                statistics.Recurring,
                Servers = statistics.Servers
            });
        }

        /// <summary>
        /// Runs a scheduled job immediately instead of waiting for its next occurrence, which is
        /// the only practical way to verify a daily job without waiting a day for it.
        /// </summary>
        [HttpPost("recurring/{jobId}/trigger")]
        public IActionResult TriggerRecurringJob(string jobId)
        {
            using var connection = JobStorage.Current.GetConnection();

            if (!connection.GetRecurringJobs().Any(job => job.Id == jobId))
                return NotFound(new { message = $"No recurring job registered with id '{jobId}'." });

            _recurringJobManager.Trigger(jobId);

            return Ok(new { message = $"Job '{jobId}' was queued for immediate execution." });
        }
    }
}
