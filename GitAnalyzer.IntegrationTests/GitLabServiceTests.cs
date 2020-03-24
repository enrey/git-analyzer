using GitAnalyzer.Application.Services.GitLab;
using NUnit.Framework;
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
        public async Task GetMergeRequests_Success()
        {
            var requests = await _service.GetMergeRequests();
        }
    }
}
