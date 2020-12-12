using Analyzer.Jira.Application.Configuration;
using Atlassian.Jira;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analyzer.Jira.Application.Services
{
    public class JiraLoader
    {
        private const int USER_STEP = 50;
        private const int ISSUE_STEP = 200;
        private readonly JiraConfig _jiraConfig;
        private readonly Atlassian.Jira.Jira _jiraClient;

        public JiraLoader(IOptionsMonitor<JiraConfig> jiraConfig)
        {
            _jiraConfig = jiraConfig.CurrentValue;
            _jiraClient = GetClient();
        }

        public IList<JiraUser> GetAllUsers()
        {
            var users = GetUsersAsync();
            users.Wait();

            return users.Result;
        }

        public IList<Issue> GetIssuesDaysBack(string days)
        {
            var task = GetIssuesAsync(@"status was in (Разработка, ""На кодревью"", Переоткрыто) after startOfDay(" + days + ") ORDER BY status DESC");
            task.Wait();

            var issues = task.Result;

            return issues;
        }

        public async Task<IList<Issue>> GetIssuesDuring(DateTimeOffset from, DateTimeOffset till)
        {
            var strFrom = from.ToString("yyyy-MM-dd");
            var strTill = till.ToString("yyyy-MM-dd");

            return await GetIssuesAsync($"status was in (Разработка, \"На кодревью\", Переоткрыто) DURING(\"{strFrom}\", \"{strTill}\") ORDER BY status DESC");
        }

        private async Task<IList<Issue>> GetIssuesAsync(string query)
        {
            var startAt = 0;
            var ttlTask = SearchQuery(query, startAt, 1);

            await Task.WhenAll(ttlTask);

            var total = ttlTask.Result.TotalItems;
            Console.WriteLine("Total issues to retreive: " + total);

            var tasks = new List<Task<IPagedQueryResult<Issue>>>();
            while (startAt < total)
            {
                var task = SearchQuery(query, startAt, ISSUE_STEP);
                tasks.Add(task);
                Console.WriteLine("Task: " + startAt);

                startAt += ISSUE_STEP;
            }

            await Task.WhenAll(tasks);

            var result = tasks
                .Select(x => x.Result)
                .SelectMany(o => o)
                .ToList();

            return result;
        }

        private async Task<IPagedQueryResult<Issue>> SearchQuery(string query, int skip, int take)
        {
            return await _jiraClient.Issues.GetIssuesFromJqlAsync(query, startAt: skip, maxIssues: take);
        }

        public IList<JiraUser> GetAllUsersFromGroup()
        {
            var task = GetUsersInGroupAsync();
            task.Wait();
            return task.Result;
        }

        private async Task<IList<JiraUser>> GetUsersAsync()
        {
            var res = await _jiraClient.Users.SearchUsersAsync(".", maxResults: 1000);
            return res.ToList();
        }

        private async Task<IList<JiraUser>> GetUsersInGroupAsync()
        {
            var startAt = 0;
            var total = USER_STEP;

            var all = new List<JiraUser>();

            while (startAt < total)
            {
                var res = await _jiraClient.Groups.GetUsersAsync(_jiraConfig.UserGroupName, startAt: startAt, maxResults: USER_STEP);
                all.AddRange(res.ToList());

                startAt += USER_STEP;
                total = res.TotalItems;
            }

            return all;
        }

        private Atlassian.Jira.Jira GetClient()
        {
            return Atlassian.Jira.Jira.CreateRestClient(_jiraConfig.Host, _jiraConfig.Username, _jiraConfig.Pwd);
        }
    }
}