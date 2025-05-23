﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SwampLocksDb.Data;

#nullable disable

namespace SwampLocks.Migrations
{
    [DbContext(typeof(FinancialContext))]
    partial class FinancialContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SwampLocksDb.Models.Article", b =>
                {
                    b.Property<string>("Ticker")
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("ArticleName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("RelevanceScore")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("SentimentScore")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("URL")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Ticker", "ArticleName", "Date");

                    b.ToTable("Articles");
                });

            modelBuilder.Entity("SwampLocksDb.Models.CashFlowStatement", b =>
                {
                    b.Property<string>("Ticker")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasColumnOrder(0);

                    b.Property<DateTime>("FiscalDateEnding")
                        .HasColumnType("datetime2")
                        .HasColumnOrder(1);

                    b.Property<decimal>("CapitalExpenditures")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CashFlowFromFinancing")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CashFlowFromInvestment")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ChangeInCashAndCashEquivalents")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ChangeInExchangeRate")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ChangeInInventory")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ChangeInOperatingAssets")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ChangeInOperatingLiabilities")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ChangeInReceivables")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("DepreciationDepletionAndAmortization")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("DividendPayout")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("DividendPayoutCommonStock")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("DividendPayoutPreferredStock")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("NetIncome")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("OperatingCashFlow")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PaymentsForOperatingActivities")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PaymentsForRepurchaseOfCommonStock")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PaymentsForRepurchaseOfEquity")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PaymentsForRepurchaseOfPreferredStock")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ProceedsFromIssuanceOfCommonStock")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ProceedsFromIssuanceOfLongTermDebtAndCapitalSecuritiesNet")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ProceedsFromIssuanceOfPreferredStock")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ProceedsFromOperatingActivities")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ProceedsFromRepaymentsOfShortTermDebt")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ProceedsFromRepurchaseOfEquity")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ProceedsFromSaleOfTreasuryStock")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ProfitLoss")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ReportedCurrency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Ticker", "FiscalDateEnding");

                    b.ToTable("CashFlowStatements");
                });

            modelBuilder.Entity("SwampLocksDb.Models.CommodityData", b =>
                {
                    b.Property<string>("CommodityName")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnOrder(0);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2")
                        .HasColumnOrder(1);

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("CommodityName", "Date");

                    b.ToTable("CommodityDataPoints");
                });

            modelBuilder.Entity("SwampLocksDb.Models.CommodityIndicator", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Interval")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Name");

                    b.ToTable("Commodities");
                });

            modelBuilder.Entity("SwampLocksDb.Models.DataUpdateTracker", b =>
                {
                    b.Property<string>("DataType")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.HasKey("DataType");

                    b.ToTable("DataUpdateTrackers");
                });

            modelBuilder.Entity("SwampLocksDb.Models.EconomicData", b =>
                {
                    b.Property<string>("IndicatorName")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnOrder(0);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2")
                        .HasColumnOrder(1);

                    b.Property<decimal>("Value")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("IndicatorName", "Date");

                    b.ToTable("EconomicDataPoints");
                });

            modelBuilder.Entity("SwampLocksDb.Models.EconomicIndicator", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Interval")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Name");

                    b.ToTable("EconomicIndicators");
                });

            modelBuilder.Entity("SwampLocksDb.Models.ExchangeRate", b =>
                {
                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("TargetCurrency")
                        .HasColumnType("nvarchar(450)");

                    b.Property<decimal>("Rate")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Date", "TargetCurrency");

                    b.ToTable("ExchangeRates");
                });

            modelBuilder.Entity("SwampLocksDb.Models.Holding", b =>
                {
                    b.Property<int>("HoldingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("HoldingId"));

                    b.Property<decimal>("Shares")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("HoldingId");

                    b.HasIndex("Ticker");

                    b.HasIndex("UserId");

                    b.ToTable("Holdings");
                });

            modelBuilder.Entity("SwampLocksDb.Models.IncomeStatement", b =>
                {
                    b.Property<string>("Ticker")
                        .HasColumnType("nvarchar(10)");

                    b.Property<DateTime>("FiscalDateEnding")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("ComprehensiveIncomeNetOfTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CostOfGoodsAndServicesSold")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CostOfRevenue")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Depreciation")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("DepreciationAndAmortization")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Ebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Ebitda")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("GrossProfit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IncomeBeforeTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IncomeTaxExpense")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("InterestAndDebtExpense")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("InterestExpense")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("InterestIncome")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("InvestmentIncomeNet")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("NetIncome")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("NetIncomeFromContinuingOperations")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("NetInterestIncome")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("NonInterestIncome")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("OperatingExpenses")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("OperatingIncome")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("OtherNonOperatingIncome")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ReportedCurrency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("ResearchAndDevelopment")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("SellingGeneralAndAdministrative")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalRevenue")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Ticker", "FiscalDateEnding");

                    b.ToTable("IncomeStatements");
                });

            modelBuilder.Entity("SwampLocksDb.Models.InterestRate", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Rate")
                        .HasColumnType("decimal(10,4)");

                    b.Property<string>("RateType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("InterestRates");
                });

            modelBuilder.Entity("SwampLocksDb.Models.Sector", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Name");

                    b.ToTable("Sectors");
                });

            modelBuilder.Entity("SwampLocksDb.Models.SectorPerformance", b =>
                {
                    b.Property<string>("SectorName")
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Performance")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("SectorName", "Date");

                    b.ToTable("SectorPerformances");
                });

            modelBuilder.Entity("SwampLocksDb.Models.SectorSentiment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SectorName")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Sentiment")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("SectorName", "Date")
                        .IsUnique();

                    b.ToTable("SectorSentiments");
                });

            modelBuilder.Entity("SwampLocksDb.Models.Stock", b =>
                {
                    b.Property<string>("Ticker")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<bool>("IsETF")
                        .HasColumnType("bit");

                    b.Property<string>("SectorName")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Ticker");

                    b.HasIndex("SectorName");

                    b.ToTable("Stocks");
                });

            modelBuilder.Entity("SwampLocksDb.Models.StockBalanceSheet", b =>
                {
                    b.Property<string>("Ticker")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasColumnOrder(0);

                    b.Property<int>("FiscalYear")
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    b.Property<decimal>("CashAndCashEquivalents")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CashAndShortTermInvestments")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CommonStock")
                        .HasColumnType("decimal(18,2)");

                    b.Property<long>("CommonStockSharesOutstanding")
                        .HasColumnType("bigint");

                    b.Property<decimal>("CurrentAccountsPayable")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CurrentDebt")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CurrentNetReceivables")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("DeferredRevenue")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Goodwill")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("IntangibleAssets")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Inventory")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Investments")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("LongTermDebt")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("LongTermInvestments")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("OtherCurrentAssets")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("OtherCurrentLiabilities")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PropertyPlantEquipment")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ReportedCurrency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("RetainedEarnings")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ShortTermDebt")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ShortTermInvestments")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalAssets")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalCurrentAssets")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalCurrentLiabilities")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalLiabilities")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalNonCurrentAssets")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalNonCurrentLiabilities")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalShareholderEquity")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TreasuryStock")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Ticker", "FiscalYear");

                    b.ToTable("StockBalanceSheets");
                });

            modelBuilder.Entity("SwampLocksDb.Models.StockData", b =>
                {
                    b.Property<string>("Ticker")
                        .HasColumnType("nvarchar(10)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("ClosingPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("MarketCap")
                        .HasColumnType("decimal(18,2)");

                    b.Property<double>("PublicSentiment")
                        .HasColumnType("float");

                    b.HasKey("Ticker", "Date");

                    b.ToTable("StockDataEntries");
                });

            modelBuilder.Entity("SwampLocksDb.Models.StockEarningStatement", b =>
                {
                    b.Property<string>("Ticker")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasColumnOrder(0);

                    b.Property<DateTime>("FiscalDateEnding")
                        .HasColumnType("datetime2")
                        .HasColumnOrder(1);

                    b.Property<string>("ReportTime")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ReportedDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("ReportedEPS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("SuprisePercentage")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("Surprise")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("estimatedEPS")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Ticker", "FiscalDateEnding");

                    b.ToTable("StockEarnings");
                });

            modelBuilder.Entity("SwampLocksDb.Models.StockSplit", b =>
                {
                    b.Property<string>("Ticker")
                        .HasColumnType("nvarchar(10)");

                    b.Property<DateTime>("EffectiveDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("SplitFactor")
                        .HasColumnType("decimal(10,4)");

                    b.HasKey("Ticker", "EffectiveDate");

                    b.ToTable("StockSplits");
                });

            modelBuilder.Entity("SwampLocksDb.Models.User", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("ProfilePicture")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SwampLocksDb.Models.Article", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Stock", "Stock")
                        .WithMany("Articles")
                        .HasForeignKey("Ticker")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("SwampLocksDb.Models.CashFlowStatement", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Stock", "Stock")
                        .WithMany("CashFlowStatements")
                        .HasForeignKey("Ticker")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("SwampLocksDb.Models.CommodityData", b =>
                {
                    b.HasOne("SwampLocksDb.Models.CommodityIndicator", "Commodity")
                        .WithMany("DataPoints")
                        .HasForeignKey("CommodityName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Commodity");
                });

            modelBuilder.Entity("SwampLocksDb.Models.EconomicData", b =>
                {
                    b.HasOne("SwampLocksDb.Models.EconomicIndicator", "Indicator")
                        .WithMany("DataPoints")
                        .HasForeignKey("IndicatorName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Indicator");
                });

            modelBuilder.Entity("SwampLocksDb.Models.Holding", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Stock", "Stock")
                        .WithMany()
                        .HasForeignKey("Ticker")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SwampLocksDb.Models.User", "User")
                        .WithMany("Holdings")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");

                    b.Navigation("User");
                });

            modelBuilder.Entity("SwampLocksDb.Models.IncomeStatement", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Stock", "Stock")
                        .WithMany("IncomeStatements")
                        .HasForeignKey("Ticker")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("SwampLocksDb.Models.SectorPerformance", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Sector", "Sector")
                        .WithMany("Performances")
                        .HasForeignKey("SectorName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sector");
                });

            modelBuilder.Entity("SwampLocksDb.Models.SectorSentiment", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Sector", "Sector")
                        .WithMany("Sentiments")
                        .HasForeignKey("SectorName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sector");
                });

            modelBuilder.Entity("SwampLocksDb.Models.Stock", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Sector", "Sector")
                        .WithMany("Stocks")
                        .HasForeignKey("SectorName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sector");
                });

            modelBuilder.Entity("SwampLocksDb.Models.StockBalanceSheet", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Stock", "Stock")
                        .WithMany("BalanceSheets")
                        .HasForeignKey("Ticker")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("SwampLocksDb.Models.StockData", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Stock", "Stock")
                        .WithMany("DataEntries")
                        .HasForeignKey("Ticker")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("SwampLocksDb.Models.StockEarningStatement", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Stock", "Stock")
                        .WithMany("EarningStatements")
                        .HasForeignKey("Ticker")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("SwampLocksDb.Models.StockSplit", b =>
                {
                    b.HasOne("SwampLocksDb.Models.Stock", "Stock")
                        .WithMany("StockSplits")
                        .HasForeignKey("Ticker")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("SwampLocksDb.Models.CommodityIndicator", b =>
                {
                    b.Navigation("DataPoints");
                });

            modelBuilder.Entity("SwampLocksDb.Models.EconomicIndicator", b =>
                {
                    b.Navigation("DataPoints");
                });

            modelBuilder.Entity("SwampLocksDb.Models.Sector", b =>
                {
                    b.Navigation("Performances");

                    b.Navigation("Sentiments");

                    b.Navigation("Stocks");
                });

            modelBuilder.Entity("SwampLocksDb.Models.Stock", b =>
                {
                    b.Navigation("Articles");

                    b.Navigation("BalanceSheets");

                    b.Navigation("CashFlowStatements");

                    b.Navigation("DataEntries");

                    b.Navigation("EarningStatements");

                    b.Navigation("IncomeStatements");

                    b.Navigation("StockSplits");
                });

            modelBuilder.Entity("SwampLocksDb.Models.User", b =>
                {
                    b.Navigation("Holdings");
                });
#pragma warning restore 612, 618
        }
    }
}
