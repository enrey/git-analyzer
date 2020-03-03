using System.Collections.Generic;

namespace GitAnalyzer.Web.Contracts.Statistics
{
    public class RepositoryWorkEstimateContract
    {
        public string RepositoryName { get; set; }

        public IEnumerable<PersonWorkEstimateContract> Estimates { get; set; }
    }
}
