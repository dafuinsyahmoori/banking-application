using BankingApplication.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BankingApplication.Controllers
{
    [ApiController]
    [Route("api/deposits")]
    [Authorize]
    public class DepositController(IMongoCollection<Deposit> depositCollection) : ControllerBase
    {
        [HttpGet("{code:withdrawalOrDepositCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDepositByCodeAsync(string code)
        {
            try
            {
                var deposit = await depositCollection.AsQueryable()
                    .Where(d => d.Code == code)
                    .Select(d => new
                    {
                        d.Code,
                        d.Amount,
                        Due = d.Due.ToLocalTime(),
                        d.Status
                    })
                    .FirstAsync();

                return Ok(deposit);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }
    }
}