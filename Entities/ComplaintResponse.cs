using MongoDB.Bson;

namespace BankingApplication.Entities
{
    public class ComplaintResponse
    {
        public ObjectId Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public ObjectId ComplaintRequestId { get; set; }
        public Guid AdminId { get; set; }
    }
}