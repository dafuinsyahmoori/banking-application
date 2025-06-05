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
    public class AccountController(IMongoCollection<Account> accountCollection, AccountUtility accountUtility, IMongoCollection<TransactionHistory> transactionHistoryCollection, IMongoCollection<Withdrawal> withdrawalCollection, [FromKeyedServices("withdrawalCodes")] Dictionary<string, Task> withdrawalCodes) : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetAllAccounts()
        {
            try
            {
                var accounts = accountCollection.AsQueryable()
                    .Select(a => new
                    {
                        a.Number,
                        a.Balance,
                        a.UserId
                    })
                    .ToArray();

                return Ok(accounts);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpGet("by/number/{number:accountNumber}")]
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
                        DateTime = th.DateTime.ToLocalTime(),
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

        [HttpPost("do/get-withdrawal-code")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> GetMoneyWithdrawalCodeAsync(MoneyWithdrawalAndDepositPayload moneyWithdrawalAndDepositPayload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var account = await accountCollection.AsQueryable()
                    .Where(a => a.Number == moneyWithdrawalAndDepositPayload.AccountNumber)
                    .Select(a => new
                    {
                        a.Id,
                        a.Balance
                    })
                    .FirstAsync();

                if (account.Balance < moneyWithdrawalAndDepositPayload.Amount)
                    return BadRequest(new { Message = "balance is insufficient" });

                var newCode = await accountUtility.GenerateNewWithdrawalCodeAsync();

                withdrawalCodes.Add(
                    newCode,
                    Task.Delay(TimeSpan.FromMinutes(60))
                        .ContinueWith(_ =>
                        {
                            var withdrawalFilter = Builders<Withdrawal>.Filter.Eq(w => w.Code, newCode);
                            var withdrawalUpdate = Builders<Withdrawal>.Update.Set(w => w.Status, WithdrawalStatus.Expired);

                            withdrawalCollection.UpdateOne(withdrawalFilter, withdrawalUpdate);
                            withdrawalCodes.Remove(newCode);

                            Console.WriteLine($"Withdrawal code {newCode} has just been expired");
                        })
                );

                await withdrawalCollection.InsertOneAsync(new()
                {
                    Code = await accountUtility.GenerateNewWithdrawalCodeAsync(),
                    Amount = moneyWithdrawalAndDepositPayload.Amount,
                    Due = DateTime.UtcNow.AddMinutes(60),
                    AccountId = account.Id
                });

                return Created("/api/withdrawals", null);
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("do/withdraw")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> WithdrawMoneyAsync(MoneyWithdrawalCodePayload moneyWithdrawalCodePayload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var withdrawal = await withdrawalCollection.AsQueryable()
                    .Where(w => w.Code == moneyWithdrawalCodePayload.Code)
                    .FirstAsync();

                if (withdrawal.Status is WithdrawalStatus.Expired or WithdrawalStatus.Failed)
                    return BadRequest(new { Message = "code is invalid" });

                var account = await accountCollection.AsQueryable()
                    .Where(a => a.Id == withdrawal.AccountId)
                    .Select(a => new
                    {
                        a.Id,
                        a.Balance
                    })
                    .FirstAsync();

                if (account.Balance < withdrawal.Amount)
                    return BadRequest(new { Message = "balance is insufficient" });

                var accountFilter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
                var accountUpdate = Builders<Account>.Update.Inc(a => a.Balance, -withdrawal.Amount);

                await accountCollection.UpdateOneAsync(accountFilter, accountUpdate);

                var withdrawalFilter = Builders<Withdrawal>.Filter.Eq(w => w.Code, withdrawal.Code);
                var withdrawalUpdate = Builders<Withdrawal>.Update.Set(w => w.Status, WithdrawalStatus.Successful);

                await withdrawalCollection.UpdateOneAsync(withdrawalFilter, withdrawalUpdate);

                withdrawalCodes.Remove(withdrawal.Code!);

                await transactionHistoryCollection.InsertOneAsync(new()
                {
                    DateTime = DateTime.UtcNow,
                    Type = TransactionType.Withdrawal,
                    Amount = withdrawal.Amount,
                    AccountId = account.Id
                });

                return Created("/api/transaction-histories", null);
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("do/deposit")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> DepositMoneyAsync(MoneyWithdrawalAndDepositPayload moneyWithdrawalAndDepositPayload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var accountId = await accountCollection.AsQueryable()
                    .Where(a => a.Number == moneyWithdrawalAndDepositPayload.AccountNumber)
                    .Select(a => a.Id)
                    .FirstAsync();

                var filter = Builders<Account>.Filter.Eq(a => a.Number, moneyWithdrawalAndDepositPayload.AccountNumber);
                var update = Builders<Account>.Update.Inc(a => a.Balance, moneyWithdrawalAndDepositPayload.Amount);

                await accountCollection.UpdateOneAsync(filter, update);

                await transactionHistoryCollection.InsertOneAsync(new()
                {
                    DateTime = DateTime.UtcNow,
                    Type = TransactionType.Deposit,
                    Amount = moneyWithdrawalAndDepositPayload.Amount,
                    AccountId = accountId
                });

                return Created("/api/transaction-histories", null);
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }
    }
}