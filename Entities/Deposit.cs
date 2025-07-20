using BankingApplication.Entities.Enums;
using MongoDB.Bson;

namespace BankingApplication.Entities
{
    public class Deposit
    {
        public ObjectId Id { get; set; }
        public string? Code { get; set; }
        public int Amount { get; set; }
        public DateTime Due { get; set; }
        public DepositStatus Status { get; set; }
        public ObjectId AccountId { get; set; }
    }
}