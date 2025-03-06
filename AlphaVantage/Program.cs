
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

class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables
        Env.Load();
        string? apiKey = Environment.GetEnvironmentVariable("ALPHA_VANTAGE_KEY");
        
        AlphaVantageClient client = new AlphaVantageClient(apiKey); // get client
        var context = new FinancialContext(); // get context (db)
        
        AlphaVantageService service = new AlphaVantageService(context, client); // get service

        RunCLI(service);
    }
    static void RunCLI(AlphaVantageService service)
    {
        while (true) 
        {
            Console.WriteLine("\n📊 DATABASE POPULATION TOOL USING ALPHA VANTAGE RESOURCES 📊");
            Console.WriteLine("1. Fetch and Store News Articles");
            Console.WriteLine("2. Fetch and Store Balance Sheets");
            Console.WriteLine("3. Fetch and Store Exchange Rates");
            Console.WriteLine("4. Fetch and Store Stock Data");
            Console.WriteLine("5. Fetch and Store Cash Flow Statements");
            Console.WriteLine("6. Fetch and Store Income Statements");
            Console.WriteLine("7. Fetch and Store Earning Statements");
            Console.WriteLine("8. Exit");
            Console.Write("Enter choice (1-8): ");

            string? choice = Console.ReadLine()?.Trim();

            if (choice == "8")
            {
                Console.WriteLine("Exiting program. Goodbye! 👋");
                break;
            }
            else if (choice == "1")
            {
                FetchNewsArticles(service);
            }
            else if (choice == "2")
            {
                FetchBalanceSheets(service);
            }
            else if (choice == "3")
            {
                FetchExchangeRates(service);
            }
            else if (choice == "4")
            {
                FetchStockData(service);
            }
            else if (choice == "5")
            {
                FetchCashFlowStatements(service);
            }
            else if (choice == "6")
            {
                FetchIncomeStatements(service);
            }
            else if (choice == "7")
            {
                FetchEarningStatements(service);
            }
            else
            {
                Console.WriteLine("❌ Invalid input. Please enter a valid option.");
            }
        }
    }

    static void FetchFinancialData(AlphaVantageService service, string dataType,
        Func<string, bool> fetchByStock, Func<string, bool> fetchBySector)
    {
        Console.WriteLine($"\nWould you like to fetch {dataType} by:");
        Console.WriteLine("1. Stock Ticker");
        Console.WriteLine("2. Sector Name");
        Console.Write("Enter choice (1/2): ");

        string? fetchChoice = Console.ReadLine()?.Trim();

        Func<string, bool>? fetchFunction = fetchChoice switch
        {
            "1" => fetchByStock,
            "2" => fetchBySector,
            _ => null
        };

        if (fetchFunction != null)
        {
            Console.Write(fetchChoice == "1" ? "Enter Stock Ticker (e.g., AAPL): " : "Enter Sector Name (e.g., Financials): ");
            string input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                Console.WriteLine($"📥 Fetching and storing {dataType} for {input}...");
                bool success = fetchFunction(input);
                Console.WriteLine(success
                    ? $"✅ {dataType} fetched and stored successfully!"
                    : $"❌ Failed to store {dataType}.");
            }
            else
            {
                Console.WriteLine("❌ Invalid input. Cannot be empty.");
            }
        }
        else
        {
            Console.WriteLine("❌ Invalid choice. Please enter 1 or 2.");
        }
    }
    
    static void FetchEarningStatements(AlphaVantageService service)
    {
        FetchFinancialData(service, "Earning Statement",
            service.FetchAndStoreAllEarningStatementsFromStock,
            service.FetchAndStoreAllEarningStatementsFromSector);
    }
    
    static void FetchBalanceSheets(AlphaVantageService service)
    {
        FetchFinancialData(service, "Balance Sheets",
            service.FetchAndStoreAllBalanceSheetsFromStock,
            service.FetchAndStoreAllBalanceSheetsFromSector);
    }
    
    static void FetchCashFlowStatements(AlphaVantageService service)
    {
        FetchFinancialData(service, "Cash Flow Statements",
            service.FetchAndStoreAllCashFlowStatementsFromStock,
            service.FetchAndStoreAllCashFlowStatementsFromSector);
    }

    static void FetchIncomeStatements(AlphaVantageService service)
    {
        FetchFinancialData(service, "Income Statements",
            service.FetchAndStoreAllIncomeStatementsFromStock,
            service.FetchAndStoreAllIncomeStatementsFromSector);
    }

    static void FetchExchangeRates(AlphaVantageService service)
    {
        Console.WriteLine("1. Get Exchange Rates for main currencies");
        Console.WriteLine("2. Get Exchange Rates for a specific currency");
        Console.Write("Enter choice (1/2): ");
        string? fetchChoice = Console.ReadLine()?.Trim();

        if (fetchChoice == "1")
            service.PopulateExchangeRates();
    }
    
    static void FetchNewsArticles(AlphaVantageService service)
    {
        Console.WriteLine("\nWould you like to fetch news by:");
        Console.WriteLine("1. Stock Ticker");
        Console.WriteLine("2. Sector Name");
        
        Console.Write("Enter choice (1/2): ");
        string? fetchChoice = Console.ReadLine()?.Trim();
        
        Console.Write("Give me a year: ");
        string? year = Console.ReadLine()?.Trim();    
        
        DateTime startDate = DateTime.Parse($"{year}-01-01");
        DateTime endDate = DateTime.Parse($"{year}-12-31");

        if (fetchChoice == "1")
        {
            Console.Write("Enter Stock Ticker (e.g., AAPL): ");
            string ticker = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (!string.IsNullOrEmpty(ticker))
            {
                Console.WriteLine($"📥 Fetching and storing news articles for stock: {ticker}...");
                service.FetchAndStoreArticlesByStock(ticker, startDate, endDate);
                Console.WriteLine("✅ Articles fetched and stored successfully!");
            }
            else
            {
                Console.WriteLine("❌ Invalid input. Stock ticker cannot be empty.");
            }
        }
        else if (fetchChoice == "2")
        {
            Console.Write("Enter Sector Name (e.g., Financials): ");
            string sector = Console.ReadLine()?.Trim() ?? "";

            if (!string.IsNullOrEmpty(sector))
            {
                Console.WriteLine($"📥 Fetching and storing news articles for sector: {sector}...");
                service.FetchAndStoreArticlesBySector(sector, startDate, endDate);
                Console.WriteLine("✅ Articles fetched and stored successfully!");
            }
            else
            {
                Console.WriteLine("❌ Invalid input. Sector name cannot be empty.");
            }
        }
        else
        {
            Console.WriteLine("❌ Invalid choice. Please enter 1 or 2.");
        }
    }


    static void FetchStockData(AlphaVantageService service)
    {
        Console.WriteLine("1. Get Closing Price for specific stock at all dates");
        Console.WriteLine("2. Get Closing Price for all stocks in a sector at all dates");
        Console.Write("Enter choice (1/2): ");
        string? fetchChoice = Console.ReadLine()?.Trim();

        if (fetchChoice == "1")
        {
            Console.Write("Enter Stock Ticker: ");
            string? ticker = Console.ReadLine()?.Trim();
            service.AddStockClosingPrice(ticker);
        }
        else if (fetchChoice == "2")
        {
            Console.Write("Enter Sector Name: ");
            string? sectorName = Console.ReadLine()?.Trim();
            service.AddStockClosingPricePerSector(sectorName);
        }
    }
}