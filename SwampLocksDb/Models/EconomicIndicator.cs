using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwampLocksDb.Models
{
    // Economic Indicator object/table
    public class EconomicIndicator
    {
        [Key] public string Name { get; set; } = string.Empty; // Primary Key

        public string Interval { get; set; } = string.Empty; // "monthly", "quarterly", "annual"
        public string Unit { get; set; } = string.Empty; // Unit of measurement

        // Navigation Property
        public List<EconomicData> DataPoints { get; set; } = new List<EconomicData>();
    }
}