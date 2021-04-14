using Analyzer.Git.Application.Configuration;
using Analyzer.Git.Application.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Analyzer.Git.Application.Services
{
    public class GitElasticService
    {
        private const string INDEX_NAME = "git";
        private const int MAX_ROWS = 10000;

        private readonly StatisticsConfig _elasticConfig;
        private readonly IGitStatisticsService _gitStatisticsService;
        private readonly ILogger<GitElasticService> _logger;

        public GitElasticService(IOptionsMonitor<StatisticsConfig> elasticConfig, IGitStatisticsService gitStatisticsService, ILogger<GitElasticService> logger)
        {
            _elasticConfig = elasticConfig.CurrentValue;
            _gitStatisticsService = gitStatisticsService;
            _logger = logger;
        }

        public IList<RepositoryStatisticsDto> GetInfo(DateTimeOffset from, DateTimeOffset till)
        {
            var client = GetClient();

            var collection = client.Search<PeriodStatisticsDto>(s => s
                .From(0)
                .Size(MAX_ROWS)
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.Date)
                        .GreaterThanOrEquals(from.DateTime)
                        .LessThanOrEquals(till.DateTime)
                    )
                )
            ).Documents;

            var grouped = collection.GroupBy(o => new { o.RepositoryName, o.WebUI })
                .Select(o => new RepositoryStatisticsDto
                {
                    RepositoryName = o.Key.RepositoryName,
                    WebUI = o.Key.WebUI,
                    Periods = o.ToList()
                }).ToList();

            return grouped;
        }

        private ElasticClient GetClient()
        {
            var settings = new ConnectionSettings(new Uri(_elasticConfig.ElasticSearchUrl)).DefaultIndex(INDEX_NAME);
            var client = new ElasticClient(settings);

            var response = client.Ping();

            if (response.OriginalException != null)
            {
                throw new Exception("Недоступен Elasticsearch", response.OriginalException);
            }

            return client;
        }

        public void UpdateMonth()
        {
            var client = GetClient();

            var result = _gitStatisticsService.GetAllRepositoriesStatisticsAsync(DateTimeOffset.Now.AddDays(-30), DateTimeOffset.Now).Result;

            client.DeleteByQuery<object>(del => del
               .Query(q => q.QueryString(qs => qs.Query("*")))
               );

            foreach (var a in result.SelectMany(o => o.Periods).ToList())
            {
                var indexResponse = client.IndexDocument(a);
                if (!indexResponse.IsValid)
                {
                    _logger.LogError("Not valid document :(");
                    _logger.LogError(indexResponse.OriginalException.ToString());
                    _logger.LogError(indexResponse.DebugInformation.ToString());
                }
            }
        }
    }
}
