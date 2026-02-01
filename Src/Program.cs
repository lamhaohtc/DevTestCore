using Src.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 👇 CORS with specific origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        // List all the origins you want to allow
        policy.WithOrigins(
                "http://localhost:8080",    // Vue dev server (HTTP)
                "https://localhost:8080",   // Vue dev server (HTTPS)
                "http://localhost:3000",    // Alternative Vue port
                "https://localhost:3000"    // Alternative Vue port (HTTPS)
            )
            .AllowAnyMethod()               // Allow all HTTP methods
            .AllowAnyHeader()               // Allow all headers
            .AllowCredentials();            // Allow credentials (cookies, auth headers)
    });
});

builder.Services.AddScoped<IFileUploadService, AzureBlobUploadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowVueApp");

app.UseAuthorization();

app.MapControllers();

app.Run();