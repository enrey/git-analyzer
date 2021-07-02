namespace Analyzer.Jira.Application.Configuration
{
    /// <summary>
    /// Конфигурация Jira
    /// </summary>
    public class JiraConfig
    {
        public string Host { get; set; }

        public string Username { get; set; }

        public string Pwd { get; set; }

        public string UserGroupName { get; set; }
    }
}
