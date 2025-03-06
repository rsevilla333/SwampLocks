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

        // Initialize API client and database context
        AlphaVantageClient client = new AlphaVantageClient(apiKey);
        var context = new FinancialContext();
        AlphaVantageService service = new AlphaVantageService(context, client);

        RunCLI(service);
    }
    static void RunCLI(AlphaVantageService service)
    {
        while (true) // Keep CLI running until user exits
        {
            Console.WriteLine("\n📊 DATABASE POPULATION TOOL 📊");
            Console.WriteLine("1. Fetch and Store News Articles");
            Console.WriteLine("2. Fetch and Store Balance Sheets");
            Console.WriteLine("3. Fetch and Store Exchange Rates");
            Console.WriteLine("4. Fetch and Store Stock Data");
            Console.WriteLine("5. Fetch and Store Cash Flow Statements");
            Console.WriteLine("6. Exit");
            Console.Write("Enter choice (1-5): ");

            string? choice = Console.ReadLine()?.Trim();

            if (choice == "6")
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
                FetcCashFlowStatements(service);
            }
            else
            {
                Console.WriteLine("❌ Invalid input. Please enter a valid option.");
            }
        }
    }

    static void FetchNewsArticles(AlphaVantageService service)
    {
        Console.WriteLine("\nWould you like to fetch news by:");
        Console.WriteLine("1. Stock Ticker");
        Console.WriteLine("2. Sector Name");
        Console.Write("Enter choice (1/2): ");

        string? fetchChoice = Console.ReadLine()?.Trim();
        DateTime startDate = DateTime.Parse("2021-01-01");
        DateTime endDate = DateTime.Parse("2021-12-31");

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

    static void FetchBalanceSheets(AlphaVantageService service)
    {
        Console.WriteLine("\nWould you like to fetch balance sheets by:");
        Console.WriteLine("1. Stock Ticker");
        Console.WriteLine("2. Sector Name");
        Console.Write("Enter choice (1/2): ");

        string? fetchChoice = Console.ReadLine()?.Trim();

        if (fetchChoice == "1")
        {
            Console.Write("Enter Stock Ticker (e.g., AAPL): ");
            string ticker = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (!string.IsNullOrEmpty(ticker))
            {
                Console.WriteLine($"📥 Fetching and storing balance sheets for stock: {ticker}...");
                bool success = service.FetchAndStoreAllBalanceSheetsFromStock(ticker);
                Console.WriteLine(success
                    ? "✅ Balance sheets fetched and stored successfully!"
                    : "❌ Failed to store balance sheets.");
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
                Console.WriteLine($"📥 Fetching and storing balance sheets for sector: {sector}...");
                bool success = service.FetchAndStoreAllBalanceSheetsFromSector(sector);
                Console.WriteLine(success
                    ? "✅ Balance sheets fetched and stored successfully!"
                    : "❌ Failed to store balance sheets.");
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
    
    static void FetcCashFlowStatements(AlphaVantageService service)
    {
        Console.WriteLine("\nWould you like to fetch cash flow statements by:");
        Console.WriteLine("1. Stock Ticker");
        Console.WriteLine("2. Sector Name");
        Console.Write("Enter choice (1/2): ");

        string? fetchChoice = Console.ReadLine()?.Trim();

        if (fetchChoice == "1")
        {
            Console.Write("Enter Stock Ticker (e.g., AAPL): ");
            string ticker = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (!string.IsNullOrEmpty(ticker))
            {
                Console.WriteLine($"📥 Fetching and storing cash flow statements for stock: {ticker}...");
                bool success = service.FetchAndStoreAllCashFlowStatemetsFromStock(ticker);
                Console.WriteLine(success
                    ? "✅ Cash flow statements fetched and stored successfully!"
                    : "❌ Failed to store Cash flow statements.");
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
                Console.WriteLine($"📥 Fetching and storing cash flow statements for sector: {sector}...");
                bool success = service.FetchAndStoreAllCashFlowStatemetsFromSector(sector);
                Console.WriteLine(success
                    ? "✅ Cash flow statements fetched and stored successfully!"
                    : "❌ Failed to store cash flow statements.");
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

    static void FetchExchangeRates(AlphaVantageService service)
    {
        Console.WriteLine("1. Get Exchange Rates for main currencies");
        Console.WriteLine("2. Get Exchange Rates for a specific currency");
        Console.Write("Enter choice (1/2): ");
        string? fetchChoice = Console.ReadLine()?.Trim();

        if (fetchChoice == "1")
            service.PopulateExchangeRates();
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
    
