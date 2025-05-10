using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class PasswordChangeRequest
    {
        [Required]
        public string? CurrentPassword { get; set; }
        [StringLength(20, MinimumLength = 8)]
        [Required]
        public string? NewPassword { get; set; }
        [Compare(nameof(NewPassword))]
        [Required]
        public string? PasswordConfirmation { get; set; }
    }
}