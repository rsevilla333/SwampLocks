using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
    // Helper class to insert holdings
    public class CreateHoldingDto
    {
        public Guid UserId { get; set; }
        public string Ticker { get; set; }
        public decimal Shares { get; set; }
    }
}