using SwampLocks.AlphaVantage.Client;
using SwampLocks.AlphaVantage.Service;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using DotNetEnv;
using SwampLocksDb.Models;
using SwampLocksDb.Data;
using System;
using System.Threading.Tasks;
using DotNetEnv;
using SwampLocks.AlphaVantage.CLI;
using SwampLocks.EmailSevice;

class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables
        Env.Load();
        string? apiKey = Environment.GetEnvironmentVariable("ALPHA_VANTAGE_KEY");
		string smtpServer = Environment.GetEnvironmentVariable("EMAIL_SERVER") ?? "none";;
        string smtpUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? "none";
        string smtpPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? "none";
        
        AlphaVantageClient client = new AlphaVantageClient(apiKey); // get client
        var context = new FinancialContext(); // get context (db)

		var emailLogger = new EmailNotificationService(smtpServer,smtpUsername, smtpPassword);
		//await emailLogger.SendEmailNotification("rsevilla@ufl.edu", "TEST" , "please work");
        
        AlphaVantageService service = new AlphaVantageService(context, client, emailLogger); // get service

        var cli = new AlphaCLI(service);
        cli.Run();

    }

    
}