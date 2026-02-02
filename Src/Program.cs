using Src.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 👇 Read CORS origins from appsettings.json
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

// If no origins configured, use default development origins
if (corsOrigins == null || corsOrigins.Length == 0)
{
    corsOrigins = new[] { "http://localhost:8080", "https://localhost:8080" };
    Console.WriteLine("⚠️ No CORS origins configured in appsettings.json. Using defaults: " + string.Join(", ", corsOrigins));
}

Console.WriteLine($"✅ CORS Origins configured: {string.Join(", ", corsOrigins)}");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IFileUploadService, AzureBlobUploadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    Console.WriteLine("🚀 Running in Development mode");
}

app.UseHttpsRedirection();
app.UseCors("AllowVueApp");
app.UseAuthorization();
app.MapControllers();

app.Run();