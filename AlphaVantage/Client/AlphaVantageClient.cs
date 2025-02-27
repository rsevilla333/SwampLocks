using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json;

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
		public List<Tuple<DateTime,string, Decimal>> GetNewsSentimentBySector(string sector, DateTime DateFrom, DateTime DateTo) {
			string function = "NEWS_SENTIMENT";
			string dateFormat = "yyyyMMddTHHmm";
			string queryURL = $"{BaseUrl}?function={function}&topics{sector}&time_from={DateFrom.ToString(dateFormat)}&time_to={DateTo.ToString(dateFormat)}&apikey={_apiKey}";
			Console.WriteLine(queryURL);

			string data = client.DownloadString(queryURL);
			Console.WriteLine(data);

			dynamic apiResponse = JsonConvert.DeserializeObject<dynamic>(data);
			List<Tuple<DateTime, string, decimal>> result = new List<Tuple<DateTime, string, decimal>>();

			return result;
		}

		public List<Tuple<DateTime,string, Decimal>> GetNewsSentimentByStock(string ticker, DateTime DateFrom, DateTime DateTo, double relevanceScore = 0.5) {
			string function = "NEWS_SENTIMENT";
			string dateFormat = "yyyyMMddTHHmm";
			string queryURL = $"{BaseUrl}?function={function}&tickers={ticker}&time_from={DateFrom.ToString(dateFormat)}&time_to={DateTo.ToString(dateFormat)}&apikey={_apiKey}";
			Console.WriteLine(queryURL);

			string data = client.DownloadString(queryURL);
			//Console.WriteLine(data);

			dynamic apiResponse = JsonConvert.DeserializeObject<dynamic>(data);
			List<Tuple<DateTime, string, decimal>> result = new List<Tuple<DateTime, string, decimal>>();

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
						Console.WriteLine(tickerSentiment.relevance_score);
                       	DateTime articleDate = DateTime.ParseExact((string)article.time_published, "yyyyMMddTHHmmss", null);
                        
                        result.Add(new Tuple<DateTime, string, decimal>(
                            articleDate,
                            article.title.ToString(),
                            (decimal)tickerSentiment.ticker_sentiment_score
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
            string queryURL = $"{BaseUrl}?function={function}&from_symbol={from}&to_symbol={to}&apikey={_apiKey}";
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
            string queryURL = $"{BaseUrl}?function={function}&symbol={ticker}&apikey={_apiKey}";
            Console.WriteLine(queryURL);
            string data = client.DownloadString(queryURL);
            
            Console.WriteLine(data);
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
        
    }
}