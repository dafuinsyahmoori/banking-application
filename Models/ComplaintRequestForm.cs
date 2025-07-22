using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class ComplaintRequestForm
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Content { get; set; }
    }
}