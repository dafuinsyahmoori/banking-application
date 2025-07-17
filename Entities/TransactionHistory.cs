using BankingApplication.Entities.Enums;
using MongoDB.Bson;

namespace BankingApplication.Entities
{
    public class TransactionHistory
    {
        public ObjectId Id { get; set; }
        public DateTime DateTime { get; set; }
        public TransactionType Type { get; set; }
        public int Amount { get; set; }
        public string? ReceiverAccountNumber { get; set; }
        public string? SenderAccountNumber { get; set; }
        public ObjectId AccountId { get; set; }
    }
}