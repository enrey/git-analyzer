using Analyzer.Git.Application.Configuration;
using Analyzer.Git.Application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Analyzer.Git.Web.Api.Hosted
{
    /// <summary>
    /// Self-hosted сервис для обновления репозиториев
    /// </summary>
    public class UpdateRepositoriesHostedService : IHostedService
    {
        private Timer _timer;
        private readonly IGitStatisticsService _gitStatisticsService;
        private readonly RepositoriesConfig _repositoriesConfig;
        private readonly ILogger<UpdateRepositoriesHostedService> _logger;

        /// <summary>
        /// Self-hosted сервис для обновления репозиториев
        /// </summary>
        public UpdateRepositoriesHostedService(
            IGitStatisticsService gitStatisticsService,
            IOptionsMonitor<RepositoriesConfig> repositoriesConfig,
            ILogger<UpdateRepositoriesHostedService> logger)
        {
            _gitStatisticsService = gitStatisticsService;
            _repositoriesConfig = repositoriesConfig.CurrentValue;
            _logger = logger;
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
            try
            {
                _logger.LogInformation("Update repo started...");
                _gitStatisticsService.UpdateAllRepositories().Wait();
                _logger.LogInformation("Update repo success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UPDATE REPO FAILED");
            }
        }
    }
}
