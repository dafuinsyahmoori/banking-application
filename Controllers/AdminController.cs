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
    [Route("api/admins")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController(IMongoCollection<Admin> adminCollection, IMemoryCache memoryCache, IPasswordHasher<object> passwordHasher) : ControllerBase
    {
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMeAsync()
        {
            var idClaim = HttpContext.User.Claims.First(cl => cl.Type == "ID");
            var id = Guid.Parse(idClaim.Value);

            try
            {
                var admin = await memoryCache.GetOrCreateAsync($"admin-{id}", async cacheEntry =>
                {
                    cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                    cacheEntry.SetSlidingExpiration(TimeSpan.FromSeconds(60));

                    return await adminCollection.AsQueryable()
                        .Where(a => a.Id == id)
                        .Select(a => new
                        {
                            a.Id,
                            a.FullName,
                            a.Email
                        })
                        .FirstAsync();
                });

                return Ok(admin);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPut("do/change-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePasswordAsync(PasswordChangeRequest passwordChangeRequest)
        {
            var idClaim = HttpContext.User.Claims.First(cl => cl.Type == "ID");
            var id = Guid.Parse(idClaim.Value);

            try
            {
                var currentPassword = await adminCollection.AsQueryable()
                    .Where(a => a.Id == id)
                    .Select(a => a.Password)
                    .FirstAsync();

                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null!, currentPassword!, passwordChangeRequest.CurrentPassword!);

                if (passwordVerificationResult is PasswordVerificationResult.Failed)
                    return BadRequest(new { Message = "current password is incorrect" });

                var newHashedPassword = passwordHasher.HashPassword(null!, passwordChangeRequest.NewPassword!);

                var filter = Builders<Admin>.Filter.Eq(a => a.Id, id);
                var update = Builders<Admin>.Update.Set(a => a.Password, newHashedPassword);

                await adminCollection.UpdateOneAsync(filter, update);
                memoryCache.Remove($"admin-{id}");

                return NoContent();
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }
    }
}