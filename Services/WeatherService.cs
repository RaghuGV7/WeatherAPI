using Microsoft.Azure.Cosmos;
using WeatherApi.Models;

namespace WeatherApi.Services;

public class WeatherService
{
    private readonly Container _container;

    public WeatherService(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<IEnumerable<WeatherForecast>> GetAllAsync()
    {
        var query = _container.GetItemQueryIterator<WeatherForecast>();
        var results = new List<WeatherForecast>();

        while (query.HasMoreResults)
        {
            foreach (var item in await query.ReadNextAsync())
            {
                results.Add(item);
            }
        }

        return results;
    }

    // Create
    public async Task AddAsync(WeatherForecast forecast)
    {
        if (string.IsNullOrEmpty(forecast.City))
            throw new ArgumentException("City must be specified for the weather forecast.");

        // Ensure that the Id is set, for example using a GUID or a unique string
        if (string.IsNullOrEmpty(forecast.Id))
            forecast.Id = Guid.NewGuid().ToString();  // Set a new unique ID if not provided


        //var doc = new WeatherForecastEntity(forecast.Id, forecast.City, forecast.Date, forecast.TemperatureC, forecast.Summary);
        await _container.CreateItemAsync(forecast, new PartitionKey(forecast.City));
    }

    // Read
    public async Task<WeatherForecast> GetAsync(string id, string city)
    {
        return await _container.ReadItemAsync<WeatherForecast>(id, new PartitionKey(city));
    }

    // Update
    public async Task UpdateAsync(string id, WeatherForecast forecast)
    {
        await _container.UpsertItemAsync(forecast, new PartitionKey(forecast.City));
    }

    // Delete
    public async Task DeleteAsync(string id, string city)
    {
        await _container.DeleteItemAsync<WeatherForecast>(id, new PartitionKey(city));
    }
}
