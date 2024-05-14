using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TikTokDetection.Helpers
{
    internal class Apprise
    {
        private static readonly ILogger logger = LoggerInitializer.CreateLogger();
        public static async Task SendNotificationsAsync(string service, string username)
        {
            await Task.Run(() =>
            {
                try
                {
                    string command = $"apprise -vv -t 'TikTok Detection' -b 'Change Found: {username}' '{service}'";

                    ProcessStartInfo startInfo = new()
                    {
                        FileName = "/bin/bash", // Specify bash as the program to run
                        Arguments = $"-c \"{command}\"", // Pass the command as an argument to bash
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = new Process())
                    {
                        process.StartInfo = startInfo;
                        process.Start();

                        string output = process.StandardOutput.ReadToEnd();
                        logger.LogInformation($"Sending notificaion - {service} - {output}");

                        process.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogInformation($"An error occurred sending notificaion - {service}: {ex.Message}");
                }
            });
        }
    }
}
