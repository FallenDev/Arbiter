using System.Threading.Tasks;
using Arbiter.App.Models;

namespace Arbiter.App.Services;

public interface ISettingsService
{
    ArbiterSettings CurrentSettings { get; }
    
    Task<ArbiterSettings> LoadSettingsAsync();
    Task SaveSettingsAsync();
}