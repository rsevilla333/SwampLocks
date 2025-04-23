using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SwampLocksDb.Models
{
    // Holding object/table
    public class Holding
    {
        [Key]
        public int HoldingId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(10)]
        public string Ticker { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Shares { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        [BindNever]
        public virtual User User { get; set; }

        [ForeignKey("Ticker")]
        [JsonIgnore]
        [BindNever]
        public virtual Stock Stock { get; set; }
    }
}