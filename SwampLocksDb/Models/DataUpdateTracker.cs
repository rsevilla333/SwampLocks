
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwampLocksDb.Models
{
    public class DataUpdateTracker
    {
        [Key] public string DataType { get; set; } // E.g., "Commodity_WTI", "Economic_GDP", "Stock_AAPL"

        public DateTime LastUpdated { get; set; } // Last successfully updated date
    }
}