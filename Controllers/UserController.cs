using BankingApplication.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BankingApplication.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController(IMongoCollection<User> userCollection) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll()
        {
            var users = userCollection.AsQueryable().ToArray();
            return Ok(users);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddAsync(User user)
        {
            await userCollection.InsertOneAsync(user);
            return Created("/users", null);
        }
    }
}