using MongoDB.Bson;

namespace BankingApplication.Entities
{
    public class ComplaintRequest
    {
        public ObjectId Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public Guid UserId { get; set; }
    }
}