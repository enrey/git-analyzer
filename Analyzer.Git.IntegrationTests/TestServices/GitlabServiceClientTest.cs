using Analyzer.Git.Application.Configuration;
using Analyzer.Git.Application.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Analyzer.Git.IntegrationTests.TestServices
{
    public class GitlabServiceClientTest : IGitlabServiceClient
    {
        public Task<IEnumerable<RepositoryInfoConfig>> GetAllReposFromApi(DateTime lastActivityAfter)
        {
            return Task.FromResult<IEnumerable<RepositoryInfoConfig>>(new List<RepositoryInfoConfig>()
            {
                new RepositoryInfoConfig(){Url = "https://git.it2g.ru/apk_ums/apk_ums.git", WebUI = "https://git.it2g.ru/apk_ums/apk_ums", LocalPath = "apk_ums" }
            });
        }
    }
}
