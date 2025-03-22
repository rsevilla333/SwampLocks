using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SwampLocks.AlphaVantage.Service;
using SwampLocks.AlphaVantage.Client;
using SwampLocksDb.Data;
using SwampLocks.AlphaVantage.Email;
using System;
using System.Threading.Tasks;
using DotNetEnv;

public class DailyUpdate
{
    private readonly AlphaVantageService _alphaVantageService;

    // Constructor for Dependency Injection (if you're handling DI in another way)
    public DailyUpdate()
    {
        // Load environment variables
        Env.Load();

        // Read the environment variables
        string apiKey = Environment.GetEnvironmentVariable("ALPHA_VANTAGE_KEY");
        string smtpServer = Environment.GetEnvironmentVariable("EMAIL_SERVER") ?? "none";
        string smtpUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? "none";
        string smtpPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? "none";

        // Initialize the necessary services
        var dbContext = new FinancialContext(); // Your DB context, assuming it's set up.
        var client = new AlphaVantageClient(apiKey);
        var emailService = new EmailNotificationService(smtpServer, smtpUsername, smtpPassword);

        // Initialize the AlphaVantageService
        _alphaVantageService = new AlphaVantageService(dbContext, client, emailService);
    }

    // The TimerTrigger runs the function once a day at midnight
    [FunctionName("DailyUpdate")]
    public void Run([TimerTrigger("0 0 18 * * *")] TimerInfo myTimer, ILogger log)
    {
        log.LogInformation($"Function executed at: {DateTime.Now}");

        // Call your service's method to perform the update
        _alphaVantageService.FetchAndUpdateEverything();
    }
}