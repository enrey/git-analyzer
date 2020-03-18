using System.Collections.Generic;

namespace GitAnalyzer.Application.Dto.Statistics
{
    public class RepositoryWorkEstimateDto
    {
        public string RepositoryName { get; set; }

        public IEnumerable<PersonWorkEstimateDto> Estimates { get; set; }
    }
}
