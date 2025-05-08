using BankingApplication.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BankingApplication.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController(IMongoCollection<User> userCollection, IMemoryCache memoryCache) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<IActionResult> GetMeAsync()
        {
            var idClaim = HttpContext.User.Claims.First(cl => cl.Type == "ID");
            var id = Guid.Parse(idClaim.Value);

            try
            {
                var user = await memoryCache.GetOrCreateAsync($"user-{id}", async cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(60);

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
    }
}