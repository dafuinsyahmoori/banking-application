using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class ComplaintResponseForm
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Content { get; set; }
    }
}