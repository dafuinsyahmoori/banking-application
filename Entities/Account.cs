using MongoDB.Bson;

namespace BankingApplication.Entities
{
    public class Account
    {
        public ObjectId Id { get; set; }
        public string? Number { get; set; }
        public decimal Balance { get; set; }
        public Guid UserId { get; set; }
    }
}