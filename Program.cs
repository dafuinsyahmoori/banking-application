using BankingApplication.BsonSerializers;
using BankingApplication.DependencyRegistrations;
using BankingApplication.Entities;
using BankingApplication.JsonConverters;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

ConventionRegistry.Register("camelCase", new ConventionPack { new CamelCaseElementNameConvention() }, _ => true);

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
BsonSerializer.RegisterSerializer(new DateOnlyBsonSerializer());

BsonClassMap.RegisterClassMap<User>(classMap =>
{
    classMap.AutoMap();

    classMap.MapMember(u => u.MiddleName).SetIgnoreIfDefault(true);
    classMap.MapMember(u => u.LastName).SetIgnoreIfDefault(true);
});

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseClient(builder.Configuration);
builder.Services.AddDatabaseInstance();
builder.Services.AddDatabaseCollections();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy("UserOnly", policy =>
    {
        policy.RequireClaim("Role", "User");
        policy.RequireClaim("ID");
    });

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter()));

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();