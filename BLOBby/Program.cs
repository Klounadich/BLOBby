using BLOBby.Services.Interfaces;
using BLOBby.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();



builder.Services.AddScoped<IBucketService, BucketService>();

var app = builder.Build();


var blobPath = builder.Configuration.GetSection("BLOBby:BlobPath").Value;
if (string.IsNullOrWhiteSpace(blobPath))
{
    throw new InvalidOperationException("BLOBby:BlobPath is not configured in appsettings.json");
}
Directory.CreateDirectory(blobPath); 

if (app.Environment.IsDevelopment())
{
   
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();