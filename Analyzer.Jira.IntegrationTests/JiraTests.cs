using Atlassian.Jira;
using JiraAnalyzer.Web.Api;
using JiraAnalyzer.Web.Api.Services;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace JiraAnalyzer.IntegrationTests
{
    public class JiraTests
    {
        LoggerMock _loggerMock;
        Mock<IOptionsMonitor<JiraConfig>> _jiraConfig;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new LoggerMock();
            _jiraConfig = new Mock<IOptionsMonitor<JiraConfig>>();
        }

        [Test]
        public void Test1()
        {
            var jiraService = new JiraService(new JiraLoader(_jiraConfig.Object), new DashService(), new LoggerMock());
            var result = jiraService.GetJiraInfo(DateTimeOffset.Now.AddDays(-20), DateTimeOffset.Now);
        }

        [Test]
        public void Test2()
        {
            var loader = new JiraLoader(_jiraConfig.Object);
            var result = loader.GetAllUsersFromGroup();

            Assert.AreEqual(68, result.Count);
        }
    }
}