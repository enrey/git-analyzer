using System.Collections.Generic;

namespace Analyzer.Git.Application.Dto
{
    public class RepositoryWorkEstimateDto
    {
        public string RepositoryName { get; set; }

        public IEnumerable<PersonWorkEstimateDto> Estimates { get; set; }
    }
}
