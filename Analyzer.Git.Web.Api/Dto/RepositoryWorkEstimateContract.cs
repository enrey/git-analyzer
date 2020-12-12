using System.Collections.Generic;

namespace Analyzer.Git.Web.Api.Dto
{
    public class RepositoryWorkEstimateContract
    {
        public string RepositoryName { get; set; }

        public IEnumerable<PersonWorkEstimateContract> Estimates { get; set; }
    }
}
