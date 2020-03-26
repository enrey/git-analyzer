using GitAnalyzer.Application.Configuration;
using GitAnalyzer.Application.Services.GitLab;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace GitAnalyzer.IntegrationTests
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
        public async Task GetMergeRequestsStatistics_Success()
        {
            //Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;
            var service = GetService();

            var requests = await service.GetMergeRequestsStatistics(startDate, endDate);
        }

        private IGitLabService GetService()
        {
            var configuration = InitTestConfiguration();
            var services = new ServiceCollection();
            services.Configure<RepositoriesConfig>(configuration.GetSection("Repositories"));
            services.Configure<GitLabConfig>(configuration.GetSection("GitLab"));
            var builder = services.BuildServiceProvider();

            return new GitLabService(
                builder.GetService<IOptionsMonitor<GitLabConfig>>(),
                builder.GetService<IOptionsMonitor<RepositoriesConfig>>()
                );
        }

        private IConfiguration InitTestConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();
            return config;
        }
    }
}
