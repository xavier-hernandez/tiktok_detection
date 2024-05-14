using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System.ComponentModel.DataAnnotations;

namespace TikTokDetection.Helpers
{
    internal static class LoggerInitializer
    {
        public static ILogger CreateLogger()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
            });

            return loggerFactory.CreateLogger<Program>();
        }
    }
}
