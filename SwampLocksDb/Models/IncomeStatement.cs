using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
    // Income Statement's object/table
    public class IncomeStatement
    {
        [Key]
        [Required]
        public string Ticker { get; set; } // Stock Ticker

        [Required]
        public DateTime FiscalDateEnding { get; set; } // Fiscal Date Ending

        [Required]
        public string ReportedCurrency { get; set; } // Reported Currency
        public decimal GrossProfit { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CostOfRevenue { get; set; }
        public decimal CostOfGoodsAndServicesSold { get; set; }
        public decimal OperatingIncome { get; set; }
        public decimal SellingGeneralAndAdministrative { get; set; }
        public decimal ResearchAndDevelopment { get; set; }
        public decimal OperatingExpenses { get; set; }
        public string InvestmentIncomeNet { get; set; } // Can be 'None' or a value
        public decimal NetInterestIncome { get; set; }
        public decimal InterestIncome { get; set; }
        public decimal InterestExpense { get; set; }
        public decimal NonInterestIncome { get; set; }
        public decimal OtherNonOperatingIncome { get; set; }
        public decimal Depreciation { get; set; }
        public decimal DepreciationAndAmortization { get; set; }
        public decimal IncomeBeforeTax { get; set; }
        public decimal IncomeTaxExpense { get; set; }
        public string InterestAndDebtExpense { get; set; } // Can be 'None' or a value
        public decimal NetIncomeFromContinuingOperations { get; set; }
        public decimal ComprehensiveIncomeNetOfTax { get; set; }
        public decimal Ebit { get; set; }
        public decimal Ebitda { get; set; }
        public decimal NetIncome { get; set; }

        // Foreign key to Stock entity (assuming you already have Stock entity defined)
        [ForeignKey("Ticker")]
        public virtual Stock Stock { get; set; }
    }
}