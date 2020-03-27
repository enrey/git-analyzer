using Atlassian.Jira;
using JiraAnalyzer.Web.Api.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraAnalyzer.Web.Api.Services
{

    public class DashService
    {
        public IList<Info> GetDash(IList<IssueAndLogs> issues, List<JiraUser> users)
        {
            var infos = new List<Info>();

            foreach (var issueAndLogs in issues)
            {
                var issue = issueAndLogs.Issue;

                // Выкидываем "готово для разработки" т.к. это часто правленые нестандартные фичи которые по кругу вернулись и перенеслись на другого дева
                if (issue.Status.Name == "Готова для разработки" || issue.Status.Name == "Ожидает разработки")
                {
                    continue;
                }

                // Фильтруем историю изменения выкидывая лишнее
                var featureLogStatuses = issueAndLogs.Logs.Where(o => o.Items.Any(i => i.FieldName == "status" || i.FieldName == "assignee")).ToList();

                // Ищем последний статус "Разработка" с конца
                var inDevelopEntry = featureLogStatuses.Where(o => o.Items.Any(i => i.ToValue == "Разработка")).LastOrDefault();

                // Странные старые статусы "в работе" нестандартный workflow
                if (inDevelopEntry == null)
                {
                    continue;
                }

                // Ищем последний статус "Передано в тестирование" с конца
                var inTestEntry = featureLogStatuses.Where(o => o.Items.Any(i => i.ToValue == "Готово к тестированию")).LastOrDefault();

                // TODO: Для сабтасок добавить закрыт
                if (inTestEntry == null)
                {
                    inTestEntry = featureLogStatuses.Where(o => o.Items.Any(i => i.ToValue == "Закрыт")).LastOrDefault();
                }

                var daysInWork = 0;

                // Если тестинг меньше разработки то какой-нить мусор с утверждением или что-то в этом роде
                if (inTestEntry != null && inTestEntry.CreatedDate > inDevelopEntry.CreatedDate)
                {
                    daysInWork = GetWorkingDays(inDevelopEntry.CreatedDate, inTestEntry.CreatedDate);
                }
                else
                {
                    daysInWork = GetWorkingDays(inDevelopEntry.CreatedDate, DateTime.Now);
                }

                var inDevelopAssignee = GetDevAssignee(users, issue, featureLogStatuses, inDevelopEntry);

                infos.Add(new Info
                {
                    AssigneeCurrent = GetCurrentIssueAssignee(users, issue),
                    Assignee = inDevelopAssignee,
                    AssigneeEmail = users.FirstOrDefault(o=>o.DisplayName == inDevelopAssignee)?.Email.ToLower(),
                    Days = (DateTime.Now - inDevelopEntry.CreatedDate).Days,
                    DaysDate = inDevelopEntry.CreatedDate,
                    DaysBeforeTest = daysInWork,
                    DaysBeforeTestDate = (inTestEntry != null) ? inTestEntry.CreatedDate : DateTime.MaxValue,
                    Status = issue.Status.Name,
                    Type = issue.Type.Name,
                    Number = issue.Key.Value,
                    Project = issue.Project,
                    Name = issue.Summary
                });
            }

            return infos;
        }


        private static int GetWorkingDays(DateTime from, DateTime to)
        {
            var dayDifference = (int)to.Subtract(from).TotalDays;
            return Enumerable
                .Range(1, dayDifference)
                .Select(x => from.AddDays(x))
                .Count(x => x.DayOfWeek != DayOfWeek.Saturday && x.DayOfWeek != DayOfWeek.Sunday);
        }

        private static List<IGrouping<string, Info>> GetInfos(List<Info> infos, bool byProject = false)
        {
            if (byProject)
                return infos.GroupBy(o => o.Project).ToList();

            return infos.GroupBy(o => "No Project").ToList();
        }


        private static string GetDevAssignee(List<JiraUser> devs, Issue issue, List<IssueChangeLog> featureLogStatuses, IssueChangeLog inDevelopEntry)
        {
            // если оно в разработки сразу берем асайни, т.к. есть странные переходы (дев передал другому деву без смены статуса)
            if (issue.Status == "Разработка")
                return GetCurrentIssueAssignee(devs, issue);

            // берем assignee последнего статуса перевода в разработку
            var inDevelopAssignee = inDevelopEntry.Items.SingleOrDefault(o => o.FieldName == "assignee")?.ToValue;

            // если не нашли - берем ближайшего предыдущего assignee, т.к. при переводе статуса он не всегда меняется
            if (inDevelopAssignee == null)
            {
                // идем назад ищем assignee
                string prevAssigneeFound = null;
                var index = featureLogStatuses.IndexOf(inDevelopEntry);
                while (index >= 0)
                {
                    prevAssigneeFound = featureLogStatuses[index].Items.SingleOrDefault(o => o.FieldName == "assignee")?.ToValue;
                    if (prevAssigneeFound != null) break;

                    index--;
                }
                inDevelopAssignee = prevAssigneeFound;

                // если не нашли, идем вперед и ищем момент когда разработчик сменился на тестеровщика
                if (prevAssigneeFound==null)
                {
                    string nextAssigneeFound = null;
                    var ind = featureLogStatuses.IndexOf(inDevelopEntry);
                    while (ind < featureLogStatuses.Count)
                    {
                        nextAssigneeFound = featureLogStatuses[ind].Items.SingleOrDefault(o => o.FieldName == "assignee")?.FromValue;
                        if (nextAssigneeFound != null) break;

                        ind++;
                    }
                    inDevelopAssignee = nextAssigneeFound;
                }
            }

            // assignee не менялся в истории, берем указанного при создании задачи
            if (inDevelopAssignee == null)
            {
                inDevelopAssignee = GetCurrentIssueAssignee(devs, issue);
            }

            if (inDevelopAssignee == null)
            {
                Console.WriteLine(issue.Key + " assignee not found");
            }

            return inDevelopAssignee;
        }

        private static string GetCurrentIssueAssignee(List<JiraUser> devs, Issue issue)
        {
            // текущий ассайни занулен зачем-то так делают
            if (issue.Assignee == null) return null;

            var fio = devs.SingleOrDefault(o => o.Username.ToLower() == issue.Assignee.ToLower() || o.Username.ToLower() == issue.Assignee.Replace("@it2g.ru", "").ToLower())?.DisplayName;

            return fio;
        }
    }
}