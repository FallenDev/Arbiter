using System;
using System.Diagnostics.CodeAnalysis;
using Arbiter.Net;

namespace Arbiter.App.Models.Packets;

public interface IPacketMessage
{
    void ReadFrom(NetworkPacketReader reader);

    bool TryReadFrom(NetworkPacketReader reader, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;
        
        try
        {
            ReadFrom(reader);
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }
}