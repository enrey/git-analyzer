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

        private readonly ElasticConfig _elasticConfig;
        private readonly IGitStatisticsService _gitStatisticsService;
        private readonly ILogger<GitElasticService> _logger;

        public GitElasticService(IOptionsMonitor<ElasticConfig> elasticConfig, IGitStatisticsService gitStatisticsService, ILogger<GitElasticService> logger)
        {
            _elasticConfig = elasticConfig.CurrentValue;
            _gitStatisticsService = gitStatisticsService;
            _logger = logger;
        }

        public IList<PersonStatisticsResultDto> GetInfo(DateTimeOffset from, DateTimeOffset till)
        {
            var client = GetClient();

            var collection = client.Search<PersonStatisticsStoreDto>(s => s
                .From(0)
                .Size(MAX_ROWS)
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.CommitDate)
                        .GreaterThanOrEquals(from.DateTime)
                        .LessThanOrEquals(till.DateTime)
                    )
                )
            ).Documents;

            var grouped = collection.GroupBy(o => new { o.RepositoryName, o.WebUI, o.Date, o.Email, o.Name })
                .Select(o => new PersonStatisticsResultDto
                {
                    RepositoryName = o.Key.RepositoryName,
                    WebUI = o.Key.WebUI,
                    Date = o.Key.Date,
                    Email = o.Key.Email.ToLower(),
                    Name = o.Key.Name,
                    CommitsCount = o.Count(),
                    Added = o.Sum(o => o.Added),
                    Deleted = o.Sum(o => o.Deleted),
                    Total = o.Sum(o => o.Total),
                    CommitsArray = o.Select(i => new PersonStatisticsCommitResultDto
                    {
                        CommitDate = i.CommitDate,
                        Message = i.Message,
                        Sha = i.Sha,
                        Added = i.Added,
                        Deleted = i.Deleted,
                        Total = i.Added + i.Deleted
                    }).ToList()
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
            var dtFrom = DateTimeOffset.Now.AddDays(-30);
            var dtTill = DateTimeOffset.Now;

            var client = GetClient();
            var result = _gitStatisticsService.GetAllRepositoriesStatisticsAsync(dtFrom, dtTill).Result;

            var storeDto = result.SelectMany(o => o.Statistics.Select(i => new PersonStatisticsStoreDto
            {
                Date = o.Date,
                RepositoryName = o.RepositoryName,
                WebUI = o.WebUI,
                Email = i.Email,
                Name = i.Name,
                CommitDate = i.CommitDate,
                Message = i.Message,
                Added = i.Added,
                Deleted = i.Deleted,
                Sha = i.Sha,
                Total = i.Total
            }
            )).ToList();

            var response = client.DeleteByQuery<PersonStatisticsStoreDto>(q => q
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.CommitDate)
                        .GreaterThanOrEquals(dtFrom.Date)
                        .LessThanOrEquals(dtTill.Date)
            )));

            foreach (var a in storeDto.ToList())
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
