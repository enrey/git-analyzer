using GitAnalyzer.Web.Application.Configuration;
using GitAnalyzer.Web.Application.Statistics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

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
                ReposFolder = "C:\\GitAnalyzer.Test\\Repos",
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

            _service = new GitStatisticsService(loggerMock.Object, statisticsConfigMonitorMock.Object, repositoriesConfigMock.Object);
        }

        [Test]
        public void UpdateAllRepositories_Test()
        {
            _service.UpdateAllRepositories();
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
    }
}