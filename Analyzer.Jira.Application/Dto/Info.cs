using System;

namespace JiraAnalyzer.Web.Api.Dto
{
    public class Info
    {
        public string AssigneeCurrent { get; set; }

        public string AssigneeCurrentId { get; set; }

        public string Assignee { get; set; }

        public string AssigneeEmail { get; set; }

        public string AssigneeId { get; set; }

        public int Days { get; set; }

        public int DaysBeforeTest { get; set; }

        public DateTime DaysDate { get; set; }

        public DateTime DaysBeforeTestDate { get; set; }

        public string Number { get; set; }

        public string Type { get; set; }

        public string Status { get; set; }

        public string Project { get; set; }

        public string Name { get; set; }

        public int OriginalEstimateHours { get; set; }
    }
}
