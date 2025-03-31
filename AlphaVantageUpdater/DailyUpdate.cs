using DotNetEnv;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SwampLocks.AlphaVantage.Service;
using SwampLocksDb.Data;
using SwampLocksDb.Models;
using SwampLocks.AlphaVantage.Client;
using SwampLocks.AlphaVantage.Email;
using System;

namespace AlphaVantageUpdater
{
    public class DailyUpdate
    {
        private readonly AlphaVantageService _alphaVantageService;
        private readonly ILogger<DailyUpdate> _logger;

        public DailyUpdate(ILogger<DailyUpdate> logger)
        {
            _logger = logger;

            // Load environment variables
            Env.Load();

            if (!File.Exists(".env"))
            {
                string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;

                if (projectRoot != null)
                {
                    string envPath = Path.Combine(projectRoot, ".env");
                    if (File.Exists(envPath))
                    {
                        Env.Load(envPath); // Load .env from the correct location
                    }
                }
            }

            // Read the environment variables
            string apiKey = Environment.GetEnvironmentVariable("ALPHA_VANTAGE_KEY") ?? "none";

            string smtpServer = Environment.GetEnvironmentVariable("EMAIL_SERVER") ?? "none";
            string smtpUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? "none";
            string smtpPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? "none";

            // Initialize the necessary services
            var dbContext = new FinancialContext(true); // Your DB context, assuming it's set up.
            var client = new AlphaVantageClient(apiKey);
            var emailService = new EmailNotificationService(smtpServer, smtpUsername, smtpPassword);

            // Initialize the AlphaVantageService
            _alphaVantageService = new AlphaVantageService(dbContext, client, emailService);
        }

        [Function("Ping")]
        public IActionResult RunPing([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            return new OkObjectResult("Ping");
        }

        [Function("ManualUpdate")]
        public IActionResult RunManualUpdate([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            _alphaVantageService.FetchAndUpdateEverything();

            return new OkObjectResult("Finished update");
        }

        [Function("TimerUpdate")]
        public IActionResult RunTimerUpdate([TimerTrigger("0 0 18 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            _alphaVantageService.FetchAndUpdateEverything();

            return new OkObjectResult("Finished update");
        }
    }
}
