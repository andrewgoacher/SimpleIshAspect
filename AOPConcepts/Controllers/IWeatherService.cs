using System.Collections.Generic;

namespace AOPConcepts.Controllers
{
    public interface IWeatherService
    {
        IEnumerable<string> GetSummaries();
        string NextSummary(int next);
        void RandomMethod();
    }
}