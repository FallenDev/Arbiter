namespace Arbiter.Net.Security;

public class Crc32Provider
{
    public uint[] Table { get; } = new uint[256];

    public Crc32Provider(uint polynomial = 0xEDB88320)
    {
        GenerateTable(polynomial, Table);
    }

    public uint Compute(ReadOnlySpan<byte> data, uint initial = 0xFFFFFFFF, uint finalXor = 0xFFFFFFFF)
    {
        var crc = initial;
        foreach (var value in data)
        {
            crc = (crc >> 8) ^ Table[(crc ^ value) & 0xFF];
        }

        return crc ^ finalXor;
    }

    private static void GenerateTable(uint polynomial, Span<uint> table)
    {
        for (var i = 0; i < 256; i++)
        {
            var crc = (uint)i;
            for (var bit = 0; bit < 8; bit++)
            {
                if ((crc & 1) != 0)
                {
                    crc = (crc >> 1) ^ polynomial;
                }
                else
                {
                    crc >>= 1;
                }
            }
            table[i] = crc;
        }
    }
}