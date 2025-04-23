using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
    // Quaterly
    public class CashFlowStatement
    {
        [Key, Column(Order = 0)]
        [Required]
        [StringLength(10)]
        public string Ticker { get; set; } // Stock Ticker

        [Key, Column(Order = 1)]
        [Required]
        public DateTime FiscalDateEnding { get; set; } // Fiscal Date Ending

        [Required]
        public string ReportedCurrency { get; set; }
        public decimal OperatingCashFlow { get; set; }
        public decimal PaymentsForOperatingActivities { get; set; }
        public decimal ProceedsFromOperatingActivities { get; set; }
        public decimal ChangeInOperatingLiabilities { get; set; }
        public decimal ChangeInOperatingAssets { get; set; }
        public decimal DepreciationDepletionAndAmortization { get; set; }
        public decimal CapitalExpenditures { get; set; }
        public decimal ChangeInReceivables { get; set; }
        public decimal ChangeInInventory { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal CashFlowFromInvestment { get; set; }
        public decimal CashFlowFromFinancing { get; set; }
        public decimal ProceedsFromRepaymentsOfShortTermDebt { get; set; }
        public decimal PaymentsForRepurchaseOfCommonStock { get; set; }
        public decimal PaymentsForRepurchaseOfEquity { get; set; }
        public decimal PaymentsForRepurchaseOfPreferredStock { get; set; }
        public decimal DividendPayout { get; set; }
        public decimal DividendPayoutCommonStock { get; set; }
        public decimal DividendPayoutPreferredStock { get; set; }
        public decimal ProceedsFromIssuanceOfCommonStock { get; set; }
        public decimal ProceedsFromIssuanceOfLongTermDebtAndCapitalSecuritiesNet { get; set; }
        public decimal ProceedsFromIssuanceOfPreferredStock { get; set; }
        public decimal ProceedsFromRepurchaseOfEquity { get; set; }
        public decimal ProceedsFromSaleOfTreasuryStock { get; set; }
        public decimal ChangeInCashAndCashEquivalents { get; set; }
        public decimal ChangeInExchangeRate { get; set; }
        public decimal NetIncome { get; set; }

        [ForeignKey("Ticker")]
        public virtual Stock Stock { get; set; } // Relationship with Stock entity
    }
}
