using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwampLocksDb.Models
{
    public class CommodityIndicator
    {
        [Key]
        public string Name { get; set; } = string.Empty; // Primary Key, e.g., "Crude Oil WTI"

        public string Interval { get; set; } = string.Empty; // "daily", "weekly", "monthly", "quarterly", "annual"
        public string Unit { get; set; } = string.Empty; // Unit of measurement (e.g., "USD per Barrel")

        // Navigation Property
        public List<CommodityData> DataPoints { get; set; } = new List<CommodityData>();
    }
}