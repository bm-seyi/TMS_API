using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;
using TMS_IDP.Configuration;
using TMS_IDP.Utilities;
using TMS_IDP.Middleware;
using TMS_IDP.DbContext;
using System.Net;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Loading Environment Variables
if (Environment.GetEnvironmentVariable("API__Key") is null)
{
    DotNetEnv.Env.Load(Path.Combine(Environment.CurrentDirectory, "Resources/.env"));
}

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();

string connectionString = builder.Configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException(nameof(connectionString));

// Rate Limit Configuration
var myOptions = new ApiRateLimitSettings();
builder.Configuration.GetSection("ApiRateLimitSettings").Bind(myOptions);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
    options.AddTokenBucketLimiter(policyName: "TokenPolicy", configureOptions: tokenOptions =>
    {
        tokenOptions.TokenLimit = myOptions.TokenLimit; 
        tokenOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; 
        tokenOptions.QueueLimit = myOptions.QueueLimit;
        tokenOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(myOptions.ReplenishmentPeriod);
        tokenOptions.TokensPerPeriod = myOptions.TokensPerPeriod;
        tokenOptions.AutoReplenishment = myOptions.AutoReplenishment;
    });
});

// AspNetCore Identity DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

// Duende Configuration DbContext
builder.Services.AddDbContext<ConfigurationDbContext>(options => options.UseSqlServer(connectionString));

// Duende Persisted DbContext
builder.Services.AddDbContext<PersistedGrantDbContext>(options => options.UseSqlServer(connectionString));

// AspNetCore Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Duende Identity Server Configuration
var migrationsAssembly = typeof(Program).Assembly.GetName().Name;
builder.Services.AddIdentityServer(options =>
{
    options.UserInteraction.LoginUrl = "/Account/Login";
    options.UserInteraction.LogoutUrl = "/Account/Logout";
})
.AddAspNetIdentity<ApplicationUser>()
.AddConfigurationStore(options =>
{
    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
    sql => sql.MigrationsAssembly(migrationsAssembly));
})
.AddOperationalStore(options =>
{
    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
    sql => sql.MigrationsAssembly(migrationsAssembly));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/account/login";
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = "https://localhost:5188";
    options.ClientId = "maui_client";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1.read");
    });
});

// Add Session services to the container
builder.Services.AddDistributedMemoryCache(); // Required for session state
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDatabaseActions, DatabaseActions>();
builder.Services.AddTransient<ISecurityUtils, SecurityUtils>();


// Ensure logging services are added
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();
app.UseStaticFiles();
app.UseHttpsRedirection();
//app.UseMiddleware<ApiMiddleware>();
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await IdentityServerSeeder.Seed(app.Services); // Add Identity Server Seeding
}

app.UseIdentityServer();
app.UseAuthentication();  
app.UseAuthorization();
app.UseSession();

IConfiguration configuration = app.Configuration;
IWebHostEnvironment environment = app.Environment;

app.MapControllers();

app.Run();
