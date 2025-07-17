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
    public class AccountController(IMongoCollection<Account> accountCollection, AccountUtility accountUtility, IMongoCollection<TransactionHistory> transactionHistoryCollection, IMongoCollection<Withdrawal> withdrawalCollection, IMongoCollection<Deposit> depositCollection, [FromKeyedServices("withdrawalCodes")] Dictionary<string, Task> withdrawalCodes, [FromKeyedServices("depositCodes")] Dictionary<string, Task> depositCodes) : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactionHistoriesByAccountNumberAsync(string number)
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

        [HttpGet("by/number/{number:accountNumber}/withdrawals")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetWithdrawalsByAccountNumberAsync(string number)
        {
            try
            {
                var accountId = await accountCollection.AsQueryable()
                    .Where(a => a.Number == number)
                    .Select(a => a.Id)
                    .FirstAsync();

                var withdrawals = withdrawalCollection.AsQueryable()
                    .Where(w => w.AccountId == accountId)
                    .Select(w => new
                    {
                        w.Code,
                        w.Amount,
                        Due = w.Due.ToLocalTime(),
                        w.Status
                    })
                    .ToArray();

                return Ok(withdrawals);
            }
            catch (MongoQueryException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpGet("by/number/{number:accountNumber}/deposits")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDepositCodesByAccountNumberAsync(string number)
        {
            try
            {
                var accountId = await accountCollection.AsQueryable()
                    .Where(a => a.Number == number)
                    .Select(a => a.Id)
                    .FirstAsync();

                var deposits = depositCollection.AsQueryable()
                    .Where(d => d.AccountId == accountId)
                    .Select(d => new
                    {
                        d.Code,
                        d.Amount,
                        Due = d.Due.ToLocalTime(),
                        d.Status
                    })
                    .ToArray();

                return Ok(deposits);
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TransferMoneyAsync(MoneyTransferPayload payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var senderAccount = await accountCollection.AsQueryable()
                    .Where(a => a.Number == payload.SenderAccountNumber)
                    .Select(a => new
                    {
                        a.Id,
                        a.Number,
                        a.Balance
                    })
                    .FirstAsync();

                if (senderAccount.Balance < payload.Amount)
                    return BadRequest(new { Message = "balance is insufficient" });

                var receiverAccountId = await accountCollection.AsQueryable()
                    .Where(a => a.Number == payload.ReceiverAccountNumber)
                    .Select(a => a.Id)
                    .FirstAsync();

                var senderFilter = Builders<Account>.Filter.Eq(a => a.Number, payload.SenderAccountNumber);
                var receiverFilter = Builders<Account>.Filter.Eq(a => a.Number, payload.ReceiverAccountNumber);

                var senderUpdate = Builders<Account>.Update.Inc(a => a.Balance, -payload.Amount);
                var receiverUpdate = Builders<Account>.Update.Inc(a => a.Balance, payload.Amount);

                await accountCollection.UpdateOneAsync(senderFilter, senderUpdate);
                await accountCollection.UpdateOneAsync(receiverFilter, receiverUpdate);

                await transactionHistoryCollection.InsertManyAsync([
                    new()
                    {
                        DateTime = DateTime.UtcNow,
                        Type = TransactionType.Transfer,
                        Amount = -payload.Amount,
                        ReceiverAccountNumber = payload.ReceiverAccountNumber,
                        AccountId = senderAccount.Id
                    },
                    new()
                    {
                        DateTime = DateTime.UtcNow,
                        Type = TransactionType.Transfer,
                        Amount = payload.Amount,
                        SenderAccountNumber = payload.SenderAccountNumber,
                        AccountId = receiverAccountId
                    }
                ]);

                return Created($"/api/accounts/{senderAccount.Number}/transaction-histories", null);
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("do/get-withdrawal-code")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMoneyWithdrawalCodeAsync(MoneyWithdrawalOrDepositPayload payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var account = await accountCollection.AsQueryable()
                    .Where(a => a.Number == payload.AccountNumber)
                    .Select(a => new
                    {
                        a.Id,
                        a.Number,
                        a.Balance
                    })
                    .FirstAsync();

                if (account.Balance < payload.Amount)
                    return BadRequest(new { Message = "balance is insufficient" });

                var newCode = await accountUtility.GenerateNewWithdrawalOrDepositCodeAsync();

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

                var newWithdrawal = new Withdrawal
                {
                    Code = newCode,
                    Amount = payload.Amount,
                    Due = DateTime.UtcNow.AddMinutes(60),
                    Status = WithdrawalStatus.Pending,
                    AccountId = account.Id
                };

                await withdrawalCollection.InsertOneAsync(newWithdrawal);

                return Created($"/api/accounts/{account.Number}/withdrawals/{newWithdrawal.Code}", new
                {
                    newWithdrawal.Code,
                    newWithdrawal.Amount,
                    Due = newWithdrawal.Due.ToLocalTime(),
                    newWithdrawal.Status
                });
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("do/withdraw")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> WithdrawMoneyAsync(MoneyWithdrawalOrDepositCodePayload payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var withdrawal = await withdrawalCollection.AsQueryable()
                    .Where(w => w.Code == payload.Code)
                    .FirstAsync();

                if (withdrawal.Status is WithdrawalStatus.Expired or WithdrawalStatus.Failed or WithdrawalStatus.Succeeded)
                    return BadRequest(new { Message = "code is invalid" });

                var account = await accountCollection.AsQueryable()
                    .Where(a => a.Id == withdrawal.AccountId)
                    .Select(a => new
                    {
                        a.Id,
                        a.Number,
                        a.Balance
                    })
                    .FirstAsync();

                if (account.Balance < withdrawal.Amount)
                    return BadRequest(new { Message = "balance is insufficient" });

                var accountFilter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
                var accountUpdate = Builders<Account>.Update.Inc(a => a.Balance, -withdrawal.Amount);

                await accountCollection.UpdateOneAsync(accountFilter, accountUpdate);

                var withdrawalFilter = Builders<Withdrawal>.Filter.Eq(w => w.Code, withdrawal.Code);
                var withdrawalUpdate = Builders<Withdrawal>.Update.Set(w => w.Status, WithdrawalStatus.Succeeded);

                await withdrawalCollection.UpdateOneAsync(withdrawalFilter, withdrawalUpdate);

                withdrawalCodes.Remove(withdrawal.Code!);

                await transactionHistoryCollection.InsertOneAsync(new()
                {
                    DateTime = DateTime.UtcNow,
                    Type = TransactionType.Withdrawal,
                    Amount = withdrawal.Amount,
                    AccountId = account.Id
                });

                return Created($"/api/accounts/{account.Number}/transaction-histories", null);
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("do/get-deposit-code")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMoneyDepositCodeAsync(MoneyWithdrawalOrDepositPayload payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (payload.Amount < 100000)
                    return BadRequest(new { Message = "minimum amount is 10.00" });

                var account = await accountCollection.AsQueryable()
                    .Where(a => a.Number == payload.AccountNumber)
                    .Select(a => new
                    {
                        a.Id,
                        a.Number,
                        a.Balance
                    })
                    .FirstAsync();

                var newCode = await accountUtility.GenerateNewWithdrawalOrDepositCodeAsync();

                depositCodes.Add(
                    newCode,
                    Task.Delay(TimeSpan.FromMinutes(60))
                        .ContinueWith(_ =>
                        {
                            var depositFilter = Builders<Deposit>.Filter.Eq(d => d.Code, newCode);
                            var depositUpdate = Builders<Deposit>.Update.Set(d => d.Status, DepositStatus.Expired);

                            depositCollection.UpdateOne(depositFilter, depositUpdate);
                            depositCodes.Remove(newCode);

                            Console.WriteLine($"Deposit code {newCode} has just been expired");
                        })
                );

                var newDeposit = new Deposit
                {
                    Code = newCode,
                    Amount = payload.Amount,
                    Due = DateTime.UtcNow.AddMinutes(60),
                    Status = DepositStatus.Pending,
                    AccountId = account.Id
                };

                await depositCollection.InsertOneAsync(newDeposit);

                return Created($"/api/accounts/{account.Number}/deposits/{newDeposit.Code}", new
                {
                    newDeposit.Code,
                    newDeposit.Amount,
                    Due = newDeposit.Due.ToLocalTime(),
                    newDeposit.Status
                });
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }

        [HttpPost("do/deposit")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DepositMoneyAsync(MoneyWithdrawalOrDepositCodePayload payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var deposit = await depositCollection.AsQueryable()
                    .Where(d => d.Code == payload.Code)
                    .FirstAsync();

                if (deposit.Status is DepositStatus.Failed or DepositStatus.Expired)
                    return BadRequest(new { Message = "code is invalid" });

                if (deposit.Amount < 100000)
                    return BadRequest(new { Message = "minimum amount is 10.00" });

                var account = await accountCollection.AsQueryable()
                    .Where(a => a.Id == deposit.AccountId)
                    .Select(a => new
                    {
                        a.Id,
                        a.Number,
                        a.Balance
                    })
                    .FirstAsync();

                var accountFilter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
                var accountUpdate = Builders<Account>.Update.Inc(a => a.Balance, deposit.Amount);

                await accountCollection.UpdateOneAsync(accountFilter, accountUpdate);

                var depositFilter = Builders<Deposit>.Filter.Eq(d => d.Code, deposit.Code);
                var depositUpdate = Builders<Deposit>.Update.Set(d => d.Status, DepositStatus.Succeeded);

                await depositCollection.UpdateOneAsync(depositFilter, depositUpdate);

                depositCodes.Remove(deposit.Code!);

                await transactionHistoryCollection.InsertOneAsync(new()
                {
                    DateTime = DateTime.UtcNow,
                    Type = TransactionType.Deposit,
                    Amount = deposit.Amount,
                    AccountId = account.Id
                });

                return Created($"/api/accounts/{account.Number}/transaction-histories", null);
            }
            catch (MongoException exception)
            {
                return BadRequest(new { exception.Message, exception.Source });
            }
        }
    }
}