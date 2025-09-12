using Arbiter.Net.Types;

namespace Arbiter.Net.Server;

public class ServerItemEntity : ServerEntityObject
{
    public DyeColor DyeColor { get; set; }

    public ushort Unknown { get; set; }
}