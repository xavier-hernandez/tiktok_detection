using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
using System.Runtime.InteropServices;

namespace TikTokDetection.Helpers
{
    internal class Settings
    {
        private static readonly ILogger logger = LoggerInitializer.CreateLogger();
        public string[] apprise { get; set; }
        public string[] tiktok { get; set; }
        public int timeout { get; set; }
        public double? similarityTrigger { get; set; }

        public static string settingsDir = string.Empty;
        public static string screenshotsDir = string.Empty;
        public static string settingsFileName = string.Empty;

        public Settings() { }
        public Settings LoadSettings()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                settingsDir = "/assets/";
                screenshotsDir = Path.Combine(settingsDir, "screenshots/");
                settingsFileName = Path.Combine(settingsDir, "settings.json");
            }
            else
            {
                settingsDir = Directory.GetCurrentDirectory();
                screenshotsDir = Path.Combine(settingsDir, "screenshots\\");
                settingsFileName = Path.Combine(settingsDir, "settings.json");
            }


            if (!Directory.Exists(settingsDir))
            {
                logger.LogError("No assets directory mounted");
                Environment.Exit(1);
            }

            if (!Directory.Exists(settingsDir))
            {
                logger.LogError("No assets directory mounted");
                Environment.Exit(1);
            }

            try
            {
                if (!Directory.Exists(screenshotsDir))
                {
                    Directory.CreateDirectory(screenshotsDir);
                }
            }
            catch {
                logger.LogError("Error creating screenshots direcotry within assets directory; check permissions");
                Environment.Exit(1);
            }

            Settings settings = new();
            if (!File.Exists(settingsFileName))
            {
                logger.LogInformation("No settings.json; notifications will just appear in the console/logs");
            }
            else
            {
                logger.LogInformation("Loading settings.json");
                string jsonText = File.ReadAllText(settingsFileName);
                try
                {
                    settings = JsonConvert.DeserializeObject<Settings>(jsonText);
                    apprise = settings.apprise;
                    tiktok = settings.tiktok;
                    timeout = settings.timeout == 0 ? 30 : settings.timeout;
                    similarityTrigger = settings.similarityTrigger ?? 100;
                }
                catch (Exception ex)
                {
                    logger.LogInformation("Trouble reading settings.json; check formatting");
                    logger.LogError(ex.Message);
                }
            }

            return settings;
        }

        public IEnumerator GetEnumerator()
        {
            yield return apprise;
            yield return tiktok;
        }

        public static bool ValidTikTokURL(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return false;
            }

            return uri.Host.Contains("tiktok.com", StringComparison.OrdinalIgnoreCase);
        }

        public static string SanitizeFileName(string url)
        {
            return Path.Combine(screenshotsDir, SanitizeUserName(url));
        }

        public static string SanitizeUserName(string url)
        {
            Uri uri = new Uri(url);
            string[] segments = uri.Segments;
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("", segments[segments.Length - 1].Split(invalidChars));
        }
    }
}


