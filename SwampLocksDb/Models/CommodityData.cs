using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
    public class CommodityData
    {
        [Key, Column(Order = 0)]
        [Required]
        public string CommodityName { get; set; } = string.Empty; // Foreign Key

        [Key, Column(Order = 1)]
        [Required]
        public DateTime Date { get; set; } // Date of the price record

        [Required]
        public decimal Price { get; set; } // Price of the commodity

        // Navigation Property
        [ForeignKey("CommodityName")]
        public CommodityIndicator Commodity { get; set; } = null!;
    }
}