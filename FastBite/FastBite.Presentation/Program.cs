using Asp.Versioning;
using FastBite.Infrastructure.Contexts;
using FastBite.Core.Models;
using FastBite.Implementation.Classes;
using FastBite.Core.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using FastBite.Implementation.Validators;
using FastBite.Shared.DTOS;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CsvHelper;
using System.Globalization;
using FastBite.Infastructure.Hubs;
using FastBite.Presentation.Middlewares;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options => 
{ 
    options.AddPolicy("CorsPolicy", builder => 
    { 
        builder 
            .WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader() 
            .AllowAnyMethod() 
            .AllowCredentials(); 
    }); 
});

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddSignalR();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        RequireExpirationTime = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Value,
        ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Value,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token) && context.Request.Cookies.ContainsKey("accessToken"))
            {
                context.Token = context.Request.Cookies["accessToken"];
            }
            return Task.CompletedTask;
        },

        OnAuthenticationFailed = async context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                var httpContext = context.HttpContext;
                var accessToken = httpContext.Request.Cookies["accessToken"];
                var refreshToken = httpContext.Request.Cookies["refreshToken"];

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var refreshEndpoint = $"http://localhost:5156/api/v1/Auth/Refresh";
                    var client = httpContext.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();
                    
                    var response = await client.PostAsJsonAsync(refreshEndpoint, new TokenDTO(accessToken, refreshToken));
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var newTokens = await response.Content.ReadFromJsonAsync<RefreshDTO>();
                        if (newTokens != null)
                        {
                            httpContext.Response.Cookies.Append("accessToken", newTokens.accessToken, new CookieOptions { HttpOnly = true });
                            httpContext.Response.Cookies.Append("refreshToken", newTokens.refreshToken, new CookieOptions { HttpOnly = true });
                            
                            httpContext.Request.Headers["Authorization"] = $"Bearer {newTokens.accessToken}";

                            var newToken = new JwtSecurityToken(newTokens.accessToken);
                            var principal = new ClaimsPrincipal(new ClaimsIdentity(newToken.Claims, "jwt"));

                            context.Principal = principal;
                            context.Success();  
                        }
                    }
                }
            }
        }
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddDbContext<FastBiteContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHttpClient();

builder.Services.AddScoped<LoginUserValidator>();
builder.Services.AddScoped<RegisterUserValidator>();
builder.Services.AddScoped<ResetPasswordValidator>();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration["Redis:RedisConnection"]));

builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IRecaptchaService, RecaptchaService>();
builder.Services.AddTransient<ITableService, TableService>();
builder.Services.AddTransient<ICheckoutService, CheckoutService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IPartyService, PartyService>();
builder.Services.AddTransient<IRedisService, RedisService>();
builder.Services.AddTransient<IRoleService, RoleService>();
builder.Services.AddTransient<IReservationService, ReservationService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();

builder.Services.AddTransient<IAccountService, AccountService>();

builder.Services.AddScoped<IBlackListService, BlackListService>();
builder.Services.AddScoped<JwtSessionMiddleware>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger");
        return;
    }
    await next();
});


app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<JwtSessionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<CartHub>("/orderHub");


app.Run("http://localhost:5156");