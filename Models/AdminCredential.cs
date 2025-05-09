using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class AdminCredential
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}