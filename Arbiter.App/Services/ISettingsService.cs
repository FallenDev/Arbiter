using System.Threading.Tasks;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public interface ISettingsService
{
    Task<ArbiterSettings> LoadFromFileAsync();
    Task SaveToFileAsync(ArbiterSettings settings);
}