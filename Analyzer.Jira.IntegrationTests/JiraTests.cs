using Analyzer.Jira.Application.Configuration;
using Analyzer.Jira.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Analyzer.Jira.IntegrationTests
{
    public class JiraTests
    {
        IOptionsMonitor<JiraConfig> _jiraConfig;

        [SetUp]
        public void Setup()
        {
            _jiraConfig = GetConfig();
        }

        [Test]
        public void TestBetween()
        {
            // Arrange
            var jiraService = new JiraService(new JiraLoader(_jiraConfig), new DashService(), new LoggerMock());

            // Act
            var result = jiraService.GetJiraInfo(DateTimeOffset.Now.AddDays(-5), DateTimeOffset.Now);

            // Assert
            Assert.IsTrue(result.Count > 0);
        }

        [Test]
        public async Task TestDuring()
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(-5);
            var endDate = DateTime.Now;
            var loader = new JiraLoader(_jiraConfig);

            // Act
            var result = loader.GetIssuesDuring(startDate, endDate);
            await Task.WhenAll(result);

            // Assert
            Assert.IsTrue(result.Result.Count > 0);
        }

        private IOptionsMonitor<JiraConfig> GetConfig()
        {
            var configuration = LoadTestConfiguration();

            var services = new ServiceCollection();
            services.Configure<JiraConfig>(configuration.GetSection("JiraConfig"));
            var builder = services.BuildServiceProvider();

            return builder.GetService<IOptionsMonitor<JiraConfig>>();
        }

        private IOptionsMonitor<JiraConfig> GetMockConfig()
        {
            var config = new JiraConfig
            {
                Host = "-",
                Username = "-",
                Pwd = "-",
                UserGroupName = "-"
            };
            return Mock.Of<IOptionsMonitor<JiraConfig>>(_ => _.CurrentValue == config);
        }

        private IConfiguration LoadTestConfiguration()
        {
            var path = Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("bin\\"));
            path += "../Analyzer.Jira.Web.Api/";

            var config = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.Development.json")
                .Build();
            return config;
        }
    }
}