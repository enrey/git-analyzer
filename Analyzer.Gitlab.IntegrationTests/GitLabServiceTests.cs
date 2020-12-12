using Analyzer.Git.Application.Configuration;
using Analyzer.Git.Application.Services.GitLab;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Analyzer.Gitlab.IntegrationTests
{
    /// <summary>
    /// Интеграционные тесты для сервиса <see cref="IGitLabService"/>
    /// </summary>
    public class GitLabServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task GetComments_Success()
        {
            // Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            var gitlabConfig = GetConfig();
            var service = new GitLabService(gitlabConfig);

            // Act
            var result = await service.GetMergeRequestsCommentsStatistics(startDate, endDate);

            // Assert
            Assert.IsTrue(result.Count() > 0);
        }

        [Test]
        public async Task GetMergeRequestsStatistics_Success()
        {
            // Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            var gitlabConfig = GetConfig();
            var service = new GitLabService(gitlabConfig);

            // Act
            var result = await service.GetMergeRequestsStatistics(startDate, endDate);

            // Assert
            Assert.IsTrue(result.Count() > 0);
        }

        private IOptionsMonitor<GitLabConfig> GetConfig()
        {
            var configuration = LoadTestConfiguration();

            var services = new ServiceCollection();
            services.Configure<GitLabConfig>(configuration.GetSection("GitLab"));
            var builder = services.BuildServiceProvider();

            return builder.GetService<IOptionsMonitor<GitLabConfig>>();
        }

        private IOptionsMonitor<GitLabConfig> GetMockConfig()
        {
            var config = new GitLabConfig { ApiUrl = "-", PrivateToken = "-" };
            return Mock.Of<IOptionsMonitor<GitLabConfig>>(_ => _.CurrentValue == config);
        }

        private IConfiguration LoadTestConfiguration()
        {
            var path = Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("bin\\"));
            path += "../Analyzer.GitLab.Web.Api/";

            var config = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.Development.json")
                .Build();
            return config;
        }
    }
}
