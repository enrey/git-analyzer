using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Analyzer.Jira.Application.Configuration;
using Analyzer.Jira.Application.Services;
using Analyzer.Jira.Web.Api.Hosted;

namespace JiraAnalyzer.Web.Api
{
    public class ServicesConfigurator
    {
        private const string JIRA_CONFIG = "JiraConfig";

        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<JiraService>();
            services.AddTransient<JiraElasticService>();
            services.AddTransient<JiraLoader>();
            services.AddTransient<DashService>();

            services.Configure<JiraConfig>(configuration.GetSection(JIRA_CONFIG));
            services.AddHostedService<ProducerHostingService>();
        }
    }
}
