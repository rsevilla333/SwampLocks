using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwampLocksDb.Models
{
    public class SectorSentiment
    {
        [Key]
        public int Id { get; set; }
        
        [Required] public string SectorName { get; set; } // Sector Identifier

        public decimal Sentiment { get; set; } // Performance Metric
        
        public string Label { get; set; }

        [Required] public DateTime Date { get; set; }

        [ForeignKey("SectorName")] public virtual Sector Sector { get; set; }

    }
}