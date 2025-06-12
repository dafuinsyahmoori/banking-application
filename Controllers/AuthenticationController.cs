using BankingApplication.Entities;
using BankingApplication.Models;
using BankingApplication.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BankingApplication.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController(IMongoCollection<User> userCollection, IMongoCollection<Admin> adminCollection, IMongoCollection<Account> accountCollection, IPasswordHasher<object> passwordHasher, AccountUtility accountUtility, AuthenticationUtility authenticationUtility, IMemoryCache memoryCache) : ControllerBase
    {
        [HttpPost("user/sign-up")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignUpUserAsync(UserForm userForm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newUserId = Guid.NewGuid();
                var newHashedPassword = passwordHasher.HashPassword(null!, userForm.Password!);

                await userCollection.InsertOneAsync(new()
                {
                    Id = newUserId,
                    FirstName = userForm.FirstName,
                    MiddleName = userForm.MiddleName,
                    LastName = userForm.LastName,
                    BirthDate = userForm.BirthDate,
                    Email = userForm.Email,
                    Username = userForm.Username,
                    Password = newHashedPassword
                });

                await accountCollection.InsertOneAsync(new()
                {
                    Number = await accountUtility.GenerateNewAccountNumberAsync(),
                    UserId = newUserId
                });

                await authenticationUtility.SignInAsync(newUserId.ToString(), "User");

                return Created("/api/users/me", null);
            }
            catch (MongoWriteException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("user/sign-in")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignInUserAsync(UserCredential userCredential)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await userCollection.AsQueryable()
                    .Where(u => u.Email == userCredential.EmailOrUsername || u.Username == userCredential.EmailOrUsername)
                    .Select(u => new
                    {
                        u.Id,
                        u.Password
                    })
                    .FirstOrDefaultAsync();

                if (user is null)
                    return BadRequest(new { Message = "user is not registered" });

                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null!, user.Password!, userCredential.Password!);

                if (passwordVerificationResult is PasswordVerificationResult.Failed)
                    return BadRequest(new { Message = "password is not correct" });

                await authenticationUtility.SignInAsync(user.Id.ToString(), "User");

                return Ok();
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("admin/sign-up")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignUpAdminAsync(AdminForm adminForm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newAdminId = Guid.NewGuid();
                var newHashedPassword = passwordHasher.HashPassword(null!, adminForm.Password!);

                await adminCollection.InsertOneAsync(new()
                {
                    FullName = adminForm.FullName,
                    Email = adminForm.Email,
                    Password = newHashedPassword
                });

                await authenticationUtility.SignInAsync(newAdminId.ToString(), "Admin");

                return Created("/api/admins/me", null);
            }
            catch (MongoWriteException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("admin/sign-in")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignInAdminAsync(AdminCredential adminCredential)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var admin = await userCollection.AsQueryable()
                    .Where(a => a.Email == adminCredential.Email)
                    .Select(a => new
                    {
                        a.Id,
                        a.Password
                    })
                    .FirstOrDefaultAsync();

                if (admin is null)
                    return BadRequest(new { Message = "admin is not registered" });

                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null!, admin.Password!, adminCredential.Password!);

                if (passwordVerificationResult is PasswordVerificationResult.Failed)
                    return BadRequest(new { Message = "password is not correct" });

                await authenticationUtility.SignInAsync(admin.Id.ToString(), "Admin");

                return Ok();
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("sign-out")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SignOutAsync()
        {
            var idClaim = HttpContext.User.Claims.First(cl => cl.Type == "ID");
            var roleClaim = HttpContext.User.Claims.First(cl => cl.Type == "Role");

            var id = Guid.Parse(idClaim.Value);
            var role = roleClaim.Value;

            var cacheKey = $"{role.ToLowerInvariant()}-{id}";

            memoryCache.Remove(cacheKey);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return NoContent();
        }
    }
}