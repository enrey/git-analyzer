using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Analyzer.Jira.Application.Configuration;
using Analyzer.Jira.Application.Services;

namespace JiraAnalyzer.Web.Api
{
    public class ServicesConfigurator
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<JiraService>();
            services.AddTransient<JiraLoader>();
            services.AddTransient<DashService>();

            services.Configure<JiraConfig>(configuration.GetSection("JiraConfig"));
        }
    }
}
