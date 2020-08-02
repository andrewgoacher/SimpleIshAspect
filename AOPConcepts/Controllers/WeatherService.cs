using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AOPConcepts.Controllers
{
    public class WeatherService : IWeatherService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        private Random rng = new Random();
        
        public IEnumerable<string> GetSummaries()
        {
            return Summaries;
        }

        public async Task<string> NextSummaryAsync(int next)
        {
            return Summaries[rng.Next(next)];
        }

        public void RandomMethod()
        {
            
        }
    }
}