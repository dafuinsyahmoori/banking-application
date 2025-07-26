using BankingApplication.Entities;
using BankingApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BankingApplication.Controllers
{
    [ApiController]
    [Route("api/complaints")]
    [Authorize]
    public class ComplaintController(IMongoCollection<ComplaintRequest> complaintRequestCollection, IMongoCollection<ComplaintResponse> complaintResponseCollection, IMongoCollection<User> userCollection) : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
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

        [HttpPost("do/create")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateComplaintAsync(ComplaintRequestForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = HttpContext.User.Claims.First(cl => cl.Type == "ID");
            var userId = Guid.Parse(userIdClaim.Value);

            try
            {
                await complaintRequestCollection.InsertOneAsync(new()
                {
                    Title = form.Title,
                    Content = form.Content,
                    UserId = userId
                });

                return Created("/api/users/me/complaints", null);
            }
            catch (MongoWriteException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("{id}/do/respond")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RespondToComplaintAsync(ObjectId id, ComplaintResponseForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var adminIdClaim = HttpContext.User.Claims.First(cl => cl.Type == "ID");
            var adminId = Guid.Parse(adminIdClaim.Value);

            try
            {
                var doesComplaintExist = await complaintRequestCollection.AsQueryable()
                    .AnyAsync(cr => cr.Id == id);

                if (!doesComplaintExist)
                    return BadRequest(new { Message = "complaint is not found" });

                await complaintResponseCollection.InsertOneAsync(new()
                {
                    Title = form.Title,
                    Content = form.Content,
                    ComplaintRequestId = id,
                    AdminId = adminId
                });

                return Created("/api/complaints", null);
            }
            catch (MongoWriteException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }
    }
}