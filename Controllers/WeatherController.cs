using Microsoft.AspNetCore.Mvc;
using WeatherApi.Models;
using WeatherApi.Services;

namespace WeatherApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly WeatherService _weatherService;

    public WeatherController(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _weatherService.GetAllAsync());

    [HttpGet("{id}/{city}")]
    public async Task<IActionResult> Get(string id, string city) => Ok(await _weatherService.GetAsync(id, city));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WeatherForecast forecast)
    {
        //if (string.IsNullOrWhiteSpace(forecast.Id))
        //    forecast.Id = Guid.NewGuid().ToString();

        await _weatherService.AddAsync(forecast);
        return CreatedAtAction(nameof(Get), new { id = forecast.Id, city = forecast.City }, forecast);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] WeatherForecast forecast)
    {
        await _weatherService.UpdateAsync(id, forecast);
        return NoContent();
    }

    [HttpDelete("{id}/{city}")]
    public async Task<IActionResult> Delete(string id, string city)
    {
        await _weatherService.DeleteAsync(id, city);
        return NoContent();
    }

    [HttpGet("sayhello/{name}")]
    public async Task<IActionResult> SayHello(string name)
    {
        return Ok($"Hello, {name}");
    }
}
