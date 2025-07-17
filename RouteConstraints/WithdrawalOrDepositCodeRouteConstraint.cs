
using System.Globalization;
using System.Text.RegularExpressions;

namespace BankingApplication.RouteConstraints
{
    public partial class WithdrawalOrDepositCodeRouteConstraint : IRouteConstraint
    {
        [GeneratedRegex(@"^[a-zA-Z0-9]{8}$")]
        private static partial Regex WithdrawalOrDepositCodeRegex();

        private static readonly Regex _regex = WithdrawalOrDepositCodeRegex();

        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out var routeValue))
            {
                var routeValueString = Convert.ToString(routeValue, CultureInfo.InvariantCulture);

                if (routeValueString is null)
                    return false;

                return _regex.IsMatch(routeValueString);
            }

            return false;
        }
    }
}