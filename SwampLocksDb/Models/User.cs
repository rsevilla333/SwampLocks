using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
    // User object/table
    public class User
    {
        [Key]
        public Guid UserId { get; set; }  

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } 

        [StringLength(255)]
        public string FullName { get; set; } 

        public string? ProfilePicture { get; set; }  
        public DateTime DateCreated { get; set; } 
        
        public virtual ICollection<Holding> Holdings { get; set; } = new List<Holding>();


    }
}