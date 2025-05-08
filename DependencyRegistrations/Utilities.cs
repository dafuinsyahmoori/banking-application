using BankingApplication.Utilities;

namespace BankingApplication.DependencyRegistrations
{
    public static class Utilities
    {
        public static IServiceCollection AddUtilities(this IServiceCollection services)
        {
            return services
                .AddTransient<AuthenticationUtility>()
                .AddTransient<AccountUtility>();
        }
    }
}