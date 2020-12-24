using Analyzer.Git.Application.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Analyzer.Git.Application.Services
{
    /// <summary>
    /// Сервис для работы с API GitLabService "http://localhost:5003/api/GitLab"
    /// </summary>
    public class GitlabServiceClient : IGitlabServiceClient
    {
       private readonly LocalServicesConfig _config;

        public GitlabServiceClient(IOptionsMonitor<LocalServicesConfig> config)
        {
            _config = config.CurrentValue;
        }

        /// <summary>
        /// Получить списко активных репозиториев начиная с указанной даты
        /// </summary>
        public async Task<IEnumerable<RepositoryInfoConfig>> GetAllReposFromApi(DateTime lastActivityAfter)
        {
            var configs = new List<RepositoryInfoConfig>();

            var _httpClient = new HttpClient { BaseAddress = new Uri(_config.Gitlab.BaseUrl) };
            var response = await _httpClient.GetAsync(_config.Gitlab.ActiveRepositories.Replace("sincedate", lastActivityAfter.Date.Date.ToString("yyyy-MM-dd")));
            var jsonString = await response.Content.ReadAsStringAsync();

            if(response.IsSuccessStatusCode && !string.IsNullOrEmpty(jsonString))
                configs.AddRange(JsonConvert.DeserializeObject<List<RepositoryInfoConfig>>(jsonString));

            return configs;
        }
    }
}
