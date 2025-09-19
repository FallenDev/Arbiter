namespace Arbiter.Net.Security;

public interface INetworkPacketEncryptor
{
    NetworkEncryptionParameters Parameters { get; }
    
    bool IsEncrypted(byte command);
    NetworkPacket Encrypt(NetworkPacket packet, byte sequence);
    NetworkPacket Decrypt(NetworkPacket packet);
}