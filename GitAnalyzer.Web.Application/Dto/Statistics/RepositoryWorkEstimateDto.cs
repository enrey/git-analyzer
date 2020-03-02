using System.Collections.Generic;

namespace GitAnalyzer.Web.Application.Dto.Statistics
{
    public class RepositoryWorkEstimateDto
    {
        public string RepositoryName { get; set; }

        public IEnumerable<PersonWorkEstimateDto> Estimates { get; set; }
    }
}
