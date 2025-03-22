using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
    public class EconomicData
    {
        [Key, Column(Order = 0)]
        [Required]
        public string IndicatorName { get; set; } = string.Empty; // Foreign Key

        [Key, Column(Order = 1)]
        [Required]
        public DateTime Date { get; set; } // Date of the data point

        [Required]
        public decimal Value { get; set; } // Value of the indicator on that date

        // Navigation Property
        [ForeignKey("IndicatorName")]
        public EconomicIndicator Indicator { get; set; } = null!;
    }
}
