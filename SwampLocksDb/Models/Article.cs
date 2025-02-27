using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwampLocksDb.Models
{
    public class Article
    {
        [Key]
        public string Id =>
            $"{Ticker}_{ArticleName}_{Date:yyyyMMdd}"; // Unique Identifier (Ticker + ArticleName + Date)

        [Required] public string Ticker { get; set; } // Stock Identifier

        [Required] public string ArticleName { get; set; } // article name

        [Required] public DateTime Date { get; set; } // Article publication date

        [Required] public decimal SentimentScore { get; set; } // article sentiment score

        [ForeignKey("Ticker")] public virtual Stock Stock { get; set; }
    }
}