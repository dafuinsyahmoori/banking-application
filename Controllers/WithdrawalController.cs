using BankingApplication.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BankingApplication.Controllers
{
    [ApiController]
    [Route("api/withdrawals")]
    [Authorize]
    public class WithdrawalController(IMongoCollection<Withdrawal> withdrawalCollection) : ControllerBase
    {
        [HttpGet("{code:withdrawalOrDepositCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetWithdrawalByCodeAsync(string code)
        {
            try
            {
                var withdrawal = await withdrawalCollection.AsQueryable()
                    .Where(w => w.Code == code)
                    .Select(w => new
                    {
                        w.Code,
                        w.Amount,
                        Due = w.Due.ToLocalTime(),
                        w.Status
                    })
                    .FirstAsync();

                return Ok(withdrawal);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }
    }
}