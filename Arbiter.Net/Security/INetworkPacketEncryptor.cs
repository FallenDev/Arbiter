namespace Arbiter.Net.Security;

public interface INetworkPacketEncryptor
{
    NetworkEncryptionParameters Parameters { get; set; }
    
    bool IsEncrypted(byte command);
    NetworkPacket Encrypt(NetworkPacket packet);
    NetworkPacket Decrypt(NetworkPacket packet);
}