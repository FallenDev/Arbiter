﻿namespace Arbiter.Net.Serialization;

public interface INetworkSerializable
{
    void Deserialize(NetworkPacketReader reader);
    void Serialize(NetworkPacketBuilder builder);
}