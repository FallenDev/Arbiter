using System.Threading.Tasks;
using Arbiter.App.Models.Settings;

namespace Arbiter.App.Services.Settings;

public interface ISettingsService
{
    Task<ArbiterSettings> LoadFromFileAsync();
    Task SaveToFileAsync(ArbiterSettings settings);
}