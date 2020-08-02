using System.Collections.Generic;
using System.Threading.Tasks;

namespace AOPConcepts.Controllers
{
    public interface IWeatherService
    {
        IEnumerable<string> GetSummaries();
        Task<string> NextSummaryAsync(int next);
        void RandomMethod();
    }
}