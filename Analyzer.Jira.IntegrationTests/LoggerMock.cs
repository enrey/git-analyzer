using JiraAnalyzer.Web.Api.Services;
using Microsoft.Extensions.Logging;
using System;

namespace JiraAnalyzer.IntegrationTests
{
    /// <summary>
    /// Mock для логгера
    /// </summary>
    public class LoggerMock : ILogger<JiraService>
    {
        public string Error { get; set; }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Error)
                Error = formatter(state, exception);
        }
    }
}