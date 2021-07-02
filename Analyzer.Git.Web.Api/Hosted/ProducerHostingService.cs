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
    public class ProducerHostingService : IHostedService
    {
        private Timer _timer;
        private readonly GitElasticService _gitElasticService;
        private readonly ElasticConfig _statisticsConfig;
        private readonly ILogger<ProducerHostingService> _logger;

        /// <summary>
        /// Self-hosted сервис ETL в еластик
        /// </summary>
        public ProducerHostingService(ILogger<ProducerHostingService> logger, GitElasticService gitElasticService, IOptionsMonitor<ElasticConfig> statisticsConfig)
        {
            _gitElasticService = gitElasticService;
            _statisticsConfig = statisticsConfig.CurrentValue;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var timeLag = 1000 * 60 * _statisticsConfig.UpdatePeriodMinutes;
            _timer = new Timer(Update, null, timeLag, timeLag);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }

        private void Update(object state)
        {
            _logger.LogInformation("ETL to Elastic Started...");
            try
            {
                _gitElasticService.UpdateMonth();
                _logger.LogInformation("ETL to Elastic Ended.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ETL to Elastic ERROR!");

            }
        }
    }
}
