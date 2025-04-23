using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace SwampLocksDb.Models
{
	// Exchange Rate object/table
    public class ExchangeRate
    {
	//	[Key]
    //    public string Id { get; set; } 
        public DateTime Date { get; set; }

		public string TargetCurrency {get; set;}

		public Decimal Rate {get; set;}

		public ExchangeRate() { }

		public ExchangeRate(DateTime date, string targetCurrency, decimal rate)
    	{
        	Date = date;
        	TargetCurrency = targetCurrency;
        	Rate = rate;
    	}
    }
}