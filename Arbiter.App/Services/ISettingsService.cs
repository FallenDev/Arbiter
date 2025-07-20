using System.Threading.Tasks;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public interface ISettingsService
{
    Task<ArbiterSettings> LoadSettingsAsync();
    Task SaveSettingsAsync(ArbiterSettings settings);
}