using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class UserForm
    {
        [StringLength(15)]
        [Required]
        public string? FirstName { get; set; }
        [StringLength(15)]
        public string? MiddleName { get; set; }
        [StringLength(15)]
        public string? LastName { get; set; }
        [Required]
        public DateOnly BirthDate { get; set; }
        [EmailAddress]
        [StringLength(70)]
        [Required]
        public string? Email { get; set; }
        [StringLength(60)]
        [Required]
        public string? Username { get; set; }
        [StringLength(20, MinimumLength = 8)]
        [Required]
        public string? Password { get; set; }
    }
}