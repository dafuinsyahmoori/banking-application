using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class UserCredential
    {
        [Required]
        public string? EmailOrUsername { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}