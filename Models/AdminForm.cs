using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class AdminForm
    {
        [StringLength(47)]
        [Required]
        public string? FullName { get; set; }
        [EmailAddress]
        [StringLength(70)]
        [Required]
        public string? Email { get; set; }
        [StringLength(20, MinimumLength = 8)]
        [Required]
        public string? Password { get; set; }
    }
}