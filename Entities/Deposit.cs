using BankingApplication.Entities.Enums;
using MongoDB.Bson;

namespace BankingApplication.Entities
{
    public class Deposit
    {
        public string? Code { get; set; }
        public decimal Amount { get; set; }
        public DateTime Due { get; set; }
        public DepositStatus Status { get; set; }
        public ObjectId AccountId { get; set; }
    }
}