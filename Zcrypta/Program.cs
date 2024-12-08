using Zcrypta.Context;
using Zcrypta.Hubs;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Binance.Net;
using Binance.Net.Interfaces.Clients;
using Zcrypta.BackgroundServices;
using Zcrypta.Managers;
using Zcrypta.Entities.BackgroundServices;
//using Zcrypta.Migrations;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Zcrypta.Repositories;
//using Zcrypta.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
// Add services to the container.
builder.Services.AddProblemDetails();


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        In = ParameterLocation.Header,
//        Description = "Please insert JWT with Bearer into field",
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey
//    });
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//            Reference = new OpenApiReference
//            {
//                Type = ReferenceType.SecurityScheme,
//                Id = "Bearer"
//            }
//            },
//            Array.Empty<string>()
//        }
//    });
//});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
}));

builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

builder.Services.AddBinance();

builder.Services.AddSingleton<ActiveTickerManager>();
builder.Services.AddSingleton<SignalTickerManager>();
builder.Services.AddHostedService<StocksFeedUpdater>();

builder.Services.AddHostedService<MaCrossoverSignaller>();
builder.Services.AddHostedService<RsiSignaller>();
builder.Services.AddHostedService<MacdSignaller>();
builder.Services.AddHostedService<BollingerBandsSignaller>();
builder.Services.AddHostedService<StochasticOscillatorSignaller>();
builder.Services.AddHostedService<TripleMaCrossoverSignaller>();
builder.Services.AddHostedService<PriceChannelSignaller>();
builder.Services.AddHostedService<VolumePriceTrendSignaller>();
builder.Services.AddHostedService<MomentumSignaller>();
builder.Services.AddHostedService<ExponentialMaCrossoverWithVolumeSignaller>();

builder.Services.Configure<UpdateOptions>(builder.Configuration.GetSection("PriceUpdateOptions"));

//var secret = builder.Configuration.GetValue<string>("Jwt:Secret");
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = "Zcrypta",
//        ValidAudience = "Zcrypta",
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
//    };
//});

//builder.Services.AddScoped<IAuthRepository, AuthRepository>();
//builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthorization();
// add identity and opt-in to endpoints
builder.Services.AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();


var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //db.Database.Migrate();
        await SeedData.InitializeAsync(scope.ServiceProvider);
    }
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
//app.MapHub<ChatHub>("/chathub");
app.MapHub<StocksFeedHub>("/pricehub");
app.MapHub<TradingSignalSenderHub>("/trading-signal-sender-hub");

app.MapIdentityApi<User>();
//app.UseAntiforgery();

app.MapPost("/logout", async (SignInManager<User> signInManager, [FromBody] object empty) =>
{
    if (empty is not null)
    {
        await signInManager.SignOutAsync();

        return Results.Ok();
    }

    return Results.Unauthorized();
}).RequireAuthorization();


// provide an endpoint for user roles
app.MapGet("/roles", (ClaimsPrincipal user) =>
{
    if (user.Identity is not null && user.Identity.IsAuthenticated)
    {
        var identity = (ClaimsIdentity)user.Identity;
        var roles = identity.FindAll(identity.RoleClaimType)
            .Select(c =>
                new
                {
                    c.Issuer,
                    c.OriginalIssuer,
                    c.Type,
                    c.Value,
                    c.ValueType
                });

        return TypedResults.Json(roles);
    }

    return Results.Unauthorized();
}).RequireAuthorization();

app.UseResponseCompression();

app.UseCors("corsapp");

app.Run();
