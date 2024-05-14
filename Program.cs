using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System.Net.Http;
using System.Runtime.InteropServices;
using TikTokDetection.Helpers;

namespace TikTokDetection
{
    internal class Program
    {
        private static readonly ILogger logger = LoggerInitializer.CreateLogger();

        public static void Main()
        {
            try
            {
                Settings s = new();
                var loadedSettings = s.LoadSettings();
                var timeout = s.timeout * 60000;

                while (true)
                {
                    Run(s).Wait();
                    Thread.Sleep(timeout);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred: {Message}", ex.Message);
            }
        }

        public static async Task Run(Settings s)
        {
            foreach (var tiktokUrl in s.tiktok)
            {
                if (Settings.ValidTikTokURL(tiktokUrl))
                {
                    //var downloadPath = "/chrome";
                    //var browserFetcherOptions = new BrowserFetcherOptions { Path = downloadPath };
                    //using var browserFetcher = new BrowserFetcher(browserFetcherOptions);
                    //var installedBrowser = await browserFetcher.DownloadAsync();

                    //await new BrowserFetcher().DownloadAsync();
                    //new BrowserFetcher().DownloadAsync().GetAwaiter().GetResult();

                    // Launch the browser
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                        {
                            Headless = true,
                            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" },
                            ExecutablePath = "/usr/local/bin/chrome"
                            //ExecutablePath = installedBrowser.GetExecutablePath()
                        });

                        using var page = await browser.NewPageAsync();

                        await page.GoToAsync(tiktokUrl, null, new[] { WaitUntilNavigation.Networkidle2 });

                        //debug
                        if (s.debug)
                        {
                            string htmlContent = await page.GetContentAsync();
                            Settings.SaveHtml(tiktokUrl, htmlContent);
                        }

                        await page.EvaluateExpressionAsync(@"document.querySelectorAll('[style*=""z-index: 1001""]').forEach(e => e.remove())");
                        await page.EvaluateExpressionAsync(@"document.querySelectorAll('div[class*=""DivHeaderWrapperMain-StyledDivHeaderWrapperMainV2""]').forEach(e => e.remove())");

                        var divSelector = "[class*=DivShareInfo]";
                        var divHandle = await page.WaitForSelectorAsync(divSelector);

                        string newFileName = Settings.SanitizeFileName(tiktokUrl + "_new.png");
                        string oldFileName = Settings.SanitizeFileName(tiktokUrl + "_old.png");

                        logger.LogInformation("New screenshot saved {tiktokUrl} - {filename}", tiktokUrl, newFileName);
                        await divHandle.ScreenshotAsync(newFileName);

                        //does old file exist?
                        if (!File.Exists(oldFileName))
                        {
                            await divHandle.ScreenshotAsync(oldFileName);
                        }
                        else
                        {
                            var similarity = Screenshot.Change(oldFileName, newFileName);
                            if (similarity < s.similarityTrigger)
                            {
                                foreach (var service in s.apprise)
                                {
                                    await Apprise.SendNotificationsAsync(service, Settings.SanitizeUserName(tiktokUrl));
                                }
                            }
                        }
                        await browser.CloseAsync();
                    }
                    else
                    {
                        await new BrowserFetcher().DownloadAsync();
                        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                        {
                            Headless = true,
                            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" },
                        });

                        using var page = await browser.NewPageAsync();

                        await page.GoToAsync(tiktokUrl, null, new[] { WaitUntilNavigation.Networkidle2 });
                        await page.EvaluateExpressionAsync(@"document.querySelectorAll('[style*=""z-index: 1001""]').forEach(e => e.remove())");
                        await page.EvaluateExpressionAsync(@"document.querySelectorAll('div[class*=""DivHeaderWrapperMain-StyledDivHeaderWrapperMainV2""]').forEach(e => e.remove())");

                        var divSelector = "[class*=DivShareInfo]";
                        var divHandle = await page.WaitForSelectorAsync(divSelector);

                        string newFileName = Settings.SanitizeFileName(tiktokUrl + "_new.png");
                        string oldFileName = Settings.SanitizeFileName(tiktokUrl + "_old.png");

                        logger.LogInformation("New screenshot saved {tiktokUrl} - {filename}", tiktokUrl, newFileName);
                        await divHandle.ScreenshotAsync(newFileName);

                        //does old file exist?
                        if (!File.Exists(oldFileName))
                        {
                            await divHandle.ScreenshotAsync(oldFileName);
                        }
                        else
                        {
                            var similarity = Screenshot.Change(oldFileName, newFileName);
                            if (similarity < s.similarityTrigger)
                            {
                                logger.LogInformation("Change found for {tiktokUserName}", Settings.SanitizeUserName(tiktokUrl));
                            }
                        }
                        await browser.CloseAsync();
                    }
                }
                else
                {
                    logger.LogInformation("Invalid Tiktok Url {tiktokUrl}", tiktokUrl);
                }
            }
        }
    }
}
