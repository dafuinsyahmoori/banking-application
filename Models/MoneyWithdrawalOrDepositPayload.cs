using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models
{
    public class MoneyWithdrawalOrDepositPayload
    {
        [RegularExpression(@"^\d{15}$")]
        [Required]
        public string? AccountNumber { get; set; }
        [Range(100000, double.PositiveInfinity)]
        [Required]
        public int Amount { get; set; }
    }
}