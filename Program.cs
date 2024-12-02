using Microsoft.Azure.Cosmos;
using Microsoft.OpenApi.Models;
using System.Reflection;
using WeatherApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(s =>
{
    // Get the connection string from configuration (appsettings.json or environment variables)
    var cosmosDbConnectionString = builder.Configuration.GetConnectionString("CosmosDb");
    return new CosmosClient(cosmosDbConnectionString);
});

// Add services to the container.
builder.Services.AddSingleton(s =>
{
    var cosmosClient = s.GetRequiredService<CosmosClient>();

    var databaseName = "WeatherDatabase";   // Replace with your database name
    var containerName = "WeatherContainer"; // Replace with your container name
    // Create the database and container if they don't exist
    CreateCosmosDbAsync(cosmosClient, databaseName, containerName).GetAwaiter().GetResult();
    return new WeatherService(cosmosClient, databaseName, containerName);
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Weather API",
        Description = "A simple ASP.NET Core 8 Web API for managing weather forecasts.",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com",
            Url = new Uri("https://yourwebsite.com")
        }
    });
});

var app = builder.Build();

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
});

// Add the global exception handler middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Method to create database and container
async Task CreateCosmosDbAsync(CosmosClient cosmosClient, string databaseName, string containerName)
{
    DatabaseResponse databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
    Console.WriteLine($"Database Created (or exists): {databaseResponse.Database.Id}");

    // Define container properties
    var containerProperties = new ContainerProperties(containerName, "/City"); // '/City' is the partition key

    ContainerResponse containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(
        containerProperties,
        throughput: 400  // Adjust throughput as needed
    );

    Console.WriteLine($"Container Created (or exists): {containerResponse.Container.Id}");
}