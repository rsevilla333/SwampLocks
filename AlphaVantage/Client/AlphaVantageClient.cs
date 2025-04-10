using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Globalization;


namespace SwampLocks.AlphaVantage.Client
{
    public class AlphaVantageClient
    {
        private WebClient client;
        private readonly string _apiKey;
        private const string BaseUrl = "https://alphavantage.co/query";

        public AlphaVantageClient(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            client = new WebClient();
        }

        public List<Tuple<DateTime, Decimal>> GetClosingStockDataWeekly(string ticker)
        {
            return GetCLosingStockData(ticker, "WEEKLY");
        }
        
        public List<Tuple<DateTime, Decimal>> GetClosingStockDataMonthly(string ticker)
        {
            return GetCLosingStockData(ticker, "MONTHLY");
        }
        
        public List<Tuple<DateTime, Decimal>> GetClosingStockDataDaily(string ticker)
        {
            return GetCLosingStockData(ticker, "DAILY");
        }
        
        public List<Tuple<DateTime, Decimal>> GetExchangeRateDaily(string from, string to)
        {
            return GetExchangeRate("DAILY",  from, to);
        }
        
        public List<Tuple<DateTime, Decimal>> GetExchangeRateWeekly(string from, string to)
        {
            return GetExchangeRate("WEEKLY",  from, to);
        }

        public List<Tuple<DateTime, Decimal>> GetExchangeRateMonthly(string from, string to)
        {
            return GetExchangeRate("MONTHLY",  from, to);
        }

		/*
			Communication Services – XLC (Communication Services Select Sector SPDR Fund)
			Consumer Discretionary – XLY (Consumer Discretionary Select Sector SPDR Fund)
			Consumer Staples – XLP (Consumer Staples Select Sector SPDR Fund)
			Energy – XLE (Energy Select Sector SPDR Fund)
			Financials – XLF (Financial Select Sector SPDR Fund)
			Healthcare – XLV (Health Care Select Sector SPDR Fund)
			Industrials – XLI (Industrial Select Sector SPDR Fund)
			Information Technology – XLK (Technology Select Sector SPDR Fund)
			Materials – XLB (Materials Select Sector SPDR Fund)
			Real Estate – XLRE (Real Estate Select Sector SPDR Fund)
			Utilities – XLU (Utilities Select Sector SPDR Fund)
		*/
		public List<string> GetStocksFromETF(string etf)
		{
			string function = $"ETF_PROFILE";
            string queryURL = $"{BaseUrl}?function={function}&symbol={etf}&apikey={_apiKey}";
            Console.WriteLine(queryURL);
            string data = client.DownloadString(queryURL);

			JObject jsonData = JObject.Parse(data);
			JArray holdings = jsonData["holdings"] as JArray;
			
			List<string> result = new List<string>();

			if (holdings == null) 
			{
				Console.WriteLine($"ETF {etf} has no holdings");
				return result;
			}

			foreach(var holding in holdings)
			{
				result.Add(holding["symbol"].ToString());
			}

			return result;
		}


		/*
		Blockchain: blockchain
		Earnings: earnings
		IPO: ipo
		Mergers & Acquisitions: mergers_and_acquisitions
		Financial Markets: financial_markets
		Economy - Fiscal Policy (e.g., tax reform, government spending): economy_fiscal
		Economy - Monetary Policy (e.g., interest rates, inflation): economy_monetary
		Economy - Macro/Overall: economy_macro
		Energy & Transportation: energy_transportation
		Finance: finance
		Life Sciences: life_sciences
		Manufacturing: manufacturing
		Real Estate & Construction: real_estate
		Retail & Wholesale: retail_wholesale
		Technology: technology
		*/
		

