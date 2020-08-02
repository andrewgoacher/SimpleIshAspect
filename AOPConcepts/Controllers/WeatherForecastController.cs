using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AOPConcepts.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IWeatherService service;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherService service)
        {
            _logger = logger;
            this.service = service;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
          
            var x = Enumerable.Range(1, 5)
                .Select(async index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC =5,
                    Summary = await service.NextSummaryAsync(service.GetSummaries().Count())
                })
                .ToArray();

            return await Task.WhenAll(x);
        }
    }
}