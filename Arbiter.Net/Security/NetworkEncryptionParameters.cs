namespace Arbiter.Net.Security;

public class NetworkEncryptionParameters
{
    public static NetworkEncryptionParameters Default => new (0, DefaultKey.Span);
    
    public const int KeyLength = 9;
    public const int KeyTableSize = 1024;
    public const int SaltTableSize = 256;
    public static readonly ReadOnlyMemory<byte> DefaultKey = "UrkcnItnI"u8.ToArray();

    private readonly byte[] _privateKey = new byte[KeyLength];
    private readonly byte[] _saltTable = new byte[SaltTableSize];
    private readonly byte[] _keyTable = new byte[KeyTableSize];

    public string? Name { get; init; }
    public int Seed { get; init; }
    public ReadOnlyMemory<byte> PrivateKey => _privateKey;
    public ReadOnlyMemory<byte> SaltTable => _saltTable;
    public ReadOnlyMemory<byte> KeyTable => _keyTable;

    public NetworkEncryptionParameters(int seed, ReadOnlySpan<byte> privateKey, string? name = null)
    {
        Seed = seed;
        SaltGenerator.GenerateSalt(seed, _saltTable);

        privateKey.CopyTo(_privateKey);

        if (name is not null)
        {
            KeyTableGenerator.GenerateKeyTable(name, _keyTable);
        }
    }

    public void GenerateKey(ushort bRand, ushort sRand, Span<byte> outputBuffer)
    {
        for (var i = 0; i < KeyLength; i++)
        {
            var index = (i * (KeyLength * i + sRand * sRand) + bRand) % KeyTableSize;
            outputBuffer[i] = _keyTable[index];
        }
    }
}