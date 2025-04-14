using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SwampLocksDb.Data;
using SwampLocksDb.Models;
using SwampLocks.AlphaVantage.Client;
using SwampLocks.EmailSevice;
using SwampLocksDb.Models;
using SwampLocksDb.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Globalization;
using System.Collections.Generic;


namespace SwampLocks.AlphaVantage.Service
{
    public class AlphaVantageService
    {
        private readonly FinancialContext _context;
        private readonly AlphaVantageClient _client;
        private readonly EmailNotificationService _emailLogger;

        private List<(string Name, string Symbol)> _sectors = new List<(string, string)>
        {
            ("Communication Services", "XLC"),
            ("Consumer Discretionary", "XLY"),
            ("Consumer Staples", "XLP"),
            ("Energy", "XLE"),
            ("Financials", "XLF"),
            ("Healthcare", "XLV"),
            ("Industrials", "XLI"),
            ("Information Technology", "XLK"),
            ("Materials", "XLB"), // 
            ("Real Estate", "XLRE"), // 30
            ("Utilities", "XLU") // 22
        };

        private List<string> _currencies = new List<string>
        {
            "EUR", "JPY", "BTC", "CAD", "AED", "AFN", "ALL", "AMD", "ANG", "AOA",
            "ARS", "AUD", "AWG", "AZN", "BAM", "BBD", "BDT", "BGN", "BHD", "BIF",
            "BMD", "BND", "BOB", "BRL", "BSD", "BTN", "BWP", "BZD", "CAD", "CDF",
            "CHF", "CLF", "CLP", "CNH", "CNY", "COP", "CUP", "CVE", "CZK", "DJF",
            "DKK", "DOP", "DZD", "EGP", "ERN", "ETB", "EUR", "FJD", "FKP", "GBP",
            "GEL", "GHS", "GIP", "GMD", "GNF", "GTQ", "GYD", "HKD", "HNL", "HRK",
            "HTG", "HUF", "ICP", "IDR", "ILS", "INR", "IQD", "IRR", "ISK", "JEP",
            "JMD", "JOD", "JPY", "KES", "KGS", "KHR", "KMF", "KPW", "KRW", "KWD",
            "KYD", "KZT", "LAK", "LBP", "LKR", "LRD", "LSL", "LYD", "MAD", "MDL",
            "MGA", "MKD", "MMK", "MNT", "MOP", "MRO", "MRU", "MUR", "MVR", "MWK",
            "MXN", "MYR", "MZN", "NAD", "NGN", "NOK", "NPR", "NZD", "OMR", "PAB",
            "PEN", "PGK", "PHP", "PKR", "PLN", "PYG", "QAR", "RON", "RSD", "RUB",
            "RUR", "RWF", "SAR", "SBDf", "SCR", "SDG", "SDR", "SEK", "SGD", "SHP",
            "SLL", "SOS", "SRD", "SYP", "SZL", "THB", "TJS", "TMT", "TND", "TOP",
            "TRY", "TTD", "TWD", "TZS", "UAH", "UGX", "USD", "UYU", "UZS", "VND",
            "VUV", "WST", "XAF", "XCD", "XDR", "XOF", "XPF", "YER", "ZAR", "ZMW",
            "ZWL"
        };

        public AlphaVantageService(FinancialContext context, AlphaVantageClient client,
            EmailNotificationService emailLogger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _emailLogger = emailLogger ?? throw new ArgumentNullException(nameof(emailLogger));
        }

        public void PopulateExchangeRates()
        {
            foreach (var currency in _currencies)
            {
                getExchangeRatesFor(currency);
            }
        }

        public void PopulateSectors()
        {
            foreach (var (name, symbol) in _sectors)
            {
                PopulateSector(name, symbol);
            }
        }

        public void FetchAndUpdateEverything()
        {
            string subject;
            string updateResult;
            var results = new List<string>();

            try
            { 
                Console.WriteLine("UPDATING ALL DATA IN DB");

               var lastUpdated = _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "Reports")?.LastUpdated;
               bool updateReports = lastUpdated.HasValue && (DateTime.UtcNow - lastUpdated.Value).Days >= 90;

               // Execute and track results
               TryExecute(PopulateExchangeRates, "PopulateExchangeRates", results);
               TryExecute(FetchAndStoreAllEconomicData, "FetchAndStoreAllEconomicData", results);
               TryExecute(FetchAndStoreAllEconomicData, "FetchAndStoreAllCommodityData", results);

                foreach (var (sectorName, symbol) in _sectors)
                {
                    TryExecute(() => AddStockClosingPricePerSector(sectorName),
                   $"AddStockClosingPricePerSector({sectorName})", results);
                    TryExecute(() => FetchAndStoreArticlesBySector(sectorName,DateTime.UtcNow),
                        $"FetchAndStoreArticlesBySector({sectorName})", results);

               if (updateReports)
               {
                   TryExecute(() => FetchAndStoreAllEarningStatementsFromSector(sectorName),
                       $"FetchAndStoreAllEarningStatementsFromSector({sectorName})", results);
                   TryExecute(() => FetchAndStoreAllBalanceSheetsFromSector(sectorName),
                       $"FetchAndStoreAllBalanceSheetsFromSector({sectorName})", results);
                   TryExecute(() => FetchAndStoreAllCashFlowStatementsFromSector(sectorName),
                       $"FetchAndStoreAllCashFlowStatementsFromSector({sectorName})", results);
                   TryExecute(() => FetchAndStoreAllIncomeStatementsFromSector(sectorName),
                       $"FetchAndStoreAllIncomeStatementsFromSector({sectorName})", results);
               }

                    AddStockSplitsPricePerSector(sectorName);
                }

			       BackfillMarketCap();

                    // Update tracking timestamps
                   _context.DataUpdateTrackers.First(d => d.DataType == "ExRates").LastUpdated = DateTime.UtcNow;
                   _context.DataUpdateTrackers.First(d => d.DataType == "EcoIndicators").LastUpdated = DateTime.UtcNow;
                   _context.DataUpdateTrackers.First(d => d.DataType == "Commodities").LastUpdated = DateTime.UtcNow;
                   _context.DataUpdateTrackers.First(d => d.DataType == "Articles").LastUpdated = DateTime.UtcNow;

                   if (updateReports)
                   {
                       _context.DataUpdateTrackers.First(d => d.DataType == "Reports").LastUpdated = DateTime.UtcNow;
                   }

                   _context.DataUpdateTrackers.First(d => d.DataType == "StockData").LastUpdated = DateTime.UtcNow;
                   _context.SaveChanges();

                   updateResult = $"Most current date for all data is now {DateTime.UtcNow}\n\nFunction Results:\n" +
                                  string.Join("\n", results);
                   subject = $"SwampLocks Database Update as of {DateTime.UtcNow:yyyy-MM-dd} result";

                   Console.WriteLine(updateResult);
           }
           catch (Exception ex)
           {
                // Handle errors and send an email with error details
                updateResult = $"ERROR: An unexpected error occurred during the database update.\n\n" +
                               $"Exception Message: {ex.Message}\n\nStackTrace:\n{ex.StackTrace}";

                subject = $"SwampLocks Database Update FAILED - {DateTime.UtcNow:yyyy-MM-dd}";
                Console.WriteLine(updateResult);
            }

