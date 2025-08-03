using Arbiter.Net.Security;

namespace Arbiter.Net.Client;

public class ClientPacketEncryptor : INetworkPacketEncryptor
{
    public NetworkEncryptionParameters Parameters { get; private set; } = NetworkEncryptionParameters.Default;

    public bool IsEncrypted(byte command)
    {
        return true;
    }
    
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