using System;
using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        public User Creator { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get;set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}