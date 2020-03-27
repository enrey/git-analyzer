using Atlassian.Jira;
using System;
using System.Collections.Generic;
using System.Text;

namespace JiraAnalyzer.Web.Api.Dto
{
    public class IssueAndLogs
    {
        public Issue Issue { get; set; }

        public List<IssueChangeLog> Logs { get; set; }
    }
}