             // Send Email Notification (Success or Error)
              _emailLogger.SendEmailNotification("rsevilla@ufl.edu", subject, updateResult);
              _emailLogger.SendEmailNotification("mdylewski@ufl.edu", subject, updateResult);
              _emailLogger.SendEmailNotification("andresportillo@ufl.edu", subject, updateResult);
              _emailLogger.SendEmailNotification("mdylewski@ufl.edu", subject, updateResult);
              _emailLogger.SendEmailNotification("patel.deep@ufl.edu", subject, updateResult);
        }
        

        private void TryExecute(Action action, string functionName, List<string> results)
        {
            try
            {
                action();
                results.Add($"{functionName}: Success");
            }
            catch (Exception ex)
            {
                results.Add($"{functionName}: ERROR - {ex.Message}");
            }
        }


        public bool PopulateSector(string sectorName, string etf)
        {
            var sectorStocksTickers = _client.GetStocksFromETF(etf);

            foreach (var ticker in sectorStocksTickers)
            {
                AddStock(ticker, sectorName);
            }

            Console.WriteLine($"Populated {sectorName} sector");
            return true;
        }

        public bool FetchAndStoreAllReportsFromSector(string sectorName, string reportType,
            Func<string, bool> fetchAndStoreByStock)
        {
            List<string> stockTickers = _context.Stocks
                .Where(s => s.Sector.Name == sectorName)
                .Select(s => s.Ticker)
                .ToList();

            foreach (var ticker in stockTickers)
            {
                try
                {
                    bool result = fetchAndStoreByStock(ticker);
                    if (!result)
                    {
                        Console.WriteLine($"Failed to process {reportType} for {ticker}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {reportType} for {ticker}: {ex.Message}");
                }
            }

            Console.WriteLine($"Populated {sectorName} sector");
            return true;
        }

        public bool FetchAndStoreAllCashFlowStatementsFromSector(string sectorName)
        {
            return FetchAndStoreAllReportsFromSector(sectorName, "Cash Flow Statement",
                FetchAndStoreAllCashFlowStatementsFromStock);
        }

        public bool FetchAndStoreAllBalanceSheetsFromSector(string sectorName)
        {
            return FetchAndStoreAllReportsFromSector(sectorName, "Balance Sheet",
                FetchAndStoreAllBalanceSheetsFromStock);
        }

        public bool FetchAndStoreAllIncomeStatementsFromSector(string sectorName)
        {
            return FetchAndStoreAllReportsFromSector(sectorName, "Income Statement",
                FetchAndStoreAllIncomeStatementsFromStock);
        }

        public bool FetchAndStoreAllEarningStatementsFromSector(string sectorName)
        {
            return FetchAndStoreAllReportsFromSector(sectorName, "Earning Statement",
                FetchAndStoreAllEarningStatementsFromStock);
        }

        public bool FetchAndStoreAllEarningStatementsFromStock(string ticker)
        {
            // get response from client
            List<List<string>> earningStatements = _client.GetEarningStatementsByStock(ticker);

            var lastUpdate = _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "Reports");
            DateTime lastUpdatedDate = lastUpdate?.LastUpdated ?? DateTime.MinValue;

            // loop through every earning statement in ticker
            foreach (var earningStatementItem in earningStatements)
            {
                DateTime date = DateTime.TryParse(earningStatementItem[0], out var fde) ? fde : DateTime.MinValue;

                if (date < lastUpdatedDate)
                {
                    Console.WriteLine("Data already up to date. Bye!!");
                    break;
                }

                try
                {
                    // create earning statement object 
                    var earningStatement = new StockEarningStatement
                    {
                        Ticker = ticker,
                        FiscalDateEnding = date,
                        ReportedDate =
                            DateTime.TryParse(earningStatementItem[1], out var rde) ? rde : DateTime.MinValue,
                        ReportedEPS = decimal.TryParse(earningStatementItem[3], out var reps) ? reps : 0,
                        estimatedEPS = decimal.TryParse(earningStatementItem[4], out var eeps) ? eeps : 0,
                        Surprise = decimal.TryParse(earningStatementItem[4], out var spr) ? spr : 0,
                        SuprisePercentage = decimal.TryParse(earningStatementItem[5], out var spp) ? spp : 0,
                        ReportTime = earningStatementItem[6]
                    };

                    // Check if the entry exists in DB
                    var existingEntry = _context.StockEarnings
                        .AsNoTracking()
                        .FirstOrDefault(b => b.Ticker == ticker && b.FiscalDateEnding == date);

                    if (existingEntry != null)
                    {
                        Console.WriteLine($"⏩ Skipping {ticker} (Date: {date}) - Already Exists in DB");
                        continue; // Skip adding the duplicate entry
                    }

                    // Check if EF Core is tracking a duplicate
                    var duplicateEntry = _context.ChangeTracker.Entries<StockEarningStatement>()
                        .FirstOrDefault(e => e.Entity.Ticker == ticker && e.Entity.FiscalDateEnding == date);

                    if (duplicateEntry != null)
                    {
                        Console.WriteLine($"🛑 Removing duplicate from tracker: {ticker} ({date})");
                        _context.Entry(duplicateEntry.Entity).State = EntityState.Detached;
                    }

                    // Add the new entry
                    _context.StockEarnings.Add(earningStatement);
                    Console.WriteLine($"✅ Added Earning Statement for: {ticker} (date: {date})");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing Earning Statement for {ticker} in {date}: {ex.Message}");
                    continue;
                }
            }

            try
            {
                int savedChanges = _context.SaveChanges();
                Console.WriteLine($"✅ Successfully saved {savedChanges} Earning Statement for {ticker}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database error when saving: {ex.Message}");
                return false;
            }

            return true;
        }

