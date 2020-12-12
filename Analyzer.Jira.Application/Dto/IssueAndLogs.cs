using Atlassian.Jira;
using System.Collections.Generic;

namespace Analyzer.Jira.Application.Dto
{
    public class IssueAndLogs
    {
        public Issue Issue { get; set; }

        public List<IssueChangeLog> Logs { get; set; }
    }
}
