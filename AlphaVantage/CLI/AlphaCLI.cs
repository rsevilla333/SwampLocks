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


namespace SwampLocks.AlphaVantage.CLI
{
    public class AlphaCLI
    {
        private AlphaVantageService _service;
        public AlphaCLI(AlphaVantageService service)
        {
            _service = service;
        }
        
        public void Run()
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
                Console.WriteLine("8. Fetch and Store Economic Data");
			    Console.WriteLine("9. Fetch and Store Commodity Data");
			    Console.WriteLine("10. Update Everything");
                Console.WriteLine("11. Exit");
                Console.Write("Enter choice (1-11): ");

                string? choice = Console.ReadLine()?.Trim();

                if (choice == "11")
                {
                    Console.WriteLine("Exiting program. Goodbye! 👋");
                    break;
                }
                else if (choice == "1")
                {
                    FetchNewsArticles();
                }
                else if (choice == "2")
                {
                    FetchBalanceSheets();
                }
                else if (choice == "3")
                {
                    FetchExchangeRates();
                }
                else if (choice == "4")
                {
                    FetchStockData();
                }
                else if (choice == "5")
                {
                    FetchCashFlowStatements();
                }
                else if (choice == "6")
                {
                    FetchIncomeStatements();
                }
                else if (choice == "7")
                {
                    FetchEarningStatements();
                }
                else if (choice == "8")
                {
                    FetchEconomicData();
                }

			    else if (choice == "9")
                {
                    FetchCommoditiesData();
                }
			    else if (choice == "10")
                {
                    FetchAndUpdateEverything();
                }
                else
                {
                    Console.WriteLine("❌ Invalid input. Please enter a valid option.");
                }
            }
        }

	    private void FetchAndUpdateEverything()
        {
            Console.WriteLine("Fetching All Economic Data");
            _service.FetchAndUpdateEverything();
        }

        private void FetchEconomicData()
        {
            Console.WriteLine("Fetching All Economic Data");
            _service.FetchAndStoreAllEconomicData();
        }


        private void FetchCommoditiesData()
        {
            Console.WriteLine("Fetching All Commodities' Data");
            _service.FetchAndStoreAllCommodityData();
        }
        

        private void FetchFinancialData(string dataType,
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
        
        private void FetchEarningStatements()
        {
            FetchFinancialData("Earning Statement",
                _service.FetchAndStoreAllEarningStatementsFromStock,
                _service.FetchAndStoreAllEarningStatementsFromSector);
        }
        
        private void FetchBalanceSheets()
        {
            FetchFinancialData("Balance Sheets",
                _service.FetchAndStoreAllBalanceSheetsFromStock,
                _service.FetchAndStoreAllBalanceSheetsFromSector);
        }
        
        private void FetchCashFlowStatements()
        {
            FetchFinancialData("Cash Flow Statements",
                _service.FetchAndStoreAllCashFlowStatementsFromStock,
                _service.FetchAndStoreAllCashFlowStatementsFromSector);
        }

        private void FetchIncomeStatements()
        {
            FetchFinancialData("Income Statements",
                _service.FetchAndStoreAllIncomeStatementsFromStock,
                _service.FetchAndStoreAllIncomeStatementsFromSector);
        }

        private void FetchExchangeRates()
        {
            Console.WriteLine("1. Get Exchange Rates for main currencies");
            Console.WriteLine("2. Get Exchange Rates for a specific currency");
            Console.Write("Enter choice (1/2): ");
            string? fetchChoice = Console.ReadLine()?.Trim();

            if (fetchChoice == "1")
                _service.PopulateExchangeRates();
        }
        
        private void FetchNewsArticles()
        {
            Console.WriteLine("\nWould you like to fetch news by:");
            Console.WriteLine("1. Stock Ticker");
            Console.WriteLine("2. Sector Name");
            
            Console.Write("Enter choice (1/2): ");
            string? fetchChoice = Console.ReadLine()?.Trim();
            
            Console.Write("Give me a year: ");
            string? year = Console.ReadLine()?.Trim();    
            
            DateTime startDate = DateTime.Parse($"{year}-01-01");
            DateTime endDate = new DateTime(2022,01,01);//DateTime.Parse($"{year}-12-31");

            if (fetchChoice == "1")
            {
                Console.Write("Enter Stock Ticker (e.g., AAPL): ");
                string ticker = Console.ReadLine()?.Trim().ToUpper() ?? "";

                if (!string.IsNullOrEmpty(ticker))
                {
                    Console.WriteLine($"📥 Fetching and storing news articles for stock: {ticker}...");
                    _service.FetchAndStoreArticlesByStock(ticker, startDate, endDate);
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
                    _service.FetchAndStoreArticlesBySector(sector, endDate);
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
        
        private void FetchStockData()
        {
            Console.WriteLine("1. Get Closing Price for specific stock at all dates");
            Console.WriteLine("2. Get Closing Price for all stocks in a sector at all dates");
            Console.Write("Enter choice (1/2): ");
            string? fetchChoice = Console.ReadLine()?.Trim();

            if (fetchChoice == "1")
            {
                Console.Write("Enter Stock Ticker: ");
                string? ticker = Console.ReadLine()?.Trim();
                _service.AddStockClosingPrice(ticker);
            }
            else if (fetchChoice == "2")
            {
                Console.Write("Enter Sector Name: ");
                string? sectorName = Console.ReadLine()?.Trim();
                _service.AddStockClosingPricePerSector(sectorName);
            }
        }
    }
}