        public bool FetchAndStoreAllCashFlowStatementsFromStock(string ticker)
        {
            // get response from client
            List<List<string>> cashFlowStatements = _client.GetCashFlowStatementsByStock(ticker);

            var lastUpdate = _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "Reports");
            DateTime lastUpdatedDate = lastUpdate?.LastUpdated ?? DateTime.MinValue;

            // loop through every cash flow statement in ticker
            foreach (var cashFlowStatementItem in cashFlowStatements)
            {
                Console.WriteLine(cashFlowStatementItem[24]);
                DateTime date = DateTime.TryParse(cashFlowStatementItem[0], out var fde) ? fde : DateTime.MinValue;

                if (date < lastUpdatedDate)
                {
                    Console.WriteLine("Data already up to date. Bye!!");
                    break;
                }

                try
                {
                    // create cash flow statement object 
                    var cashFlow = new CashFlowStatement
                    {
                        Ticker = ticker,
                        FiscalDateEnding = date,
                        ReportedCurrency = cashFlowStatementItem[1],
                        OperatingCashFlow = decimal.TryParse(cashFlowStatementItem[2], out var ocf) ? ocf : 0,
                        PaymentsForOperatingActivities =
                            decimal.TryParse(cashFlowStatementItem[3], out var pfoa) ? pfoa : 0,
                        ProceedsFromOperatingActivities =
                            decimal.TryParse(cashFlowStatementItem[4], out var pfoaVal) ? pfoaVal : 0,
                        ChangeInOperatingLiabilities =
                            decimal.TryParse(cashFlowStatementItem[5], out var ciol) ? ciol : 0,
                        ChangeInOperatingAssets = decimal.TryParse(cashFlowStatementItem[6], out var coas) ? coas : 0,
                        DepreciationDepletionAndAmortization =
                            decimal.TryParse(cashFlowStatementItem[7], out var dda) ? dda : 0,
                        CapitalExpenditures = decimal.TryParse(cashFlowStatementItem[8], out var ce) ? ce : 0,
                        ChangeInReceivables = decimal.TryParse(cashFlowStatementItem[9], out var cir) ? cir : 0,
                        ChangeInInventory = decimal.TryParse(cashFlowStatementItem[10], out var cii) ? cii : 0,
                        ProfitLoss = decimal.TryParse(cashFlowStatementItem[11], out var pl) ? pl : 0,
                        CashFlowFromInvestment = decimal.TryParse(cashFlowStatementItem[12], out var cfi) ? cfi : 0,
                        CashFlowFromFinancing = decimal.TryParse(cashFlowStatementItem[13], out var cff) ? cff : 0,
                        ProceedsFromRepaymentsOfShortTermDebt =
                            decimal.TryParse(cashFlowStatementItem[14], out var prsd) ? prsd : 0,
                        PaymentsForRepurchaseOfCommonStock =
                            decimal.TryParse(cashFlowStatementItem[15], out var prcs) ? prcs : 0,
                        PaymentsForRepurchaseOfEquity =
                            decimal.TryParse(cashFlowStatementItem[16], out var prce) ? prce : 0,
                        PaymentsForRepurchaseOfPreferredStock =
                            decimal.TryParse(cashFlowStatementItem[17], out var prps) ? prps : 0,
                        DividendPayout = decimal.TryParse(cashFlowStatementItem[18], out var dp) ? dp : 0,
                        DividendPayoutCommonStock =
                            decimal.TryParse(cashFlowStatementItem[19], out var dpcs) ? dpcs : 0,
                        DividendPayoutPreferredStock =
                            decimal.TryParse(cashFlowStatementItem[20], out var dpps) ? dpps : 0,
                        ProceedsFromIssuanceOfCommonStock =
                            decimal.TryParse(cashFlowStatementItem[21], out var pfcs) ? pfcs : 0,
                        ProceedsFromIssuanceOfLongTermDebtAndCapitalSecuritiesNet =
                            decimal.TryParse(cashFlowStatementItem[22], out var pfldn) ? pfldn : 0,
                        ProceedsFromIssuanceOfPreferredStock =
                            decimal.TryParse(cashFlowStatementItem[23], out var pfps) ? pfps : 0,
                        ProceedsFromRepurchaseOfEquity =
                            decimal.TryParse(cashFlowStatementItem[24], out var prceq) ? prceq : 0,
                        ProceedsFromSaleOfTreasuryStock =
                            decimal.TryParse(cashFlowStatementItem[25], out var pfsts) ? pfsts : 0,
                        ChangeInCashAndCashEquivalents =
                            decimal.TryParse(cashFlowStatementItem[26], out var cce) ? cce : 0,
                        ChangeInExchangeRate = decimal.TryParse(cashFlowStatementItem[27], out var cer) ? cer : 0,
                        NetIncome = decimal.TryParse(cashFlowStatementItem[28], out var ni) ? ni : 0
                    };


                    // Check if the entry exists in DB
                    var existingEntry = _context.CashFlowStatements
                        .AsNoTracking()
                        .FirstOrDefault(b => b.Ticker == ticker && b.FiscalDateEnding == date);

                    if (existingEntry != null)
                    {
                        Console.WriteLine($"⏩ Skipping {ticker} (Year: {date}) - Already Exists in DB");
                        continue; // Skip adding the duplicate entry
                    }

                    // Check if EF Core is tracking a duplicate
                    var duplicateEntry = _context.ChangeTracker.Entries<CashFlowStatement>()
                        .FirstOrDefault(e => e.Entity.Ticker == ticker && e.Entity.FiscalDateEnding == date);

                    if (duplicateEntry != null)
                    {
                        Console.WriteLine($"🛑 Removing duplicate from tracker: {ticker} ({date})");
                        _context.Entry(duplicateEntry.Entity).State = EntityState.Detached;
                    }

                    // Add the new entry
                    _context.CashFlowStatements.Add(cashFlow);
                    Console.WriteLine($"✅ Added Cash Flow Statement for: {ticker} (date: {date})");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing Cash Flow Statement for {ticker} in {date}: {ex.Message}");
                    continue;
                }
            }

