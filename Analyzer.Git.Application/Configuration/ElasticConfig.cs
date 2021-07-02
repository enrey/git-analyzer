using System.Runtime.Serialization;

namespace Analyzer.Git.Application.Configuration
{
    public class ElasticConfig
    {
        public string ElasticSearchUrl { get; set; }

        public int UpdatePeriodMinutes { get; set; }
    }
}
