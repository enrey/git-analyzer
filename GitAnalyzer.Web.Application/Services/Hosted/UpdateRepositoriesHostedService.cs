using GitAnalyzer.Web.Application.Configuration;
using GitAnalyzer.Web.Application.Services.Statistics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace GitAnalyzer.Web.Application.Services.Hosted
{
    /// <summary>
    /// Self-hosted сервис для обновления репозиториев
    /// </summary>
    public class UpdateRepositoriesHostedService : IHostedService
    {
        private Timer _timer;
        private readonly IGitStatisticsService _gitStatisticsService;
        private readonly RepositoriesConfig _repositoriesConfig;

        /// <summary>
        /// Self-hosted сервис для обновления репозиториев
        /// </summary>
        public UpdateRepositoriesHostedService(
            IGitStatisticsService gitStatisticsService,
            IOptionsMonitor<RepositoriesConfig> repositoriesConfig)
        {
            _gitStatisticsService = gitStatisticsService;
            _repositoriesConfig = repositoriesConfig.CurrentValue;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Update, null, 0, 1000 * 60 * _repositoriesConfig.ReposUpdatePeriodMinutes);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }

        private void Update(object state)
        {
            _gitStatisticsService.UpdateAllRepositories().Wait();
        }
    }
}