            try
            {
                int savedChanges = _context.SaveChanges();
                Console.WriteLine($"✅ Successfully saved {savedChanges} Cash Flow Statement for {ticker}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database error when saving: {ex.Message}");
                return false;
            }

            return true;
        }

public bool FetchAndStoreAllBalanceSheetsFromStock(string ticker)
{
	var trackDate = false;
    try
    {
        List<List<string>> sheets = _client.GetBalanceSheetsByStock(ticker);

        var lastUpdate = _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "Reports");
        DateTime lastUpdatedDate = lastUpdate?.LastUpdated ?? DateTime.MinValue;

        if (sheets == null || sheets.Count == 0)
        {
            Console.WriteLine($"No balance sheet data found for {ticker}.");
            return false;
        }


        var existingSheets = _context.StockBalanceSheets
            .Where(b => b.Ticker == ticker)
            .ToDictionary(b => b.FiscalYear); // FiscalYear → entity

        List<StockBalanceSheet> newEntries = new();

        foreach (var sheet in sheets)
        {
            try
            {
                int year = int.Parse(sheet[0].Substring(0, 4));

                if (year < lastUpdatedDate.Year && trackDate)
                {
                    Console.WriteLine("Data already up to date. Bye!!");
                    break;
                }

                var balanceSheet = new StockBalanceSheet
                {
                    Ticker = ticker,
                    FiscalYear = year,
                    ReportedCurrency = sheet[1],
                    TotalAssets = ParseDecimal(sheet[2]),
                    TotalCurrentAssets = ParseDecimal(sheet[3]),
                    CashAndCashEquivalents = ParseDecimal(sheet[4]),
                    CashAndShortTermInvestments = ParseDecimal(sheet[5]),
                    Inventory = ParseDecimal(sheet[6]),
                    CurrentNetReceivables = ParseDecimal(sheet[7]),
                    TotalNonCurrentAssets = ParseDecimal(sheet[8]),
                    PropertyPlantEquipment = ParseDecimal(sheet[9]),
                    IntangibleAssets = ParseDecimal(sheet[11]),
                    Goodwill = ParseDecimal(sheet[13]),
                    Investments = ParseDecimal(sheet[14]),
                    LongTermInvestments = ParseDecimal(sheet[15]),
                    ShortTermInvestments = ParseDecimal(sheet[16]),
                    OtherCurrentAssets = ParseDecimal(sheet[18]),
                    TotalLiabilities = ParseDecimal(sheet[19]),
                    TotalCurrentLiabilities = ParseDecimal(sheet[20]),
                    CurrentAccountsPayable = ParseDecimal(sheet[21]),
                    DeferredRevenue = ParseDecimal(sheet[22]),
                    CurrentDebt = ParseDecimal(sheet[23]),
                    ShortTermDebt = ParseDecimal(sheet[24]),
                    TotalNonCurrentLiabilities = ParseDecimal(sheet[25]),
                    LongTermDebt = ParseDecimal(sheet[27]),
                    OtherCurrentLiabilities = ParseDecimal(sheet[31]),
                    TotalShareholderEquity = ParseDecimal(sheet[33]),
                    TreasuryStock = ParseDecimal(sheet[34]),
                    RetainedEarnings = ParseDecimal(sheet[35]),
                    CommonStock = ParseDecimal(sheet[36]),
                    CommonStockSharesOutstanding = long.TryParse(sheet[37], out var cso) ? cso : 0
                };

                if (existingSheets.TryGetValue(year, out var existing))
                {
                    // Update existing values
                    _context.Entry(existing).CurrentValues.SetValues(balanceSheet);
                }
                else
                {
                    newEntries.Add(balanceSheet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing balance sheet for {ticker}: {ex.Message}");
                continue;
            }
        }

        if (newEntries.Count > 0)
        {
            _context.StockBalanceSheets.AddRange(newEntries);
        }

       	 	_context.SaveChanges();
        	Console.WriteLine($"✅ Successfully inserted/updated {sheets.Count} balance sheets for {ticker}.");

        	return true;
    	}
    	catch (Exception ex)
    	{
        	Console.WriteLine($"❌ Error syncing balance sheets for {ticker}: {ex.Message}");
        	return false;
    	}
	}	

	private decimal ParseDecimal(string input)
	{
    	return decimal.TryParse(input, out var val) ? val : 0;
	}




        public bool FetchAndStoreAllIncomeStatementsFromStock(string ticker)
        {
            // get response from client
            List<List<string>> incomeStatements = _client.GetIncomeStatementsByStock(ticker);

            var lastUpdate = _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "Reports");
            DateTime lastUpdatedDate = lastUpdate?.LastUpdated ?? DateTime.MinValue;

            // loop through income statement sheet in ticker
            foreach (var incomeStatementItem in incomeStatements)
            {
                DateTime date = DateTime.TryParse(incomeStatementItem[0], out var fde) ? fde : DateTime.MinValue;

                if (date < lastUpdatedDate)
                {
                    Console.WriteLine("Data already up to date. Bye!!");
                    break;
                }

                try
                {
                    // create income statement object 
                    var incomeStatement = new IncomeStatement
                    {
                        Ticker = ticker,
                        FiscalDateEnding = date,
                        ReportedCurrency = incomeStatementItem[1],
                        GrossProfit = decimal.TryParse(incomeStatementItem[2], out var gp) ? gp : 0,
                        TotalRevenue = decimal.TryParse(incomeStatementItem[3], out var tr) ? tr : 0,
                        CostOfRevenue = decimal.TryParse(incomeStatementItem[4], out var cor) ? cor : 0,
                        CostOfGoodsAndServicesSold = decimal.TryParse(incomeStatementItem[5], out var cgs) ? cgs : 0,
                        OperatingIncome = decimal.TryParse(incomeStatementItem[6], out var oi) ? oi : 0,
                        SellingGeneralAndAdministrative =
                            decimal.TryParse(incomeStatementItem[7], out var sgna) ? sgna : 0,
                        ResearchAndDevelopment = decimal.TryParse(incomeStatementItem[8], out var rnd) ? rnd : 0,
                        OperatingExpenses = decimal.TryParse(incomeStatementItem[9], out var oe) ? oe : 0,
                        InvestmentIncomeNet = incomeStatementItem[10],
                        NetInterestIncome = decimal.TryParse(incomeStatementItem[11], out var nii) ? nii : 0,
                        InterestIncome = decimal.TryParse(incomeStatementItem[12], out var ii) ? ii : 0,
                        InterestExpense = decimal.TryParse(incomeStatementItem[13], out var ie) ? ie : 0,
                        NonInterestIncome = decimal.TryParse(incomeStatementItem[14], out var nii2) ? nii2 : 0,
                        OtherNonOperatingIncome = decimal.TryParse(incomeStatementItem[15], out var onoi) ? onoi : 0,
                        Depreciation = decimal.TryParse(incomeStatementItem[16], out var dep) ? dep : 0,
                        DepreciationAndAmortization = decimal.TryParse(incomeStatementItem[17], out var da) ? da : 0,
                        IncomeBeforeTax = decimal.TryParse(incomeStatementItem[18], out var ibt) ? ibt : 0,
                        IncomeTaxExpense = decimal.TryParse(incomeStatementItem[19], out var itx) ? itx : 0,
                        InterestAndDebtExpense = incomeStatementItem[20],
                        NetIncomeFromContinuingOperations =
                            decimal.TryParse(incomeStatementItem[21], out var nic) ? nic : 0,
                        ComprehensiveIncomeNetOfTax =
                            decimal.TryParse(incomeStatementItem[22], out var cinot) ? cinot : 0,
                        Ebit = decimal.TryParse(incomeStatementItem[23], out var ebit) ? ebit : 0,
                        Ebitda = decimal.TryParse(incomeStatementItem[24], out var ebitda) ? ebitda : 0,
                        NetIncome = decimal.TryParse(incomeStatementItem[25], out var ni) ? ni : 0
                    };


                    // Check if the entry exists in DB
                    var existingEntry = _context.IncomeStatements
                        .AsNoTracking()
                        .FirstOrDefault(b => b.Ticker == ticker && b.FiscalDateEnding == date);

                    if (existingEntry != null)
                    {
                        Console.WriteLine($"⏩ Skipping {ticker} (Date: {date}) - Already Exists in DB");
                        continue; // Skip adding the duplicate entry
                    }

                    // Check if EF Core is tracking a duplicate
                    var duplicateEntry = _context.ChangeTracker.Entries<IncomeStatement>()
                        .FirstOrDefault(e => e.Entity.Ticker == ticker && e.Entity.FiscalDateEnding == date);

                    if (duplicateEntry != null)
                    {
                        Console.WriteLine($"🛑 Removing duplicate from tracker: {ticker} ({date})");
                        _context.Entry(duplicateEntry.Entity).State = EntityState.Detached;
                    }

                    // Add the new entry
                    _context.IncomeStatements.Add(incomeStatement);
                    Console.WriteLine($"✅ Added Income Statement for: {ticker} (date: {date})");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing Income Statement for {ticker} in {date}: {ex.Message}");
                    continue;
                }
            }

            try
            {
                int savedChanges = _context.SaveChanges();
                Console.WriteLine($"✅ Successfully saved {savedChanges} Income Statement for {ticker}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database error when saving: {ex.Message}");
                return false;
            }

            return true;
        }

        public void FetchAndStoreArticlesBySector(string sectorName, DateTime to)
        {
            try
            {
                // Get all stocks in the specified sector
                List<string> stockTickers = _context.Stocks
                    .Where(s => s.Sector.Name == sectorName)
                    .Select(s => s.Ticker)
                    .ToList();

                DateTime from =
                    _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "Articles")?.LastUpdated ??
                    DateTime.UtcNow;

                foreach (var ticker in stockTickers)
                {
                    try
                    {
                        Console.WriteLine($"Fetching articles for stock: {ticker}");
                        bool success = FetchAndStoreArticlesByStock(ticker,new DateTime(2025,01,01), new DateTime(2023,12,31));
                        Console.WriteLine($"✅ Successfully fetched and stored articles for {ticker}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error processing stock {ticker}: {ex.Message}");
                        Console.WriteLine("Continuing to the next stock...");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Critical error in FetchAndStoreArticlesBySector: {ex.Message}");
            }
        }
		
 public bool FetchAndStoreArticlesByStock(string ticker, DateTime from, DateTime to, bool trackDate = true)
        {
            try
            {
                var lastUpdate = _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "Articles");
                DateTime lastUpdatedDate = lastUpdate?.LastUpdated ?? DateTime.MinValue;

                // Fetch all relevant existing articles **once** into a HashSet for quick lookup
                var existingArticles = _context.Articles
                    .Where(a => a.Ticker == ticker && a.Date >= from && a.Date <= to)
                    .Select(a => new { a.ArticleName, a.Date }) // Select only necessary fields
                    .AsNoTracking()
                    .ToHashSet();

                List<Article> articlesToInsert = new List<Article>(); // Store new articles for batch insert

                List<Tuple<DateTime, string, string, Decimal, Decimal>> articles = _client.GetNewsSentimentByStock(ticker, lastUpdatedDate, DateTime.UtcNow, 0.05);

                foreach (var article in articles)
                {
                    
                    DateTime articleDate = article.Item1.Date;
                    string articleTitle = article.Item2.Length > 255
                        ? article.Item2.Substring(0, 255)
                        : article.Item2;
                    string articleUrl = article.Item3;
                    decimal sentimentScore = (decimal)Convert.ToDouble(article.Item4, CultureInfo.InvariantCulture);
                    decimal relevanceScore = (decimal)Convert.ToDouble(article.Item5, CultureInfo.InvariantCulture);

                    string articleKey = $"{articleTitle}_{articleDate}";

                    if (articleDate < lastUpdatedDate && trackDate)
                    {
                        break;
                    }
                    
                    if (existingArticles.Contains(new { ArticleName = articleTitle, Date = articleDate }))
                    {
                        Console.WriteLine($"Skipping duplicate: {articleTitle} (Date: {articleDate:yyyy-MM-dd})");
                        continue;
                    }

                    var newsEntry = new Article
                    {
                        Ticker = ticker,
                        ArticleName = articleTitle,
                        Date = articleDate,
                        SentimentScore = sentimentScore,
                        URL = articleUrl,
                        RelevanceScore = relevanceScore
                    };
                     
                    
                    articlesToInsert.Add(newsEntry);
                    existingArticles.Add(new { ArticleName = articleTitle, Date = articleDate }); 
                }
                
                if (articlesToInsert.Count > 0)
                {
                    _context.Articles.AddRange(articlesToInsert);
                    _context.SaveChanges();
                    Console.WriteLine($"✅ Successfully saved {articlesToInsert.Count} new articles.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error occurred: {ex.Message}");
                _context.ChangeTracker.Clear();
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"👉 Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
        
        public bool AddStockClosingPricePerSector(string sectorName)
        {
            try
            {
                // Fetch all stocks from the specified sector
                List<string> stockTickers = _context.Stocks
                    .Where(s => s.Sector.Name == sectorName)
                    .Select(s => s.Ticker)
                    .ToList();

                foreach (var ticker in stockTickers)
                {
                    try
                    {
                        // For each ticker, fetch and store the stock closing price
                        AddStockClosingPrice(ticker);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding stock closing price {ticker}: {ex.Message}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding stock closing prices for sector {sectorName}: {ex.Message}");
                return false;
            }
        }
        
        public bool AddStockClosingPrice(string ticker, bool trackDate = true)
        {
            try
            {
                // Fetch the closing stock data for the given ticker using the client
                List<Tuple<DateTime, decimal>> stockData = _client.GetClosingStockDataDaily(ticker);

                var lastUpdate = _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "StockData");
                DateTime lastUpdatedDate = lastUpdate?.LastUpdated ?? DateTime.MinValue;

                if (stockData == null || !stockData.Any())
                {
                    Console.WriteLine($"No closing price data found for {ticker}.");
                    return false;
                }

                // Fetch existing stock data for quick lookup
                var existingStockData = _context.StockDataEntries
                    .Where(sd => sd.Ticker == ticker)
                    .Select(sd => new { sd.Date, sd.ClosingPrice })
                    .AsNoTracking()
                    .ToDictionary(sd => sd.Date, sd => sd.ClosingPrice);

                List<StockData> newEntries = new List<StockData>();
                bool updatedExisting = false;

                foreach (var closingPriceData in stockData)
                {
                    DateTime dataDate = closingPriceData.Item1.Date;
                    decimal closingPrice = closingPriceData.Item2;

                    if (dataDate < lastUpdatedDate && trackDate)
                    {
                        Console.WriteLine("Data already up to date. Bye!!");
                        break;
                    }
                    

                    if (existingStockData.TryGetValue(dataDate, out var existingPrice))
                    {
                        if (existingPrice == 0 || existingPrice == null)
                        {
                            var stockToUpdate = _context.StockDataEntries.FirstOrDefault(sd => sd.Ticker == ticker && sd.Date == dataDate);
                            if (stockToUpdate != null)
                            {
                                stockToUpdate.ClosingPrice = closingPrice;
                                updatedExisting = true;
                                Console.WriteLine($"Updated closing price for {ticker} on {dataDate:yyyy-MM-dd}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Stock data for {ticker} on {dataDate:yyyy-MM-dd} already exists with a valid closing price.");
                        }
                    }
                    else
                    {
                        // Create a new stock data entry
                        var stockDataEntry = new StockData
                        {
                            Ticker = ticker,
                            Date = dataDate,
                            ClosingPrice = closingPrice,
                            MarketCap = 0,
                            PublicSentiment = 0
                        };

                        newEntries.Add(stockDataEntry);
                    }
                }

                if (newEntries.Count > 0)
                {
                    _context.StockDataEntries.AddRange(newEntries);
                    Console.WriteLine($"✅ Added {newEntries.Count} new stock closing prices for {ticker}.");
                }

                if (updatedExisting || newEntries.Count > 0)
                {
                    _context.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding stock closing prices for {ticker}: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }


        public void FetchAndStoreAllEconomicData()
        {
            try
            {
                List<string> economicIndicators = new List<string>
                {
                    "GDP",
                    "GDPPC",
                    "10Y-TCMR",
                    "30Y-TCMR",
                    "7Y-TCMR",
                    "5Y-TCMR",
                    "2Y-TCMR",
                    "3M-TCMR",
                    "FFR",
                    "CPI",
                    "Inflation",
                    "RetailSales",
                    "Durables",
                    "Unemployment",
                    "TNP"
                };

                foreach (var indicator in economicIndicators)
                {
                    try
                    {
                        FetchAndStoreEconomicData(indicator);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to Fetch and Store Data for {indicator}: {ex.Message}");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to Fetch and Store Economic Data");
            }
            
            Console.WriteLine("Fetched and Stored Economic Data");
        }

        public bool FetchAndStoreEconomicData(string indicator)
        {
            Console.WriteLine($"indicator: {indicator}");
            List<Tuple<DateTime, decimal>> ecoData = _client.GetEconomicData(indicator);

			var lastUpdate = _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "EcoIndicators");
			DateTime lastUpdatedDate = lastUpdate?.LastUpdated ?? DateTime.MinValue;
            
            foreach (var dataPointItem in ecoData)
            {
                DateTime date = new DateTime();
                decimal value;
                
                try
                {
                    date = dataPointItem.Item1;
                    value = dataPointItem.Item2;

					if(date < lastUpdatedDate) 
					{	
						Console.WriteLine("Data already up to date. Bye!!");
						break;
					}
                    
                    var ecoDataPoint = new EconomicData
                    {
                        IndicatorName = indicator,
                        Date = date,
                        Value = value
                    };

                    // Check if the entry exists in DB
                    var existingEntry = _context.EconomicDataPoints
                        .AsNoTracking()
                        .FirstOrDefault(b => b.IndicatorName == indicator && b.Date == date);

                    if (existingEntry != null)
                    {
                        Console.WriteLine($"⏩ Skipping  {indicator} (Date: {date}) - Already Exists in DB");
                        continue; // Skip adding the duplicate entry
                    }

                    // Check if EF Core is tracking a duplicate
                    var duplicateEntry = _context.ChangeTracker.Entries<EconomicData>()
                        .FirstOrDefault(b => b.Entity.IndicatorName == indicator && b.Entity.Date == date);

                    if (duplicateEntry != null)
                    {
                        Console.WriteLine($"🛑 Removing duplicate from indicator: {indicator} ({date})");
                        _context.Entry(duplicateEntry.Entity).State = EntityState.Detached;
                    }

                    _context.EconomicDataPoints.Add(ecoDataPoint);
                    Console.WriteLine($"Added {indicator}, Val: {value}, Date: {date}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to store vals for {indicator} on {date} {ex.Message}");
                }
            }
            try
            {
                int savedChanges = _context.SaveChanges();
                Console.WriteLine($"✅ Successfully saved {savedChanges} for {indicator}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database error when saving: {ex.Message}");
                return false;
            }
            return true;
            
        }

		public void FetchAndStoreAllCommodityData()
        {
            try
            {
                List<string> commodities = new List<string>
                {
                    "WTI",
                    "BRENT",
                    "NATURAL_GAS",
                    "COPPER",
                    "ALUMINUM",
                    "WHEAT",
                    "CORN",
                    "COTTON",
                    "SUGAR",
                    "COFFEE",
                    "ALL_COMMODITIES"
                };

                foreach (var commodity in commodities)
                {
                    try
                    {
                        FetchAndStoreCommodityData(commodity);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to Fetch and Store Data for {commodity}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to Fetch and Store Commodity Data");
            }

            Console.WriteLine("Fetched and Stored Commodity Data");
        }

        public bool FetchAndStoreCommodityData(string commodity)
        {
            Console.WriteLine($"Fetching commodity: {commodity}");
            List<Tuple<DateTime, decimal>> commodityData = _client.GetCommodityData(commodity);

			var lastUpdate = _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "Commodities");
			DateTime lastUpdatedDate = lastUpdate?.LastUpdated ?? DateTime.MinValue;

            foreach (var dataPointItem in commodityData)
            {
                DateTime date = dataPointItem.Item1;
                decimal value = dataPointItem.Item2;

				if(date < lastUpdatedDate) 
				{	
					Console.WriteLine("Data already up to date. Bye!!");
					break;
				}

                var commodityDataPoint = new CommodityData
                {
                    CommodityName = commodity,
                    Date = date,
                    Price = value
                };

                var existingEntry = _context.CommodityDataPoints
                    .AsNoTracking()
                    .FirstOrDefault(b => b.CommodityName == commodity && b.Date == date);

                if (existingEntry != null)
                {
                    Console.WriteLine($"⏩ Skipping {commodity} (Date: {date}) - Already Exists in DB");
                    continue;
                }

                _context.CommodityDataPoints.Add(commodityDataPoint);
                Console.WriteLine($"Added {commodity}, Price: {value}, Date: {date}");
            }

            try
            {
                int savedChanges = _context.SaveChanges();
                Console.WriteLine($"✅ Successfully saved {savedChanges} for {commodity}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database error when saving: {ex.Message}");

				if (ex.InnerException != null)
    			{
        			Console.WriteLine($"👉 Inner Exception: {ex.InnerException.Message}");
    			}
                return false;
            }
            return true;
        }
        
        public bool AddStock(string ticker, string sectorName)
        {
            var sector = _context.Sectors.FirstOrDefault(s => s.Name == sectorName);
            if (sector == null)
            {
                Console.WriteLine("Sector not found");
                return false;
            }
            
            var stock = _context.Stocks.FirstOrDefault(s => s.Ticker == ticker);
            
            Console.WriteLine($"Adding stock: {ticker} to sector: {sectorName}");
            
            if (stock == null)
            {
                // Create stock if it does not exist
                stock = new Stock
                {
                    Ticker = ticker,
                    SectorName = sectorName,
                };
                _context.Stocks.Add(stock);
            } else return false;
            
            
            _context.SaveChanges();
            Console.WriteLine($"Successfully added stock, ticker: {ticker}, sector: {sectorName}");
            return true;
        }
        

        private bool getExchangeRatesFor(string symbol)
        {
            List<Tuple<DateTime, Decimal>> exRates = _client.GetExchangeRateDaily("USD", symbol);
            
            var newRates = new List<ExchangeRate>();

			var lastUpdate = _context.DataUpdateTrackers.FirstOrDefault(d => d.DataType == "ExRates");
			DateTime lastUpdatedDate = lastUpdate?.LastUpdated ?? DateTime.MinValue;
            
            foreach (var rate in exRates)
            {
				DateTime date = rate.Item1.Date;
				if(date < lastUpdatedDate) 
				{	
					Console.WriteLine("Data already up to date. Bye!!");
					break;
				}

                // Check if the exchange rate already exists in the database
                bool exists = _context.ExchangeRates
                    .Any(r => r.Date == date && r.TargetCurrency == symbol);

                if (!exists)
                {
                    // Add new exchange rate
                    newRates.Add(new ExchangeRate
                    {
                        Date = date, 
                        TargetCurrency = symbol,
                        Rate = rate.Item2
                    });

					Console.WriteLine($"Added Exchange Rate for {symbol} @ {date}");
                }
				else 
				{
					Console.WriteLine($" Exchange Rate for {symbol} @ {date} already in db");
				}
            }
            
            if (newRates.Any())
            {
                _context.ExchangeRates.AddRange(newRates);
                _context.SaveChanges();
                Console.WriteLine($"Inserted {newRates.Count} new exchange rates for {symbol}.");
                return true; // Indicates new data was added
            }
            
            Console.WriteLine($"No new exchange rates for {symbol}, all data already exists.");
            return false; // No new data added
        }
        
        public void FetchAndStoreStockSplits(string ticker)
        {
            try
            {
                var stockSplits = _client.GetStockSplits(ticker);
                if (stockSplits.Count == 0)
                {
                    Console.WriteLine($"No stock splits found for {ticker}");
                    return;
                }

                foreach (var (date, splitRatioString) in stockSplits)
                {
                    
                    if (!decimal.TryParse(splitRatioString, out decimal splitRatio))
                    {
                        Console.WriteLine($"❌ Invalid split ratio '{splitRatioString}' for {ticker} on {date}");
                        continue;
                    }
                    
                    var existingSplit = _context.StockSplits
                        .FirstOrDefault(s => s.Ticker == ticker && s.EffectiveDate == date);

                    if (existingSplit == null)
                    {
                        _context.StockSplits.Add(new StockSplit
                        {
                            Ticker = ticker,
                            EffectiveDate = date,
                            SplitFactor = splitRatio
                        });
                    }
                }

                _context.SaveChanges();
                Console.WriteLine($"✅ Stock splits stored for {ticker}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching stock splits for {ticker}: {ex.Message}");
            }
        }

		public void BackfillMarketCap()
		{
			const decimal MaxMarketCap = 6_000_000_000_000m; 
			 Console.WriteLine($"Add Market Caps");

    		// Group stock data by year first
    		var stockDataGroupedByYear = _context.StockDataEntries
        		.Where(sd => sd.MarketCap == 0 && sd.ClosingPrice > 0)
        		.ToList()
        		.GroupBy(sd => sd.Date.Year)
        		.OrderBy(g => g.Key);

    		// Load all balance sheets once
    		var allBalanceSheets = _context.StockBalanceSheets
        		.Where(bs => bs.CommonStockSharesOutstanding > 0)
        		.ToList();

    		var balanceSheetsByTicker = allBalanceSheets
        		.GroupBy(bs => bs.Ticker)
        		.ToDictionary(g => g.Key, g => g.OrderByDescending(bs => bs.FiscalYear).ToList());

    		int totalUpdated = 0;

    		foreach (var yearlyGroup in stockDataGroupedByYear)
    		{
				//_context.ChangeTracker.Clear();
        		int year = yearlyGroup.Key;
        		int updatedThisYear = 0;

               if(year < 2012 && year > 2023) continue;

        		Console.WriteLine($"🔄 Processing year: {year}");

        		foreach (var data in yearlyGroup)
        		{
            		if (balanceSheetsByTicker.TryGetValue(data.Ticker, out var balanceSheets))
            		{
                		var bestMatch = balanceSheets
                    		.FirstOrDefault(bs => bs.FiscalYear <= data.Date.Year);
				
                			if (bestMatch != null)
                			{
								var newMarketCap = data.ClosingPrice * bestMatch.CommonStockSharesOutstanding;

                   			 	// Skip
                    			if (newMarketCap > MaxMarketCap)
                    			{
                        		//	Console.WriteLine($"⚠️ Skipped [{data.Ticker}] on {data.Date.ToShortDateString()} — MarketCap too large: {newMarketCap:N0}");
                        			continue;
                    			}
                			data.MarketCap = newMarketCap;
							_context.Entry(data).Property(sd => sd.MarketCap).IsModified = true;
                			updatedThisYear++;

						//	Console.WriteLine($"✅ Updated [{data.Ticker}] on {data.Date.ToShortDateString()} — MarketCap: {newMarketCap:N0} (Price: {data.ClosingPrice}, Shares: {bestMatch.CommonStockSharesOutstanding})");
                		}
            		}
        	  }

        		_context.SaveChanges(); 

        		totalUpdated += updatedThisYear;
        		Console.WriteLine($"✅ Year {year}: Updated {updatedThisYear} entries.");
    		}

    		Console.WriteLine($" Done! Total entries updated: {totalUpdated}");
		}
        
        public bool AddStockSplitsPricePerSector(string sectorName)
        {
            try
            {
                // Fetch all stocks from the specified sector
                List<string> stockTickers = _context.Stocks
                    .Where(s => s.Sector.Name == sectorName)
                    .Select(s => s.Ticker)
                    .ToList();

                foreach (var ticker in stockTickers)
                {
                    try
                    {
                        // For each ticker, fetch and store the stock split
                        FetchAndStoreStockSplits(ticker);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding stock splits {ticker}: {ex.Message}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding stock splits {sectorName}: {ex.Message}");
                return false;
            }
        }
    }
}
