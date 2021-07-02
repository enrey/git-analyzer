using Analyzer.Jira.Application.Configuration;
using Analyzer.Jira.Application.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Analyzer.Jira.Application.Services
{
    public class JiraElasticService
    {
        private const string INDEX_NAME = "jira";
        private const int MAX_ROWS = 10000;

        private readonly ElasticConfig _elasticConfig;
        private readonly JiraService _jiraService;
        private readonly ILogger<JiraElasticService> _logger;

        public JiraElasticService(IOptionsMonitor<ElasticConfig> elasticConfig, JiraService jiraService, ILogger<JiraElasticService> logger)
        {
            _elasticConfig = elasticConfig.CurrentValue;
            _jiraService = jiraService;
            _logger = logger;
        }

        public IList<Info> GetJiraInfo(DateTimeOffset from, DateTimeOffset till)
        {
            var settings = new ConnectionSettings(new Uri(_elasticConfig.ElasticSearchUrl)).DefaultIndex(INDEX_NAME);
            var client = new ElasticClient(settings);

            var collection = client.Search<Info>(s => s
                .From(0)
                .Size(MAX_ROWS)
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.DateStartDev)
                        .GreaterThanOrEquals(from.DateTime)
                        .LessThanOrEquals(till.DateTime)
                    )
                )
            ).Documents;

            var collection2 = client.Search<Info>(s => s
                .From(0)
                .Size(MAX_ROWS)
                .Query(q => q.Bool(b => b
                       .MustNot(m => m
                            .Exists(e => e
                                .Field(f => f.DateEndDev)
                           )
                       )
                       )
            )).Documents;

            //TODO: Update по номерам ишью?

            var res = collection.Concat(collection2).GroupBy(p => p.Number)
                    .Select(g => g.First())
                    .ToList();

            return res;
        }



        public void UpdateMonth()
        {
            var result = _jiraService.GetJiraInfo(DateTimeOffset.Now.AddDays(-30), DateTimeOffset.Now);

            var settings = new ConnectionSettings(new Uri(_elasticConfig.ElasticSearchUrl)).DefaultIndex(INDEX_NAME);
            var client = new ElasticClient(settings);

            client.DeleteByQuery<object>(del => del
               .Query(q => q.QueryString(qs => qs.Query("*")))
               );

            foreach (var a in result)
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
