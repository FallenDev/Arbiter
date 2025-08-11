using System.Collections.Generic;
using System.Linq;

namespace Arbiter.App.ViewModels;

public class RawHexViewModel : ViewModelBase
{
    private readonly IReadOnlyCollection<byte> _rawData;

    public string RawHex => string.Join(' ', _rawData.Select(x => x.ToString("X2")));
    
    public RawHexViewModel(IReadOnlyCollection<byte> rawData)
    {
        _rawData = rawData;
    }
}