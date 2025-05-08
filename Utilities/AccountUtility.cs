using System.Text;

namespace BankingApplication.Utilities
{
    public class AccountUtility
    {
        private readonly int[] _digits = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        public Task<string> GenerateAccountNumberAsync()
        {
            var accountNumber = new StringBuilder(15);

            for (int i = 0; i < accountNumber.Capacity; i++)
            {
                var random = new Random();
                var randomIndex = random.Next(_digits.Length);
                var randomDigit = _digits[randomIndex];

                accountNumber = accountNumber.Append(randomDigit);
            }

            return Task.FromResult(accountNumber.ToString());
        }
    }
}