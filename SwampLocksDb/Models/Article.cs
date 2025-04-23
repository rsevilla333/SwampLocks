using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwampLocksDb.Models
{
    // Stock's article object/table
    public class Article
    {
        [Key]
        public string Id =>
            $"{Ticker}_{ArticleName}_{Date:yyyyMMdd}"; // Unique Identifier (Ticker + ArticleName + Date)

        [Required] public string Ticker { get; set; } // Stock Identifier

        [Required] public string ArticleName { get; set; } // article name

        [Required] public DateTime Date { get; set; } // Article publication date

        [Required] public decimal SentimentScore { get; set; } // article sentiment score
        
        public decimal RelevanceScore { get; set; } // article relevance score (0-1)

        public string URL { get; set; } // article url

        [ForeignKey("Ticker")] public virtual Stock Stock { get; set; }
    }
}