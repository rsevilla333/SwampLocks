using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SwampLocksDb.Data;
using SwampLocksDb.Models;
using SwampLocks.AlphaVantage.Client;
using SwampLocksDb.Models;
using SwampLocksDb.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Globalization;


namespace SwampLocks.AlphaVantage.Service
{
    public class AlphaVantageService
    {
        private readonly FinancialContext _context;
        private readonly AlphaVantageClient _client;

        public AlphaVantageService(FinancialContext context, AlphaVantageClient client)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public void PopulateExchangeRates()
        {
            getExchangeRatesFor("EUR");
            getExchangeRatesFor("JPY");
            getExchangeRatesFor("BTC");
            getExchangeRatesFor("CAD");
        }

        public void PopulateSectors()
        {
            PopulateSector("Communication Services", "XLC");  // Communication Services Sector
            PopulateSector("Consumer Discretionary", "XLY");  // Consumer Discretionary Sector  
            PopulateSector("Consumer Staples", "XLP");  // Consumer Staples Sector  
            PopulateSector("Energy", "XLE");  // Energy Sector  
            PopulateSector("Financials", "XLF");  // Financials Sector  
            PopulateSector("Healthcare", "XLV");  // Healthcare Sector  
            PopulateSector("Industrials", "XLI");  // Industrials Sector  
            PopulateSector("Information Technology", "XLK");  // Information Technology Sector  
            PopulateSector("Materials", "XLB");  // Materials Sector  
            PopulateSector("Real Estate", "XLRE");  // Real Estate Sector  
            PopulateSector("Utilities", "XLU");  // Utilities Sector  
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
        
        public bool FetchAndStoreAllReportsFromSector(string sectorName, string reportType, Func<string, bool> fetchAndStoreByStock)
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
            
            // loop through every earning statement in ticker
            foreach (var earningStatementItem in earningStatements)
            {
                DateTime date = DateTime.TryParse(earningStatementItem[0], out var fde) ? fde : DateTime.MinValue;
                
                try
                {
                    // create earning statement object 
                    var earningStatement = new StockEarningStatement
                    {
                        Ticker = ticker,
                        FiscalDateEnding = date,
                        ReportedDate = DateTime.TryParse(earningStatementItem[1], out var rde) ? rde : DateTime.MinValue,
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
            
            // loop through every cash flow statement in ticker
            foreach (var cashFlowStatementItem in cashFlowStatements)
            {
                DateTime date = DateTime.TryParse(cashFlowStatementItem[0], out var fde) ? fde : DateTime.MinValue;
                try
                {
                    // create cash flow statement object 
                    var cashFlow = new CashFlowStatement
                    {
                        Ticker = ticker,
                        FiscalDateEnding = date,
                        ReportedCurrency = cashFlowStatementItem[1],
                        OperatingCashFlow = decimal.TryParse(cashFlowStatementItem[2], out var ocf) ? ocf : 0,
                        PaymentsForOperatingActivities = decimal.TryParse(cashFlowStatementItem[3], out var pfoa) ? pfoa : 0,
                        ProceedsFromOperatingActivities = decimal.TryParse(cashFlowStatementItem[4], out var pfoaVal) ? pfoaVal : 0,
                        ChangeInOperatingLiabilities = decimal.TryParse(cashFlowStatementItem[5], out var ciol) ? ciol : 0,
                        ChangeInOperatingAssets = decimal.TryParse(cashFlowStatementItem[6], out var coas) ? coas : 0,
                        DepreciationDepletionAndAmortization = decimal.TryParse(cashFlowStatementItem[7], out var dda) ? dda : 0,
                        CapitalExpenditures = decimal.TryParse(cashFlowStatementItem[8], out var ce) ? ce : 0,
                        ChangeInReceivables = decimal.TryParse(cashFlowStatementItem[9], out var cir) ? cir : 0,
                        ChangeInInventory = decimal.TryParse(cashFlowStatementItem[10], out var cii) ? cii : 0,
                        ProfitLoss = decimal.TryParse(cashFlowStatementItem[11], out var pl) ? pl : 0,
                        CashFlowFromInvestment = decimal.TryParse(cashFlowStatementItem[12], out var cfi) ? cfi : 0,
                        CashFlowFromFinancing = decimal.TryParse(cashFlowStatementItem[13], out var cff) ? cff : 0,
                        ProceedsFromRepaymentsOfShortTermDebt = decimal.TryParse(cashFlowStatementItem[14], out var prsd) ? prsd : 0,
                        PaymentsForRepurchaseOfCommonStock = decimal.TryParse(cashFlowStatementItem[15], out var prcs) ? prcs : 0,
                        PaymentsForRepurchaseOfEquity = decimal.TryParse(cashFlowStatementItem[16], out var prce) ? prce : 0,
                        PaymentsForRepurchaseOfPreferredStock = decimal.TryParse(cashFlowStatementItem[17], out var prps) ? prps : 0,
                        DividendPayout = decimal.TryParse(cashFlowStatementItem[18], out var dp) ? dp : 0,
                        DividendPayoutCommonStock = decimal.TryParse(cashFlowStatementItem[19], out var dpcs) ? dpcs : 0,
                        DividendPayoutPreferredStock = decimal.TryParse(cashFlowStatementItem[20], out var dpps) ? dpps : 0,
                        ProceedsFromIssuanceOfCommonStock = decimal.TryParse(cashFlowStatementItem[21], out var pfcs) ? pfcs : 0,
                        ProceedsFromIssuanceOfLongTermDebtAndCapitalSecuritiesNet = decimal.TryParse(cashFlowStatementItem[22], out var pfldn) ? pfldn : 0,
                        ProceedsFromIssuanceOfPreferredStock = decimal.TryParse(cashFlowStatementItem[23], out var pfps) ? pfps : 0,
                        ProceedsFromRepurchaseOfEquity = decimal.TryParse(cashFlowStatementItem[24], out var prceq) ? prceq : 0,
                        ProceedsFromSaleOfTreasuryStock = decimal.TryParse(cashFlowStatementItem[25], out var pfsts) ? pfsts : 0,
                        ChangeInCashAndCashEquivalents = decimal.TryParse(cashFlowStatementItem[26], out var cce) ? cce : 0,
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
            // get response from client
            List<List<string>> sheets = _client.GetBalanceSheetsByStock(ticker);

            // loop through every balance sheet in ticker
            foreach (var sheet in sheets)
            {
                int year = 0;
                try
                {
                    year = int.Parse(sheet[0].Substring(0, 4));
                    
                    // create balance sheet object 
                    var balanceSheet = new StockBalanceSheet
                    {
                        Ticker = ticker,
                        FiscalYear = year,
                        ReportedCurrency = sheet[1],
                        TotalAssets = decimal.TryParse(sheet[2], out var ta) ? ta : 0,
                        TotalCurrentAssets = decimal.TryParse(sheet[3], out var tca) ? tca : 0,
                        CashAndCashEquivalents = decimal.TryParse(sheet[4], out var cec) ? cec : 0,
                        CashAndShortTermInvestments = decimal.TryParse(sheet[5], out var csi) ? csi : 0,
                        Inventory = decimal.TryParse(sheet[6], out var inv) ? inv : 0,
                        CurrentNetReceivables = decimal.TryParse(sheet[7], out var cnr) ? cnr : 0,
                        TotalNonCurrentAssets = decimal.TryParse(sheet[8], out var tnca) ? tnca : 0,
                        PropertyPlantEquipment = decimal.TryParse(sheet[9], out var ppe) ? ppe : 0,
                        IntangibleAssets = decimal.TryParse(sheet[11], out var ia) ? ia : 0,
                        Goodwill = decimal.TryParse(sheet[13], out var gw) ? gw : 0,
                        Investments = decimal.TryParse(sheet[14], out var invst) ? invst : 0,
                        LongTermInvestments = decimal.TryParse(sheet[15], out var lti) ? lti : 0,
                        ShortTermInvestments = decimal.TryParse(sheet[16], out var sti) ? sti : 0,
                        OtherCurrentAssets = decimal.TryParse(sheet[17], out var oca) ? oca : 0,
                        TotalLiabilities = decimal.TryParse(sheet[19], out var tl) ? tl : 0,
                        TotalCurrentLiabilities = decimal.TryParse(sheet[20], out var tcl) ? tcl : 0,
                        CurrentAccountsPayable = decimal.TryParse(sheet[21], out var cap) ? cap : 0,
                        DeferredRevenue = decimal.TryParse(sheet[22], out var dr) ? dr : 0,
                        CurrentDebt = decimal.TryParse(sheet[23], out var cd) ? cd : 0,
                        ShortTermDebt = decimal.TryParse(sheet[24], out var std) ? std : 0,
                        TotalNonCurrentLiabilities = decimal.TryParse(sheet[25], out var tncl) ? tncl : 0,
                        LongTermDebt = decimal.TryParse(sheet[27], out var ltd) ? ltd : 0,
                        OtherCurrentLiabilities = decimal.TryParse(sheet[30], out var ocl) ? ocl : 0,
                        TotalShareholderEquity = decimal.TryParse(sheet[32], out var tse) ? tse : 0,
                        TreasuryStock = decimal.TryParse(sheet[33], out var ts) ? ts : 0,
                        RetainedEarnings = decimal.TryParse(sheet[34], out var re) ? re : 0,
                        CommonStock = decimal.TryParse(sheet[35], out var cs) ? cs : 0,
                        CommonStockSharesOutstanding = long.TryParse(sheet[36], out var cso) ? cso : 0
                    };

                    // Check if the entry exists in DB
                    var existingEntry = _context.StockBalanceSheets
                        .AsNoTracking()
                        .FirstOrDefault(b => b.Ticker == ticker && b.FiscalYear == year);

                    if (existingEntry != null)
                    {
                        Console.WriteLine($"⏩ Skipping {ticker} (Year: {year}) - Already Exists in DB");
                        continue; // Skip adding the duplicate entry
                    }

                    // Check if EF Core is tracking a duplicate
                    var duplicateEntry = _context.ChangeTracker.Entries<StockBalanceSheet>()
                        .FirstOrDefault(e => e.Entity.Ticker == ticker && e.Entity.FiscalYear == year);

                    if (duplicateEntry != null)
                    {
                        Console.WriteLine($"🛑 Removing duplicate from tracker: {ticker} ({year})");
                        _context.Entry(duplicateEntry.Entity).State = EntityState.Detached;
                    }

                    // Add the new entry
                    _context.StockBalanceSheets.Add(balanceSheet);
                    Console.WriteLine($"✅ Added Balance Sheet for: {ticker} (Year: {year})");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing balance sheet for {ticker} in {year}: {ex.Message}");
                    continue;
                }
            }
            
            try
            {
                int savedChanges = _context.SaveChanges();
                Console.WriteLine($"✅ Successfully saved {savedChanges} balance sheets for {ticker}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database error when saving: {ex.Message}");
                return false;
            }

            return true;
        }

        public bool FetchAndStoreAllArticlesByStock(DateTime from, DateTime to)
        {
            // Retrieve all stocks from the database
            var stocks = _context.Stocks.ToList();
            
            if (stocks.Any())
            {
                foreach (var stock in stocks)
                {
                    FetchAndStoreArticlesByStock(stock.Ticker, from, to);
                }
            }
            else
            {
                Console.WriteLine("No stocks found in the database.");
            }
            
            return true;
        }
        
        public bool FetchAndStoreAllIncomeStatementsFromStock(string ticker)
        {
            // get response from client
            List<List<string>> incomeStatements = _client.GetIncomeStatementsByStock(ticker);
            
            // loop through income statement sheet in ticker
            foreach (var incomeStatementItem in incomeStatements)
            {
                DateTime date = DateTime.TryParse(incomeStatementItem[0], out var fde) ? fde : DateTime.MinValue;
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
                        SellingGeneralAndAdministrative = decimal.TryParse(incomeStatementItem[7], out var sgna) ? sgna : 0,
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
                        NetIncomeFromContinuingOperations = decimal.TryParse(incomeStatementItem[21], out var nic) ? nic : 0,
                        ComprehensiveIncomeNetOfTax = decimal.TryParse(incomeStatementItem[22], out var cinot) ? cinot : 0,
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
        
        public void FetchAndStoreArticlesBySector(string sectorName, DateTime from, DateTime to)
        {
            try
            {
               // Get all stocks in the specified sector
                List<string> stockTickers = _context.Stocks
                    .Where(s => s.Sector.Name == sectorName)
                    .Select(s => s.Ticker)
                    .ToList();
    
                foreach (var ticker in stockTickers)
                {
                    try
                    {
                        Console.WriteLine($"Fetching articles for stock: {ticker}");
                        FetchAndStoreArticlesByStock(ticker, from, to);
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
        
    public bool FetchAndStoreArticlesByStock(string ticker, DateTime from, DateTime to)
    {
        List<Tuple<DateTime, string, Decimal>> articles = _client.GetNewsSentimentByStock(ticker, from, to, 0.05);

        foreach (var article in articles)
        {

            DateTime articleDate = article.Item1.Date;
            string articleTitle = article.Item2;
            decimal sentimentScore = (decimal)Convert.ToDouble(article.Item3, CultureInfo.InvariantCulture);


            var newsEntry = new Article
            {
                Ticker = ticker,
                ArticleName = articleTitle,
                Date = articleDate,
                SentimentScore = sentimentScore
            };

            // Check if article already exists
            var existingArticle = _context.Articles
                .FirstOrDefault(a => a.Ticker == newsEntry.Ticker && a.ArticleName == newsEntry.ArticleName && a.Date == newsEntry.Date);

            if (existingArticle != null)
            {
                // If the article exists, skip this iteration and continue
                Console.WriteLine($"Article already exists: {existingArticle.ArticleName} (Date: {existingArticle.Date:yyyy-MM-dd})");
                _context.Entry(existingArticle).State = EntityState.Detached;
                continue;
            }

            var trackedArticle = _context.Entry(newsEntry);
            if (trackedArticle.State == EntityState.Detached)
            {
                _context.Articles.Add(newsEntry);
                Console.WriteLine($"Added: {newsEntry.ArticleName} (Date: {articleDate:yyyy-MM-dd}, sentiment: {sentimentScore})");
            }

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException dbEx)
            {
                // Catch the exception if a duplicate key violation occurs
                var sqlException = dbEx.InnerException as SqlException;
                if (sqlException != null && sqlException.Number == 2627)
                {
                    // Duplicate key error, just log it and continue
                    Console.WriteLine($"Duplicate key violation: {newsEntry.ArticleName} (Date: {articleDate:yyyy-MM-dd}). Skipping...");
                }
                else
                {
                    // Other DB update exceptions, log them for further investigation
                    Console.WriteLine($"Error saving changes: {dbEx.Message}");
                    
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                    }
                }
            }
        }

        return true;
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
        
        public bool AddStockClosingPrice(string ticker)
        {
            try
            {
                // Fetch the closing stock data for the given ticker using the client
                List<Tuple<DateTime, Decimal>> stockData = _client.GetClosingStockDataDaily(ticker);

                if (stockData == null || !stockData.Any())
                {
                    Console.WriteLine($"No closing price data found for {ticker}.");
                    return false;
                }

                foreach (var closingPriceData in stockData)
                {
                    DateTime dataDate = closingPriceData.Item1.Date;
                    decimal closingPrice = closingPriceData.Item2;
                    
                    if (dataDate.Year <= 2010)
                    {
                        Console.WriteLine($"Year {dataDate.Year} reached. Stopping execution.");
                        break; // Stop the loop if the year is 2009 or later
                    }
                    
                    // Check if stock data already exists for the given ticker and date
                    var existingStockData = _context.StockDataEntries
                        .FirstOrDefault(sd => sd.Ticker == ticker && sd.Date.Date == dataDate.Date);

                    if (existingStockData != null)
                    {
                        // If the existing closing price is 0 or null, update it
                        if (existingStockData.ClosingPrice == 0 || existingStockData.ClosingPrice == null)
                        {
                            existingStockData.ClosingPrice = closingPrice;
                            
                            _context.SaveChanges(); // Save changes to the database

                            Console.WriteLine($"Updated closing price for {ticker} on {dataDate.ToShortDateString()}");
                        }
                        else
                        {
                            Console.WriteLine($"Stock data for {ticker} on {dataDate.ToShortDateString()} already exists with a valid closing price.");
                        }
                    }
                    else
                    {
                        // Create a new StockData entry if it does not exist
                        var stockDataEntry = new StockData
                        {
                            Ticker = ticker,
                            Date = dataDate,
                            ClosingPrice = closingPrice,
                            MarketCap = 0, // default val
                            PublicSentiment = 0 // default val
                        };

                        // Add the new stock data to the context
                        _context.StockDataEntries.Add(stockDataEntry);
                        _context.SaveChanges(); // Save changes to the database

                        Console.WriteLine($"Added stock closing price for {ticker} on {dataDate.ToShortDateString()}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding stock closing prices for {ticker}: {ex.Message}");
                return false;
            }
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
            
            foreach (var rate in exRates)
            {
                // Check if the exchange rate already exists in the database
                bool exists = _context.ExchangeRates
                    .Any(r => r.Date == rate.Item1.Date && r.TargetCurrency == symbol);

                if (!exists)
                {
                    // Add new exchange rate
                    newRates.Add(new ExchangeRate
                    {
                        Date = rate.Item1.Date, 
                        TargetCurrency = symbol,
                        Rate = rate.Item2
                    });
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
    }
}
