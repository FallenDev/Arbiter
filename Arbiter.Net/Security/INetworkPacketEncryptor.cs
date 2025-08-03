namespace Arbiter.Net.Security;

public interface INetworkPacketEncryptor
{
    NetworkEncryptionParameters Parameters { get; }
    
    bool IsEncrypted(byte command);
    NetworkPacket Encrypt(NetworkPacket packet);
    NetworkPacket Decrypt(NetworkPacket packet);

    void SetParameters(NetworkEncryptionParameters parameters);
}