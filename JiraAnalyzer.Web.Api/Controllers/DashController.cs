using JiraAnalyzer.Web.Api.Dto;
using JiraAnalyzer.Web.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace JiraAnalyzer.Web.Api.Controllers
{
    /// <summary> Информация по Jira </summary>
    [ApiController]
    [Route("api/dash")]
    public class JiraController : Controller
    {
        private const int TIMEOUT_SECONDS = 60 * 10;

        private readonly ILogger<JiraController> _logger;
        private readonly IMemoryCache _cache;
        private readonly JiraService _jiraService;

        /// <summary> Информация по Jira </summary>
        public JiraController(ILogger<JiraController> logger, IMemoryCache cache, JiraService jiraService)
        {
            _logger = logger;
            _cache = cache;
            _jiraService = jiraService;
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = TIMEOUT_SECONDS)]
        [ProducesResponseType(typeof(IEnumerable<Info>), StatusCodes.Status200OK)]
        public IActionResult Get(int? forDays = 20)
        {
            return _cache.GetOrCreate("dash" + forDays,
                cacheEntry => {
                    cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(TIMEOUT_SECONDS);

                    // var days = "-20d";
                    var days = $"-{forDays}d"; 
                    _logger.LogInformation($"Querying for {days}");

                    var collection = _jiraService.GetJiraInfo(days);

                    _logger.LogInformation("Ended");
                    return Json(collection);
                });        
 
        }

        [HttpGet("{startDate}/{endDate}")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = TIMEOUT_SECONDS)]
        [ProducesResponseType(typeof(IEnumerable<Info>), StatusCodes.Status200OK)]
        public IActionResult Get(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return _cache.GetOrCreate("dash" + startDate.DateTime.ToShortDateString() + "/" + endDate.DateTime.ToShortDateString(),
                cacheEntry => {
                    cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(TIMEOUT_SECONDS);

                    var collection = _jiraService.GetJiraInfo(startDate, endDate);

                    _logger.LogInformation("Ended");
                    return Json(collection);
                });

        }
    }
}
