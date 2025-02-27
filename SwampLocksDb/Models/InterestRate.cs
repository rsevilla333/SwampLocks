using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
    public class InterestRate
    {
        [Key]
        public string Id { get; set; }  // Unique Identifier (RateType + Date)

        [Required]
        public DateTime Date { get; set; } // Date of the interest rate

        [Required]
        public string RateType { get; set; } // Type of rate (e.g., "FEDFUNDS", "LIBOR", etc.)

        [Required]
        [Column(TypeName = "decimal(10,4)")]
        public decimal Rate { get; set; } // Interest rate value

        [Required]
        public string Currency { get; set; } // Currency associated with the rate
        
        // Default Constructor
        public InterestRate() { }

        // Parameterized Constructor
        public InterestRate(DateTime date, string rateType, string currency, decimal rate)
        {
            Date = date;
            Rate = rate;
            Currency = currency;
            RateType = rateType;
            Id = $"{RateType}_{Date:yyyyMMdd}"; // Set Id explicitly
        }
    }
}