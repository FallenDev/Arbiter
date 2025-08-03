using Arbiter.Net.Security;

namespace Arbiter.Net.Server;

public class ServerPacketEncryptor : INetworkPacketEncryptor
{
    public NetworkEncryptionParameters Parameters { get; private set; } = NetworkEncryptionParameters.Default;

    public NetworkPacket Encrypt(NetworkPacket packet)
    {
        return packet;
    }

    public NetworkPacket Decrypt(NetworkPacket packet)
    {
        return packet;
    }

    public void SetParameters(NetworkEncryptionParameters parameters)
    {
        Parameters = parameters;
    }
}