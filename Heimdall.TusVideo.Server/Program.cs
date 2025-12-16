using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Upload-Offset", "Location", "Upload-Length", "Tus-Resumable");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// Create uploads directory if it doesn't exist
var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
Directory.CreateDirectory(uploadPath);

// Configure TUS middleware
app.UseTus(httpContext => new DefaultTusConfiguration
{
    Store = new TusDiskStore(uploadPath),
    UrlPath = "/files",
    Events = new Events
    {
        OnFileCompleteAsync = async eventContext =>
        {
            var file = await eventContext.GetFileAsync();
            var metadata = await file.GetMetadataAsync(eventContext.CancellationToken);
            
            Console.WriteLine($"Upload complete: {file.Id}");
            if (metadata.ContainsKey("name"))
            {
                Console.WriteLine($"File name: {metadata["name"].GetString(System.Text.Encoding.UTF8)}");
            }
        }
    }
});

app.MapGet("/", () => "TUS Video Upload Server is running. Upload endpoint: /files");

app.Run();
