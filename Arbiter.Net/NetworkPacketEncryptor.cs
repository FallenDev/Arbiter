using Arbiter.Net.Security;

namespace Arbiter.Net;

public abstract class NetworkPacketEncryptor
{
    private const int KeyLength = 9;
    private const int SaltLength = 256;

    protected static readonly byte[] DefaultKey = "UrkcnItnI"u8.ToArray();

    private int _saltAlgorithm;
    private readonly byte[] _privateKey = new byte[KeyLength];
    private readonly byte[] _saltTable = new byte[SaltLength];

    public int SaltAlgorithm => _saltAlgorithm;
    public ReadOnlySpan<byte> PrivateKey => _privateKey;

    protected NetworkPacketEncryptor()
    {
        SetPrivateKey(DefaultKey);
        SetSaltAlgorithm(0);
    }

    public void SetSaltAlgorithm(int algorithm)
    {
        if (algorithm is < 0 or > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(algorithm), "Invalid salt algorithm");
        }

        _saltAlgorithm = algorithm;
        SaltGenerator.GenerateSalt(algorithm, _saltTable);
    }

    public void SetPrivateKey(ReadOnlySpan<byte> keyBytes)
    {
        if (keyBytes.Length != PrivateKey.Length)
        {
            throw new ArgumentException("Key length must match");
        }

        keyBytes.CopyTo(_privateKey);
    }

    public abstract NetworkPacket Encrypt(NetworkPacket packet);
    public abstract NetworkPacket Decrypt(NetworkPacket packet);
}