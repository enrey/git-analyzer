using Analyzer.Git.Application.Configuration;
using Analyzer.Git.Application.Dto.GitLab;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Analyzer.Gitlab.Application.Services
{
    public class GitLabElasticService
    {
        private const string INDEX_NAME = "gitlab";

        private const int MAX_ROWS = 10000;

        private readonly GitLabConfig _gitLabConfig;
        private readonly ElasticConfig _elasticConfig;
        private readonly IGitLabService _gitLabService;
        private readonly ILogger<GitLabElasticService> _logger;

        public GitLabElasticService(IOptionsMonitor<GitLabConfig> gitLabConfig, IOptionsMonitor<ElasticConfig> elasticConfig, IGitLabService gitLabService, ILogger<GitLabElasticService> logger)
        {
            _gitLabConfig = gitLabConfig.CurrentValue;
            _elasticConfig = elasticConfig.CurrentValue;
            _gitLabService = gitLabService;
            _logger = logger;
        }

        public IList<CommentsStatisicsDto> GetMergeRequestsCommentsStatistics(DateTimeOffset from, DateTimeOffset till)
        {
            var settings = new ConnectionSettings(new Uri(_elasticConfig.ElasticSearchUrl)).DefaultIndex(INDEX_NAME);
            var client = new ElasticClient(settings);

            var collection = client.Search<DateAndIdItemStore>(s => s
                .From(0)
                .Size(MAX_ROWS)
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.Dt)
                        .GreaterThanOrEquals(from.DateTime)
                        .LessThanOrEquals(till.DateTime)
                    )
                )
            ).Documents;

            return MapCommentsBack(collection.Where(o => o.EventType == EventType.Comment).ToList());
        }

        public IList<UserMergeRequestsStatisicsDto> GetMergeRequestsStatistics(DateTimeOffset from, DateTimeOffset till)
        {
            var settings = new ConnectionSettings(new Uri(_elasticConfig.ElasticSearchUrl)).DefaultIndex(INDEX_NAME);
            var client = new ElasticClient(settings);

            var collection = client.Search<DateAndIdItemStore>(s => s
                .From(0)
                .Size(MAX_ROWS)
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.Dt)
                        .GreaterThanOrEquals(from.DateTime)
                        .LessThanOrEquals(till.DateTime)
                    )
                )
            ).Documents;

            return MapBack(collection.Where(o => o.EventType != EventType.Comment).ToList());
        }

        private IList<CommentsStatisicsDto> MapCommentsBack(List<DateAndIdItemStore> list)
        {
            return list.GroupBy(o => new { o.Email, o.Username }).Select(o => new CommentsStatisicsDto
            {
                Email = o.Key.Email,
                Username = o.Key.Username,
                TotalComments = o.Count(),
                Items = o.Select(k => new CommentsStatisicsItemDto { Dt = k.Dt, Comment = k.Comment /*, Username = k.Username */}).ToList(),

            }).ToList();
        }

        private IList<UserMergeRequestsStatisicsDto> MapBack(List<DateAndIdItemStore> list)
        {
            return list.GroupBy(o => new { o.Email, o.Username }).Select(o => new UserMergeRequestsStatisicsDto
            {
                Email = o.Key.Email,
                Username = o.Key.Username,
                OpenedTotal = o.Where(i => i.EventType == EventType.Opened).Count(),
                MergedTotal = o.Where(i => i.EventType == EventType.Merged).Count(),
                OpenedDates = o.Where(i => i.EventType == EventType.Opened).Select(k => new DateAndIdItem { Dt = k.Dt, Iid = k.Iid, Repo = k.Repo, Url = k.Url, Title = k.Title }).ToList(),
                MergedDates = o.Where(i => i.EventType == EventType.Merged).Select(k => new DateAndIdItem { Dt = k.Dt, Iid = k.Iid, Repo = k.Repo, Url = k.Url, Title = k.Title }).ToList(),
            }).ToList();
        }

        private DateAndIdItemStore GetStoreItem(UserMergeRequestsStatisicsDto dto, DateAndIdItem item, EventType eventType)
        {
            return new DateAndIdItemStore
            {
                Username = dto.Username,
                Email = dto.Email,
                EventType = eventType,
                Dt = item.Dt,
                Iid = item.Iid,
                Repo = item.Repo,
                Url = item.Url,
                Title = item.Title
            };
        }
        private IEnumerable<DateAndIdItemStore> GetStoreItems(IEnumerable<UserMergeRequestsStatisicsDto> items)
        {
            foreach (var a in items)
            {
                foreach (var mr in a.MergedDates)
                {
                    yield return GetStoreItem(a, mr, EventType.Merged);
                }
                foreach (var mr in a.OpenedDates)
                {
                    yield return GetStoreItem(a, mr, EventType.Opened);
                }
            }
        }

        private IEnumerable<DateAndIdItemStore> GetStoreCommentItems(IEnumerable<CommentsStatisicsDto> items)
        {
            foreach (var a in items)
            {
                foreach (var dto in a.Items)
                {
                    yield return new DateAndIdItemStore
                    {
                        Username = a.Username,
                        Email = a.Email,
                        EventType = EventType.Comment,
                        Dt = dto.Dt,
                        Comment = dto.Comment
                    };
                }
            }
        }


        public void Update()
        {
            var dtFrom = DateTimeOffset.Now.AddDays(-30).Date;
            var dtTill = DateTimeOffset.Now.Date;

            var result = _gitLabService.GetMergeRequestsStatistics(dtFrom, dtTill).Result;
            var items = GetStoreItems(result).ToList();

            var resultComments = _gitLabService.GetMergeRequestsCommentsStatistics(dtFrom, dtTill).Result;
            items = items.Concat(GetStoreCommentItems(resultComments)).ToList();

            var settings = new ConnectionSettings(new Uri(_elasticConfig.ElasticSearchUrl)).DefaultIndex(INDEX_NAME);

            var client = new ElasticClient(settings);
            var response = client.DeleteByQuery<DateAndIdItemStore>(q => q
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.Dt)
                        .GreaterThanOrEquals(dtFrom)
                        .LessThanOrEquals(dtTill)
                        )
                    )
            );

            foreach (var a in items)
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
