using Analyzer.Jira.Application.Dto;
using Atlassian.Jira;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Analyzer.Jira.Application.Services
{

    public class JiraService
    {
        private const int PARALLEL_DEGREE = 10;

        private readonly JiraLoader _jiraLoader;
        private readonly DashService _dashService;
        private readonly ILogger<JiraService> _logger;

        public JiraService(JiraLoader jiraLoader, DashService dashService, ILogger<JiraService> logger)
        {
            _jiraLoader = jiraLoader;
            _dashService = dashService;
            _logger = logger;
        }

        public IList<Info> GetJiraInfo(string days)
        {
            var issues = _jiraLoader.GetIssuesDaysBack(days);
            _logger.LogInformation($"Total issues: {issues.Count}");

            return Combine(issues);
        }

        public IList<Info> GetJiraInfo(DateTimeOffset from, DateTimeOffset till)
        {
            var issues = _jiraLoader.GetIssuesDuring(from, till).Result;
            _logger.LogInformation($"Total issues: {issues.Count}");

            return Combine(issues);
        }

        public IList<User> GetUsers()
        {
            var users = _jiraLoader.GetAllUsersFromGroup();

            return users;
        }

        private IList<Info> Combine(IList<Issue> issues)
        {
            var issuesAndLogs = GetLogs(issues).ToList();
            _logger.LogInformation($"Total issues with logs: {issuesAndLogs.Count}");

            var users = _jiraLoader.GetAllUsers().ToList();
            var collection = _dashService.GetDash(issuesAndLogs, users);
            return collection;
        }

        private IEnumerable<IssueAndLogs> GetLogs(IList<Issue> issues)
        {
            return issues.AsParallel()
                .WithDegreeOfParallelism(PARALLEL_DEGREE)
                .Select(issue => new IssueAndLogs { Issue = issue, Logs = issue.GetChangeLogsAsync().Result.ToList() })
                .ToList();
        }
    }
}
