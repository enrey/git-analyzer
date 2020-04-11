using Atlassian.Jira;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraAnalyzer.Web.Api.Services
{
    public class JiraLoader
    {
        private const int USER_STEP = 50;
        private const int ISSUE_STEP = 100;
        private readonly JiraConfig _jiraConfig;

        public JiraLoader(IOptionsMonitor<JiraConfig> jiraConfig)
        {
            _jiraConfig = jiraConfig.CurrentValue;
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

        public IList<Issue> GetIssuesDuring(DateTimeOffset from, DateTimeOffset till)
        {
            var strFrom = from.ToString("yyyy-MM-dd");
            var strTill = till.ToString("yyyy-MM-dd");

            var task = GetIssuesAsync($"status was in (Разработка, \"На кодревью\", Переоткрыто) DURING(\"{strFrom}\", \"{strTill}\") ORDER BY status DESC");
            task.Wait();

            var issues = task.Result;

            return issues;
        }

        public List<IssueChangeLog> GetLogs(Issue issue)
        {
            var task = GetLog(issue);
            task.Wait();
            return task.Result;
        }

        private async Task<IList<Issue>> GetIssuesAsync(string query)
        {
            var startAt = 0;
            var total = ISSUE_STEP;

            var all = new List<Issue>();

            while (startAt < total)
            {
                var res = await SearchQuery(query, startAt, ISSUE_STEP);

                all.AddRange(res.ToList());

                startAt += ISSUE_STEP;
                total = res.TotalItems;
            }

            return all;
        }

        private async Task<IPagedQueryResult<Issue>> SearchQuery(string query, int skip, int take)
        {
            return await GetClient().Issues.GetIssuesFromJqlAsync(query, startAt: skip, maxIssues: take);
        }

        public IList<JiraUser> GetAllUsersFromGroup()
        {
            var task = GetUsersInGroupAsync();
            task.Wait();
            return task.Result;
        }

        private async Task<IList<JiraUser>> GetUsersAsync()
        {
            var res = await GetClient().Users.SearchUsersAsync(".", maxResults: 1000);
            return res.ToList();
        }

        private async Task<IList<JiraUser>> GetUsersInGroupAsync()
        {
            var startAt = 0;
            var total = USER_STEP;

            var all = new List<JiraUser>();

            while (startAt < total)
            {
                var res = await GetClient().Groups.GetUsersAsync(_jiraConfig.UserGroupName, startAt: startAt, maxResults: USER_STEP);
                all.AddRange(res.ToList());

                startAt += USER_STEP;
                total = res.TotalItems;
            }

            return all;
        }

        private Jira GetClient()
        {
            return Jira.CreateRestClient(_jiraConfig.Host, _jiraConfig.Username, _jiraConfig.Pwd);
        }

        private static async Task<List<IssueChangeLog>> GetLog(Issue issue)
        {
            return (await issue.GetChangeLogsAsync()).ToList();
        }
    }
}