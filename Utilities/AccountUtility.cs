using System.Text;

namespace BankingApplication.Utilities
{
    public class AccountUtility
    {
        private static readonly int[] _digits = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
        private static readonly char[] _alphanumerics = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

        public Task<string> GenerateNewAccountNumberAsync()
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

        public Task<string> GenerateNewWithdrawalOrDepositCodeAsync()
        {
            var withdrawalCode = new StringBuilder(8);

            for (int i = 0; i < withdrawalCode.Capacity; i++)
            {
                var random = new Random();
                var randomIndex = random.Next(_alphanumerics.Length);
                var randomCharacter = _alphanumerics[randomIndex];

                withdrawalCode = withdrawalCode.Append(randomCharacter);
            }

            return Task.FromResult(withdrawalCode.ToString());
        }
    }
}