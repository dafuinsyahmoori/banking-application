using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class MoneyTransferPayload
    {
        [RegularExpression(@"^\d{15}$")]
        [Required]
        public string? SenderAccountNumber { get; set; }
        [RegularExpression(@"^\d{15}$")]
        [Required]
        public string? ReceiverAccountNumber { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}