using Analyzer.Jira.Application.Dto;
using Analyzer.Jira.Application.Services;
using Atlassian.Jira;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;

namespace JiraAnalyzer.Web.Api.Controllers
{
    /// <summary> Информация по Jira </summary>
    [ApiController]
    [Route("api/dash")]
    public class JiraController : Controller
    {
        private readonly ILogger<JiraController> _logger;
        private readonly JiraService _jiraService;
        private readonly JiraElasticService _jiraElasticService;

        /// <summary> Информация по Jira </summary>
        public JiraController(ILogger<JiraController> logger, JiraService jiraService, JiraElasticService jiraElasticService)
        {
            _logger = logger;
            _jiraService = jiraService;
            _jiraElasticService = jiraElasticService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Info>), StatusCodes.Status200OK)]
        public IActionResult Get(int? forDays = 20)
        {
            var days = $"-{forDays}d";
            _logger.LogInformation($"Querying for {days}");

            var collection = _jiraService.GetJiraInfo(days);

            _logger.LogInformation("Ended");
            return Json(collection);
        }

        [HttpGet("jira")]
        [ProducesResponseType(typeof(IEnumerable<Info>), StatusCodes.Status200OK)]
        public IActionResult Get(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            _logger.LogInformation($"Querying for {startDate.DateTime.ToShortDateString()} - {endDate.DateTime.ToShortDateString()}");

            var collection = _jiraElasticService.GetJiraInfo(startDate, endDate);

            _logger.LogInformation("Ended");
            return Json(collection);
        }

        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<JiraUser>), StatusCodes.Status200OK)]
        public IActionResult GetUsers()
        {
            var collection = _jiraService.GetUsers();

            return Json(collection);
        }

        [HttpPost("update-elastic")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Update()
        {
            _jiraElasticService.UpdateMonth();

            return Json("ok");
        }
    }
}
