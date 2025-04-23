using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwampLocksDb.Models
{
        // Sector Performace object/table
        public class SectorPerformance
        {
            [Key] public string Id => $"{SectorName}_{Date:yyyyMMdd}";

            [Required] public string SectorName { get; set; } // Stock Identifier

            public decimal Performance { get; set; } // Performance Metric

            [Required] public DateTime Date { get; set; }

            [ForeignKey("SectorName")] public virtual Sector Sector { get; set; }

        }
}