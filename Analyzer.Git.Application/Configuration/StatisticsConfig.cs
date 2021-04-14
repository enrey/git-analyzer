using System.Runtime.Serialization;

namespace Analyzer.Git.Application.Configuration
{
    public class StatisticsConfig
    {
        public string ElasticSearchUrl { get; set; }

        public int UpdatePeriodMinutes { get; set; }
    }
}
