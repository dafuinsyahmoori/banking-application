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
        [Range(10000, double.PositiveInfinity)]
        [Required]
        public int Amount { get; set; }
    }
}