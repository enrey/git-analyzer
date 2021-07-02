using Analyzer.Jira.Application.Configuration;
using Analyzer.Jira.Application.Dto;
using Analyzer.Jira.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Nest;
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
        public void GetFromElastic()
        {
            var settings = new ConnectionSettings(new Uri("http://127.0.0.1:9200")).DefaultIndex("jira");
            var client = new ElasticClient(settings);
            //var res = client.Search<Info>(s => s    .MatchAll());

            var res = client.Search<Info>(s => s.From(0)
                         .Size(10000).Query(q => q
        .DateRange(r => r
            .Field(f => f.DateStartDev)
            .GreaterThanOrEquals(new DateTime(2021, 01, 01))
        //.LessThan(new DateTime(2018, 01, 01))
                    )
                )
            );


        }


        [Test]
        public void TestBetween()
        {
            // Arrange
            var jiraService = new JiraService(new JiraLoader(_jiraConfig), new DashService(_jiraConfig), new LoggerMock());

            // Act
            var result = jiraService.GetJiraInfo(DateTimeOffset.Now.AddDays(-30), DateTimeOffset.Now);

            var settings = new ConnectionSettings(new Uri("http://127.0.0.1:9200")).DefaultIndex("jira");
            var client = new ElasticClient(settings);

            client.DeleteByQuery<object>(del => del
                .Query(q => q.QueryString(qs => qs.Query("*"))));

            //var client = new ElasticClient();
            var r = client.Ping();

            foreach (var a in result)
            {
                var indexResponse = client.IndexDocument(a);
                if (!indexResponse.IsValid)
                {
                    // If the request isn't valid, we can take action here
                }
            }


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