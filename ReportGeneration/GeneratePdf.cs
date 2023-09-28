using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace ReportGeneration
{
    public class GeneratePdf
    {
        private readonly ILogger _logger;

        public GeneratePdf(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GeneratePdf>();
        }

        [Function("GeneratePdfFunc")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string? url = req.Query["url"];
                if (string.IsNullOrEmpty(url))
                {
                    return new BadRequestObjectResult("Provided url is invalid.");
                }

                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                var launchOptions = new LaunchOptions { Headless = true, Args = new[] { "--no-sandbox" } };
                var pdfOptions = new PdfOptions { PrintBackground = true, Landscape = true };
                var path = $"{DateTime.Today.ToShortDateString().Replace("/", "-")}.pdf";

                var browser = await Puppeteer.LaunchAsync(launchOptions);

                using (var page = await browser.NewPageAsync())
                {
                    await page.GoToAsync(url /*navigation*/);
                    await page.PdfAsync(path, pdfOptions);
                }

                return new OkObjectResult(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generating the Pdf file failed.");
                return new BadRequestObjectResult($"An error occured while generating the document: {ex.Message}");
            }
        }
    }
}
