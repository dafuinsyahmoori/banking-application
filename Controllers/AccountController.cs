using BankingApplication.Entities;
using BankingApplication.Entities.Enums;
using BankingApplication.Models;
using BankingApplication.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BankingApplication.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize]
    public class AccountController(IMongoCollection<Account> accountCollection, AccountUtility accountUtility, IMongoCollection<TransactionHistory> transactionHistoryCollection) : ControllerBase
    {
        [HttpGet("by/number/{number:accountNumber}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> GetAccountByNumberAsync(string number)
        {
            try
            {
                var account = await accountCollection.AsQueryable()
                    .Where(a => a.Number == number)
                    .Select(a => new
                    {
                        a.Number,
                        a.Balance
                    })
                    .FirstAsync();

                return Ok(account);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpGet("by/number/{number:accountNumber}/transaction-histories")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> GetTransactionHistoriesByAccountNumber(string number)
        {
            try
            {
                var accountId = await accountCollection.AsQueryable()
                    .Where(a => a.Number == number)
                    .Select(a => a.Id)
                    .FirstAsync();

                var transactionHistories = transactionHistoryCollection.AsQueryable()
                    .Where(th => th.AccountId == accountId)
                    .OrderByDescending(th => th.DateTime)
                    .Select(th => new
                    {
                        th.DateTime,
                        th.Type,
                        th.Amount,
                        th.ReceiverAccountNumber,
                        th.SenderAccountNumber
                    })
                    .ToArray();

                return Ok(transactionHistories);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("do/create")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> CreateNewAccountAsync()
        {
            var userIdClaim = HttpContext.User.Claims.First(cl => cl.Type == "ID");
            var userId = Guid.Parse(userIdClaim.Value);

            try
            {
                await accountCollection.InsertOneAsync(new()
                {
                    Number = await accountUtility.GenerateNewAccountNumberAsync(),
                    UserId = userId
                });

                return Created("/api/accounts", null);
            }
            catch (MongoWriteException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("do/transfer")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> TransferMoneyAsync(MoneyTransferPayload moneyTransferPayload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var senderAccount = await accountCollection.AsQueryable()
                    .Where(a => a.Number == moneyTransferPayload.SenderAccountNumber)
                    .Select(a => new
                    {
                        a.Id,
                        a.Balance
                    })
                    .FirstAsync();

                if (senderAccount.Balance < moneyTransferPayload.Amount)
                    return BadRequest(new { Message = "balance is insufficient" });

                var receiverAccountId = await accountCollection.AsQueryable()
                    .Where(a => a.Number == moneyTransferPayload.ReceiverAccountNumber)
                    .Select(a => a.Id)
                    .FirstAsync();

                var senderFilter = Builders<Account>.Filter.Eq(a => a.Number, moneyTransferPayload.SenderAccountNumber);
                var receiverFilter = Builders<Account>.Filter.Eq(a => a.Number, moneyTransferPayload.ReceiverAccountNumber);

                var senderUpdate = Builders<Account>.Update.Inc(a => a.Balance, -moneyTransferPayload.Amount);
                var receiverUpdate = Builders<Account>.Update.Inc(a => a.Balance, moneyTransferPayload.Amount);

                await accountCollection.UpdateOneAsync(senderFilter, senderUpdate);
                await accountCollection.UpdateOneAsync(receiverFilter, receiverUpdate);

                await transactionHistoryCollection.InsertManyAsync([
                    new()
                    {
                        DateTime = DateTime.UtcNow,
                        Type = TransactionType.Transfer,
                        Amount = -moneyTransferPayload.Amount,
                        ReceiverAccountNumber = moneyTransferPayload.ReceiverAccountNumber,
                        AccountId = senderAccount.Id
                    },
                    new()
                    {
                        DateTime = DateTime.UtcNow,
                        Type = TransactionType.Transfer,
                        Amount = moneyTransferPayload.Amount,
                        SenderAccountNumber = moneyTransferPayload.SenderAccountNumber,
                        AccountId = receiverAccountId
                    }
                ]);

                return Created("/api/transaction-histories", null);
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }
    }
}