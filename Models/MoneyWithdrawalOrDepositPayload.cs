using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class MoneyWithdrawalOrDepositPayload
    {
        [RegularExpression(@"^\d{15}$")]
        [Required]
        public string? AccountNumber { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}