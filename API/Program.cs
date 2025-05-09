using Microsoft.IdentityModel.Tokens;
using TMS_API.Hubs;
using TMS_API.Utilities;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
    string parentDirectory = Directory.GetParent(Environment.CurrentDirectory)?.FullName ?? throw new ArgumentNullException(nameof(parentDirectory));
    DotNetEnv.Env.Load(Path.Combine(parentDirectory, ".env"));
#endif

builder.Configuration.AddJsonFile("appsettings.json", optional: false).AddEnvironmentVariables();
string connectionString = builder.Configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException(nameof(connectionString));

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5188";
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = "api1",
            ValidateIssuer = true,
            ValidIssuer = "https://localhost:5188",
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection
builder.Services.AddScoped<IDatabaseActions, DatabaseActions>();
builder.Services.AddScoped<IDatabaseConnection, DatabaseConnection>();

// Background Services
builder.Services.AddHostedService<LinesBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<LinesHub>("/LinesHub");

app.Run();