		public List<Tuple<DateTime,string, string, Decimal, Decimal>> GetNewsSentimentByStock(string ticker, DateTime DateFrom, DateTime DateTo, double relevanceScore = 0.05) {
			string function = "NEWS_SENTIMENT";
			string dateFormat = "yyyyMMddTHHmm";
			int limit = 1000;
			string queryURL = $"{BaseUrl}?function={function}&tickers={ticker}&time_from={DateFrom.ToString(dateFormat)}&time_to={DateTo.ToString(dateFormat)}&limit={1000}&apikey={_apiKey}";
			Console.WriteLine(queryURL);

			string data = client.DownloadString(queryURL);
			//Console.WriteLine(data);

			dynamic apiResponse = JsonConvert.DeserializeObject<dynamic>(data);
			var result = new List<Tuple<DateTime, string, string, decimal, decimal>>();

			if(apiResponse.feed == null)
			{
				Console.WriteLine($"No news for stock: {ticker}");
				return result;
			}


			foreach (var article in apiResponse.feed)
            {
                foreach (var tickerSentiment in article.ticker_sentiment)
                {
                    if (tickerSentiment.ticker == ticker && tickerSentiment.relevance_score >= relevanceScore)
                    { 
                       	DateTime articleDate = DateTime.ParseExact((string)article.time_published, "yyyyMMddTHHmmss", null);
						decimal sentimentScore = (decimal)double.Parse(tickerSentiment.ticker_sentiment_score.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
						decimal tickerRelevanceScore = (decimal)double.Parse(tickerSentiment.relevance_score.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
						Console.WriteLine(tickerSentiment.relevance_score + " " + sentimentScore);
                        
                        result.Add(new Tuple<DateTime, string, string, decimal, decimal>(
                            articleDate,
                            article.title.ToString(),
							article.url.ToString(),
                            sentimentScore,
                            tickerRelevanceScore
                        ));
                    }
                }
            }
			Console.WriteLine(result.Count);
			return result;
		}
        
        private List<Tuple<DateTime, Decimal>> GetExchangeRate(string interval, string from, string to)
        {
            string function = $"FX_{interval}";
            string queryURL = $"{BaseUrl}?function={function}&from_symbol={from}&to_symbol={to}&outputsize=full&apikey={_apiKey}";
            Console.WriteLine(queryURL);
            string data = client.DownloadString(queryURL);
            
           // Console.WriteLine(data);
			JObject jsonData = JObject.Parse(data);
			interval = interval.Substring(0, 1) + interval.Substring(1).ToLower();
			string metaDataLoc = (interval != "Daily") ? $"{interval} Time Series FX" :  "Time Series FX (Daily)";

			JObject timeSeries = jsonData[metaDataLoc] as JObject;

    		if (timeSeries == null)
    		{
        		Console.WriteLine("No time series data found." + metaDataLoc);
        		return new List<Tuple<DateTime, decimal>>();
   		 	}
			
            List<Tuple<DateTime, decimal>> closingPrices = new List<Tuple<DateTime, decimal>>();

    		foreach (var entry in timeSeries.Properties()) 
    		{
        		DateTime date = DateTime.Parse(entry.Name); // Get the date
        		decimal closingPrice = decimal.Parse(entry.Value["4. close"].ToString());// Get closing price

        		closingPrices.Add(new Tuple<DateTime, decimal>(date, closingPrice));
    		}

    		return closingPrices;
        }
        
        //https://www.alphavantage.co/query?function=OVERVIEW&symbol=MSFT&apikey=YOUR_API_KEY
        //https://www.alphavantage.co/query?function=FX_DAILY&from_symbol=USD&to_symbol=EUR&apikey=YOUR_API_KEY
        //url = 'https://www.alphavantage.co/query?function=NEWS_SENTIMENT&tickers=' + tickers + '&topics=' + topics + '&time_from=20170101T0000&time_to=20241231T2359&sort=EARLIEST&limit=1000&apikey=' + api_key
        
      
        private List<Tuple<DateTime, Decimal>> GetCLosingStockData(string ticker, string interval)
        {
            string function = $"TIME_SERIES_{interval}";
            string queryURL = $"{BaseUrl}?function={function}&symbol={ticker}&outputsize=full&apikey={_apiKey}";
            Console.WriteLine(queryURL);
            string data = client.DownloadString(queryURL);
            
           // Console.WriteLine(data);
            // Parse JSON response
            JObject jsonData = JObject.Parse(data);

            interval = interval.Substring(0, 1) + interval.Substring(1).ToLower();
            Console.WriteLine(interval);

            string metaDataLoc = (interval != "Daily") ? $"{interval} Time Series" :  "Time Series (Daily)";
           // Console.WriteLine(metaDataLoc);
            var timeSeries = jsonData[metaDataLoc] as JObject;

            if (timeSeries == null)
            {
                Console.WriteLine("No time series data found.");
                return new List<Tuple<DateTime, decimal>>();
            }

            List<Tuple<DateTime, decimal>> closingPrices = new List<Tuple<DateTime, decimal>>();

            foreach (var entry in timeSeries.Properties()) 
            {
                DateTime date = DateTime.Parse(entry.Name); // Get the date
                decimal closingPrice = decimal.Parse(entry.Value["4. close"].ToString()); // Get closing price

                closingPrices.Add(new Tuple<DateTime, decimal>(date, closingPrice));
            }

            return closingPrices;
        }
        
        // Fetch General Report Statements for a Stock
        public List<List<string>> GetFinancialReportByStock(string ticker, string function, string reportKey, List<string> fields)
        {
	        string queryURL = $"{BaseUrl}?function={function}&symbol={ticker}&apikey={_apiKey}";
	        Console.WriteLine(queryURL);

	        string data = client.DownloadString(queryURL);
	        JObject jsonData = JObject.Parse(data);
	        var reports = jsonData[reportKey] as JArray;

	        var result = new List<List<string>>();

	        if (reports != null)
	        {
		        foreach (var report in reports)
		        {
			        var reportData = fields.Select(field => report[field]?.ToString()).ToList();
			        result.Add(reportData);
		        }
	        }
	        return result;
        }

        // Fetch Balance Sheets
        public List<List<string>> GetBalanceSheetsByStock(string ticker)
		{
		    return GetFinancialReportByStock(ticker, "BALANCE_SHEET", "annualReports", new List<string>
		    {
		        "fiscalDateEnding", "reportedCurrency", "totalAssets", "totalCurrentAssets",
		        "cashAndCashEquivalentsAtCarryingValue", "cashAndShortTermInvestments",
		        "inventory", "currentNetReceivables", "totalNonCurrentAssets",
		        "propertyPlantEquipment", "accumulatedDepreciationAmortizationPPE",
		        "intangibleAssets", "intangibleAssetsExcludingGoodwill", "goodwill",
		        "investments", "longTermInvestments", "shortTermInvestments",
		        "otherCurrentAssets", "otherNonCurrentAssets", "totalLiabilities",
		        "totalCurrentLiabilities", "currentAccountsPayable", "deferredRevenue",
		        "currentDebt", "shortTermDebt", "totalNonCurrentLiabilities",
		        "capitalLeaseObligations", "longTermDebt", "currentLongTermDebt",
		        "longTermDebtNoncurrent", "shortLongTermDebtTotal",
		        "otherCurrentLiabilities", "otherNonCurrentLiabilities",
		        "totalShareholderEquity", "treasuryStock", "retainedEarnings",
		        "commonStock", "commonStockSharesOutstanding"
		    });
		}

		// Fetch Cash Flow Statements
		public List<List<string>> GetCashFlowStatementsByStock(string ticker)
		{
		    return GetFinancialReportByStock(ticker, "CASH_FLOW", "quarterlyReports", new List<string>
		    {
		        "fiscalDateEnding", "reportedCurrency", "operatingCashflow",
		        "paymentsForOperatingActivities", "proceedsFromOperatingActivities",
		        "changeInOperatingLiabilities", "changeInOperatingAssets",
		        "depreciationDepletionAndAmortization", "capitalExpenditures",
		        "changeInReceivables", "changeInInventory", "profitLoss",
		        "cashflowFromInvestment", "cashflowFromFinancing",
		        "proceedsFromRepaymentsOfShortTermDebt", "paymentsForRepurchaseOfCommonStock",
		        "paymentsForRepurchaseOfEquity", "paymentsForRepurchaseOfPreferredStock",
		        "dividendPayout", "dividendPayoutCommonStock", "dividendPayoutPreferredStock",
		        "proceedsFromIssuanceOfCommonStock", "proceedsFromIssuanceOfLongTermDebtAndCapitalSecuritiesNet",
		        "proceedsFromIssuanceOfPreferredStock", "proceedsFromRepurchaseOfEquity",
		        "proceedsFromSaleOfTreasuryStock", "changeInCashAndCashEquivalents",
		        "changeInExchangeRate", "netIncome"
		    });
		}

		// Fetch Income Statements
		public List<List<string>> GetIncomeStatementsByStock(string ticker)
		{
		    return GetFinancialReportByStock(ticker, "INCOME_STATEMENT", "quarterlyReports", new List<string>
		    {
		        "fiscalDateEnding", "reportedCurrency", "grossProfit", "totalRevenue",
		        "costOfRevenue", "costofGoodsAndServicesSold", "operatingIncome",
		        "sellingGeneralAndAdministrative", "researchAndDevelopment",
		        "operatingExpenses", "investmentIncomeNet", "netInterestIncome",
		        "interestIncome", "interestExpense", "nonInterestIncome",
		        "otherNonOperatingIncome", "depreciation", "depreciationAndAmortization",
		        "incomeBeforeTax", "incomeTaxExpense", "interestAndDebtExpense",
		        "netIncomeFromContinuingOperations", "comprehensiveIncomeNetOfTax",
		        "ebit", "ebitda", "netIncome"
		    });
		}
		
		// Fetch Earning Statements
		public List<List<string>> GetEarningStatementsByStock(string ticker)
		{
			return GetFinancialReportByStock(ticker, "EARNINGS", "quarterlyEarnings", new List<string>
			{
				"fiscalDateEnding", "reportedDate", "reportedEPS", "estimatedEPS", 
				"surprise", "surprisePercentage", "reportTime"
			});
		}

		public List<Tuple<DateTime, decimal>> GetEconomicData(string indicator)
		{
			var ecoIndicatorFunction = new Dictionary<string, string>
			{
				{"GDP", "REAL_GDP&interval=quarterly" }, // quarterly
				{"GDPPC", "REAL_GDP_PER_CAPITA" }, // quarterly
				{"10Y-TCMR", "TREASURY_YIELD" }, // daily
				{"30Y-TCMR", "TREASURY_YIELD&interval=daily&maturity=30year" }, //  daily
				{"7Y-TCMR", "TREASURY_YIELD&interval=daily&maturity=7year" }, //  daily
				{"5Y-TCMR", "TREASURY_YIELD&interval=daily&maturity=5year" }, //  daily
				{"2Y-TCMR", "TREASURY_YIELD&interval=daily&maturity=2year" }, //  daily
				{"3M-TCMR", "TREASURY_YIELD&interval=daily&maturity=3month" }, //  daily
				{"FFR", "FEDERAL_FUNDS_RATE&interval=daily" }, // daily
				{"CPI", "CPI" }, // monthly
				{"Inflation" ,"INFLATION" }, // annual
				{"RetailSales","RETAIL_SALES"}, // monthly
				{"Durables","DURABLES"}, // mobthly
				{"Unemployment", "UNEMPLOYMENT"}, // monthly
				{"TNP", "NONFARM_PAYROLL" }, // monthly
			};
			
			var results = new List<Tuple<DateTime, decimal>>();
			
			if (ecoIndicatorFunction.TryGetValue(indicator, out var function))
			{
				string queryURL = $"{BaseUrl}?function={function}&apikey={_apiKey}";
				Console.WriteLine(queryURL);
				
				string data = client.DownloadString(queryURL);
				//Console.WriteLine(data);
				JObject jsonData = JObject.Parse(data);
				var reports = jsonData["data"] as JArray;
				
				if (reports != null)
				{
					foreach (var report in reports)
					{
						//var date = DateTime.TryParse(report["value"]?.ToString(), out var parsedDate) ? parsedDate : (DateTime?)null;
						//var value = Decimal.TryParse(report["date"]?.ToString(), out var parsedValue) ? parsedValue : (decimal?)null;

						string val = report["value"].ToString();
						
						if(val == ".")
							val = "0";
						
						results.Add(new Tuple<DateTime, decimal>(
							DateTime.Parse(report["date"].ToString()), 
							Decimal.Parse(val)
						));

						//Console.WriteLine(results.Count);

					}
				}
			}
			else
			{
				Console.WriteLine($"Not a valid indicator");
			}
			
			return results;
		}

		public List<Tuple<DateTime, decimal>> GetCommodityData(string commodity)
		{
   			var commodityFunctionMap = new Dictionary<string, string>
    		{
        		{"WTI", "WTI&interval=daily"},
        		{"BRENT", "BRENT&interval=daily"},
        		{"NATURAL_GAS", "NATURAL_GAS&interval=daily"},
        		{"COPPER", "COPPER&interval=monthly"},
        		{"ALUMINUM", "ALUMINUM&interval=monthly"},
        		{"WHEAT", "WHEAT&interval=monthly"},
        		{"CORN", "CORN&interval=monthly"},
        		{"COTTON", "COTTON&interval=monthly"},
        		{"SUGAR", "SUGAR&interval=monthly"},
        		{"COFFEE", "COFFEE&interval=monthly"},
        		{"ALL_COMMODITIES", "ALL_COMMODITIES&interval=monthly"}
    		};

    		var results = new List<Tuple<DateTime, decimal>>();

    		if (commodityFunctionMap.TryGetValue(commodity, out var function))
    		{
        		string queryURL = $"{BaseUrl}?function={function}&apikey={_apiKey}";
        		Console.WriteLine(queryURL);

        		using (WebClient client = new WebClient())
        		{
            		string data = client.DownloadString(queryURL);
            		JObject jsonData = JObject.Parse(data);
            		var reports = jsonData["data"] as JArray;

            		if (reports != null)
            		{
            		    foreach (var report in reports)
                		{
                    		string val = report["value"]?.ToString() ?? "0";
                    		if (val == ".") val = "0";

                    		results.Add(new Tuple<DateTime, decimal>(
                        		DateTime.Parse(report["date"].ToString()),
                        		Decimal.Parse(val)
                    		));
                		}
            		}
        		}
    		}
    		else
    		{
        		Console.WriteLine("Not a valid commodity indicator.");
    		}

    		return results;
		}
		
		public List<Tuple<DateTime, string>> GetStockSplits(string ticker)
		{
			string function = "SPLITS";
			string queryURL = $"{BaseUrl}?function={function}&symbol={ticker}&apikey={_apiKey}";
			Console.WriteLine(queryURL);

			string data = client.DownloadString(queryURL);
			JObject jsonData = JObject.Parse(data);

			List<Tuple<DateTime, string>> splits = new List<Tuple<DateTime, string>>();
    
			if (jsonData["data"] != null)
			{
				foreach (var entry in jsonData["data"])
				{
					DateTime date = DateTime.Parse(entry["effective_date"].ToString());
					string splitRatio = entry["split_factor"].ToString();
					splits.Add(new Tuple<DateTime, string>(date, splitRatio));
				}
			}

			return splits;
		}


    }
}