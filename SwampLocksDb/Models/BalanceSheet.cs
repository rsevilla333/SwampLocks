using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
    public class StockBalanceSheet
    {
        [Key, Column(Order = 0)]
        [Required]
        [StringLength(10)]
        public string Ticker { get; set; } // Stock Ticker

        [Key, Column(Order = 1)]
        [Required]
        public int FiscalYear { get; set; } // Fiscal Year

        [Required]
        public string ReportedCurrency { get; set; }

        public decimal TotalAssets { get; set; }
        public decimal TotalCurrentAssets { get; set; }
        public decimal CashAndCashEquivalents { get; set; }
        public decimal CashAndShortTermInvestments { get; set; }
        public decimal Inventory { get; set; }
        public decimal CurrentNetReceivables { get; set; }
        public decimal TotalNonCurrentAssets { get; set; }
        public decimal PropertyPlantEquipment { get; set; }
        public decimal IntangibleAssets { get; set; }
        public decimal Goodwill { get; set; }
        public decimal Investments { get; set; }
        public decimal LongTermInvestments { get; set; }
        public decimal ShortTermInvestments { get; set; }
        public decimal OtherCurrentAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalCurrentLiabilities { get; set; }
        public decimal CurrentAccountsPayable { get; set; }
        public decimal DeferredRevenue { get; set; }
        public decimal CurrentDebt { get; set; }
        public decimal ShortTermDebt { get; set; }
        public decimal TotalNonCurrentLiabilities { get; set; }
        public decimal LongTermDebt { get; set; }
        public decimal OtherCurrentLiabilities { get; set; }
        public decimal TotalShareholderEquity { get; set; }
        public decimal TreasuryStock { get; set; }
        public decimal RetainedEarnings { get; set; }
        public decimal CommonStock { get; set; }
        public long CommonStockSharesOutstanding { get; set; }

        [ForeignKey("Ticker")]
        public virtual Stock Stock { get; set; } // Relationship with Stock entity
    }
}