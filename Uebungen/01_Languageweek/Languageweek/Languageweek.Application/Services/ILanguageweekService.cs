using Languageweek.Application.Commands;
using Languageweek.Application.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Languageweek.Application.Services
{
    public interface ILanguageweekService
    {
        // TODO: Add more if needed
        Task<Model.LanguageWeek> CreateLanguageWeekAsync(CreateLanguageWeekCommand command);
        Task DeleteLanguageWeekAsync(int id, bool forceDelete = false);
        Task UpdateLanguageWeekAsync(UpdateLanguageWeekCommand command);
        Task UpdateLanguageWeekPriceAsync(UpdateLanguageWeekPriceCommand command);
    }
}