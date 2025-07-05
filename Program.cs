using BankingApplication.AuthorizationMiddlewareResultHandlers;
using BankingApplication.BsonSerializers;
using BankingApplication.DependencyRegistrations;
using BankingApplication.Entities;
using BankingApplication.JsonConverters;
using BankingApplication.RouteConstraints;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

ConventionRegistry.Register("camelCase", new ConventionPack { new CamelCaseElementNameConvention(), new EnumRepresentationConvention(BsonType.String) }, _ => true);

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
BsonSerializer.RegisterSerializer(new DateOnlyBsonSerializer());
BsonSerializer.RegisterSerializer(new DateTimeBsonSerializer());

BsonClassMap.RegisterClassMap<User>(classMap =>
{
    classMap.AutoMap();

    classMap.MapMember(u => u.MiddleName).SetIgnoreIfNull(true);
    classMap.MapMember(u => u.LastName).SetIgnoreIfNull(true);
});

BsonClassMap.RegisterClassMap<TransactionHistory>(classMap =>
{
    classMap.AutoMap();

    classMap.MapMember(u => u.ReceiverAccountNumber).SetIgnoreIfNull(true);
    classMap.MapMember(u => u.SenderAccountNumber).SetIgnoreIfNull(true);
});

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseClient(builder.Configuration);
builder.Services.AddDatabaseInstance();
builder.Services.AddDatabaseCollections();

builder.Services.AddPendingWithdrawalCodes();
builder.Services.AddPendingDepositCodes();

builder.Services.AddMemoryCache();

builder.Services.AddRouting(options => options.ConstraintMap.Add("accountNumber", typeof(AccountNumberRouteConstraint)));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AUTH_COOKIE";
        options.Cookie.SameSite = builder.Environment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Lax;
    });

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