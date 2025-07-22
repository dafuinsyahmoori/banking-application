using BankingApplication.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BankingApplication.Controllers
{
    [ApiController]
    [Route("api/complaints")]
    [Authorize]
    public class ComplaintController(IMongoCollection<ComplaintRequest> complaintRequestCollection, IMongoCollection<ComplaintResponse> complaintResponseCollection, IMongoCollection<User> userCollection, IMongoCollection<Admin> adminCollection) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetAllComplaintRequests()
        {
            try
            {
                var complaintRequests = complaintRequestCollection.AsQueryable()
                    .Join(
                        userCollection.AsQueryable(),
                        cr => cr.UserId,
                        u => u.Id,
                        (complaintRequest, user) => new
                        {
                            complaintRequest.Id,
                            complaintRequest.Title,
                            complaintRequest.Content,
                            User = new
                            {
                                user.Id,
                                user.FirstName,
                                user.MiddleName,
                                user.LastName,
                                user.Username
                            }
                        }
                    )
                    .ToArray();

                return Ok(complaintRequests);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }
    }
}