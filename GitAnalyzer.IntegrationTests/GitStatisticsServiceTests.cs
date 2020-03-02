using GitAnalyzer.Web.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using GitAnalyzer.Web.Application.Services.Statistics;

namespace GitAnalyzer.IntegrationTests
{
    public class GitStatisticsServiceTests
    {
        IGitStatisticsService _service;

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger<GitStatisticsService>>();

            var statisticsConfigMonitorMock = new Mock<IOptionsMonitor<StatisticsConfig>>();
            statisticsConfigMonitorMock.Setup(cfg => cfg.CurrentValue).Returns(new StatisticsConfig { PeriodIntervalDays = 1 });

            var repositoriesConfigMock = new Mock<IOptionsMonitor<RepositoriesConfig>>();
            repositoriesConfigMock.Setup(cfg => cfg.CurrentValue).Returns(new RepositoriesConfig
            {
                ReposFolder = "C:\\Test.Repos",
                ReposInfo = new[]
                {
                    new RepositoryInfoConfig
                    {
                        Name = "¿‚ÚÓÍÓ‰",
                        Url = "http://git.it2g.ru/avtokod/siv.avtokod.git",
                        LocalPath = "siv.avtokod",
                        Username = "",
                        Password = ""
                    },
                    new RepositoryInfoConfig
                    {
                        Name = "Õœ¿",
                        Url = "http://git.it2g.ru/npa/npa.git",
                        LocalPath = "npa",
                        Username = "",
                        Password = ""
                    },
                    new RepositoryInfoConfig
                    {
                        Name = "–¬œ ¬Õ∆",
                        Url = "http://git.it2g.ru/mmc/rvp-vnj.git",
                        LocalPath = "rvp-vnj",
                        Username = "",
                        Password = ""
                    }
                }
            }); 

            var workEstimateConfigMock = new Mock<IOptionsMonitor<WorkEstimateConfig>>();
            workEstimateConfigMock.Setup(cfg => cfg.CurrentValue).Returns(new WorkEstimateConfig
            {
                WorkDayHours = 8,
                PaddingHours = 2
            });

            _service = new GitStatisticsService(
                loggerMock.Object, 
                statisticsConfigMonitorMock.Object, 
                repositoriesConfigMock.Object, 
                workEstimateConfigMock.Object);
        }

        [Test]
        public async Task UpdateAllRepositories_Test()
        {
            await _service.UpdateAllRepositories();
        }

        [Test]
        public async Task GetAllRepositoriesStatistics_Test()
        {
            //Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            //Act
            var result = await _service.GetAllRepositoriesStatisticsAsync(startDate, endDate);
        }

        [Test]
        public async Task GetWorkSessionsEstimate_Test()
        {
            //Arrange
            //var startDate = DateTime.Now.AddYears(-10);
            var startDate = new DateTime(2000, 1, 28);
            var endDate = DateTime.Now;

            //Act
            var result  = await _service.GetWorkSessionsEstimate(startDate, endDate);
        }
    }
}