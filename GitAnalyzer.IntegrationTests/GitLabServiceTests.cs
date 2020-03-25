using GitAnalyzer.Application.Services.GitLab;
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
        IGitLabService _service;

        [SetUp]
        public void Setup()
        {
            _service = new GitLabService();
        }

        [Test]
        public async Task GetMergeRequestsStatistics_Success()
        {
            //Arrange
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            var requests = await _service.GetMergeRequestsStatistics(startDate, endDate);
        }
    }
}
