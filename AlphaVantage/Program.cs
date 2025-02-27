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

class Program
{
    static async Task Main(string[] args)
    {
        Env.Load();
        string? apiKey = Environment.GetEnvironmentVariable("ALPHA_VANTAGE_KEY");
        
        AlphaVantageClient client = new AlphaVantageClient(apiKey);
        var context = new FinancialContext();

        AlphaVantageService service = new AlphaVantageService(context, client);
        
        client.GetNewsSentimentByStock("TSLA", new DateTime(2025, 2, 20, 14, 0, 0), new DateTime(2025, 2, 25, 14, 0, 0));
        //service.FetchAndStoreAllArticlesByStock(new DateTime(2020, 1, 1, 0, 0, 0), new DateTime(2025, 2, 24, 14, 0, 0));

    }
}