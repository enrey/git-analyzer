using GitLabApiClient;
using GitLabApiClient.Models.MergeRequests.Requests;
using GitLabApiClient.Models.MergeRequests.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalyzer.Application.Services.GitLab
{
    /// <summary>
    /// Сервис для взаимодействия с GitLab
    /// </summary>
    public class GitLabService : IGitLabService
    {

        /// <summary>
        /// Сервис для взаимодействия с GitLab
        /// </summary>
        public GitLabService()
        {
        }

        public async Task<IEnumerable<MergeRequest>> GetMergeRequests()
        {
            var baseUrl = "http://git.it2g.ru/api/v4";
            var privateToken = "syxZ4jwyUqEDL99v_EUx";

            var client = new GitLabClient(baseUrl, privateToken);

            var requests = await client.MergeRequests.GetAsync(options =>
            {
                //options.
                options.State = QueryMergeRequestState.All;
            });

            //тест
            var r = requests.First(r => r.Id == 11989);

            return requests;
        }

    }
}
