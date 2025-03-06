using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
public class Stock
{
    [Key]
    [Required]
    [StringLength(10)]
    public string Ticker { get; set; } // Unique Identifier for the Stock

    [Required]
    public string SectorName { get; set; }  

    [ForeignKey("SectorName")]
    public virtual Sector Sector { get; set; }

    public List<StockData> DataEntries { get; set; } = new List<StockData>(); // List of StockData (Per Date)

	public virtual List<Article> Articles { get; set; } = new List<Article>(); // articles related to stock (Per Date)
	
	public virtual List<StockBalanceSheet> BalanceSheets { get; set; } = new List<StockBalanceSheet>();

	public virtual List<CashFlowStatement> CashFlowStatements { get; set; } = new List<CashFlowStatement>();
	
	public virtual List<IncomeStatement> IncomeStatements { get; set; } = new List<IncomeStatement>();

}
}
