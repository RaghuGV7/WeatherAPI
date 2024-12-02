using Newtonsoft.Json;

namespace WeatherApi.Models;

public class WeatherForecast
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }  // Cosmos DB requires an 'Id' property
    public string City { get; set; }
    public DateTime Date { get; set; }
    public double TemperatureC { get; set; }
    public string Summary { get; set; }
}