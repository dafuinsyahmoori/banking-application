using BankingApplication.AuthorizationMiddlewareResultHandlers;
using BankingApplication.Configurations;
using BankingApplication.DependencyRegistrations;
using BankingApplication.JsonConverters;
using BankingApplication.RouteConstraints;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.ConfigureConventionRegistries();
builder.Configuration.ConfigureBsonSerializers();
builder.Configuration.ConfigureBsonClassMaps();

builder.Services.AddDatabaseClient(builder.Configuration);
builder.Services.AddDatabaseInstance();
builder.Services.AddDatabaseCollections();

builder.Services.AddPendingWithdrawalCodes();
builder.Services.AddPendingDepositCodes();

builder.Services.AddMemoryCache();

builder.Services.AddRouting(options =>
{
    options.ConstraintMap.Add("accountNumber", typeof(AccountNumberRouteConstraint));
    options.ConstraintMap.Add("withdrawalOrDepositCode", typeof(WithdrawalOrDepositCodeRouteConstraint));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("UserOnly", policy =>
    {
        policy.RequireClaim("Role", "User");
        policy.RequireClaim("ID");
    })
    .AddPolicy("AdminOnly", policy =>
    {
        policy.RequireClaim("Role", "Admin");
        policy.RequireClaim("ID");
    });

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new TransactionTypeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new WithdrawalStatusJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new DepositStatusJsonConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, UnauthorizedAuthorizationMiddlewareResultHandler>();

builder.Services.AddTransient<IPasswordHasher<object>, PasswordHasher<object>>();

builder.Services.AddUtilities();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();