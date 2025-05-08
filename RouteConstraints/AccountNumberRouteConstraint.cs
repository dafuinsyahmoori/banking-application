using System.Globalization;
using System.Text.RegularExpressions;

namespace BankingApplication.RouteConstraints
{
    public partial class AccountNumberRouteConstraint : IRouteConstraint
    {
        [GeneratedRegex(@"^\d{15}$", RegexOptions.CultureInvariant)]
        private static partial Regex AccountNumberRegex();

        private static readonly Regex _regex = AccountNumberRegex();

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