using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Analyzer.Git.Application.Services.Statistics;
using Analyzer.Git.Application.Configuration;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Analyzer.Git.IntegrationTests
{
    public class GitStatisticsServiceTests
    {
        IGitStatisticsService _service;
        LoggerMock _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new LoggerMock();

            var builder = GetConfig();
            var repoConfig = builder.GetService<IOptionsMonitor<RepositoriesConfig>>();
            var statConfig = builder.GetService<IOptionsMonitor<StatisticsConfig>>();
            var estConfig = builder.GetService<IOptionsMonitor<WorkEstimateConfig>>();

            _service = new GitStatisticsService(_loggerMock, statConfig, repoConfig, estConfig);
        }

        [Test]
        public async Task UpdateAllRepositories_Test()
        {
            //Act
            await _service.UpdateAllRepositories();

            //Assert
            Assert.IsNull(_loggerMock.Error, _loggerMock.Error);
        }

        [Test]
        public async Task GetAllRepositoriesStatistics_Test()
        {
            // Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            // Act
            var result = await _service.GetAllRepositoriesStatisticsAsync(startDate, endDate);

            // Assert 
            Assert.IsTrue(result.Count() > 0);
        }

        [Test]
        public async Task GetWorkSessionsEstimate_Test()
        {
            // Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            // Act
            var result = await _service.GetWorkSessionsEstimate(startDate, endDate);

            // Assert 
            Assert.IsTrue(result.Count() > 0);
        }

        private ServiceProvider GetConfig()
        {
            var configuration = LoadTestConfiguration();

            var services = new ServiceCollection();
            services.Configure<RepositoriesConfig>(configuration.GetSection("Repositories"));
            services.Configure<StatisticsConfig>(configuration.GetSection("Statistics"));
            services.Configure<WorkEstimateConfig>(configuration.GetSection("WorkEstimate"));
            var builder = services.BuildServiceProvider();

            return builder;
        }

        private IOptionsMonitor<RepositoriesConfig> GetMockConfig()
        {
            var config = new RepositoriesConfig
            {
                ReposFolder = "-",
                MergeUserName = "-",
                MergeUserEmail = "-",
                ReposInfo = new[]
                {
                    new RepositoryInfoConfig
                    {
                        Name = "-",
                        Url = "-",
                        LocalPath = "-",
                        Username = "-",
                        Password = "-"
                    },
                }
            };
            return Mock.Of<IOptionsMonitor<RepositoriesConfig>>(_ => _.CurrentValue == config);
        }

        private IConfiguration LoadTestConfiguration()
        {
            var path = Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("bin\\"));
            path += "../Analyzer.Git.Web.Api/";

            var config = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.Development.json")
                .Build();
            return config;
        }
    }
}