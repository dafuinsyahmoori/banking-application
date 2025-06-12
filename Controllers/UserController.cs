using BankingApplication.Entities;
using BankingApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BankingApplication.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController(IMongoCollection<User> userCollection, IMemoryCache memoryCache, IMongoCollection<Account> accountCollection, IPasswordHasher<object> passwordHasher) : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetAllUsers()
        {
            try
            {
                var users = userCollection.AsQueryable()
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.MiddleName,
                        u.LastName,
                        u.BirthDate,
                        u.Email,
                        u.Username
                    })
                    .ToArray();

                return Ok(users);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpGet("me")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMeAsync()
        {
            var idClaim = HttpContext.User.Claims.First(cl => cl.Type == "ID");
            var id = Guid.Parse(idClaim.Value);

            try
            {
                var user = await memoryCache.GetOrCreateAsync($"user-{id}", async cacheEntry =>
                {
                    cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                    cacheEntry.SetSlidingExpiration(TimeSpan.FromSeconds(60));

                    return await userCollection.AsQueryable()
                        .Where(u => u.Id == id)
                        .Select(u => new
                        {
                            u.Id,
                            u.FirstName,
                            u.MiddleName,
                            u.LastName,
                            u.BirthDate,
                            u.Email,
                            u.Username
                        })
                        .FirstAsync();
                });

                return Ok(user);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpGet("me/accounts")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetMyAccounts()
        {
            var idClaim = HttpContext.User.Claims.First(cl => cl.Type == "ID");
            var id = Guid.Parse(idClaim.Value);

            try
            {
                var accounts = accountCollection.AsQueryable()
                    .Where(a => a.UserId == id)
                    .Select(a => new
                    {
                        a.Number,
                        a.Balance
                    })
                    .ToArray();

                return Ok(accounts);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPut("do/change-password")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePasswordAsync(PasswordChangeRequest passwordChangeRequest)
        {
            var idClaim = HttpContext.User.Claims.First(cl => cl.Type == "ID");
            var id = Guid.Parse(idClaim.Value);

            try
            {
                var currentPassword = await userCollection.AsQueryable()
                    .Where(u => u.Id == id)
                    .Select(u => u.Password)
                    .FirstAsync();

                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null!, currentPassword!, passwordChangeRequest.CurrentPassword!);

                if (passwordVerificationResult is PasswordVerificationResult.Failed)
                    return BadRequest(new { Message = "current password is incorrect" });

                var newHashedPassword = passwordHasher.HashPassword(null!, passwordChangeRequest.NewPassword!);

                var filter = Builders<User>.Filter.Eq(u => u.Id, id);
                var update = Builders<User>.Update.Set(u => u.Password, newHashedPassword);

                await userCollection.UpdateOneAsync(filter, update);
                memoryCache.Remove($"user-{id}");

                return NoContent();
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }
    }
}