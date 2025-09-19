namespace Arbiter.Net.Security;

// CRC-16/CCITT-FALSE
public class Crc16Provider
{
    public ushort[] Table { get; } = new ushort[256];

    public Crc16Provider(ushort polynomial = 0x1021)
    {
        GenerateTable(polynomial, Table);
    }

    public ushort Compute(ReadOnlySpan<byte> data, ushort initial = 0xFFFF)
    {
        var crc = initial;
        foreach (var b in data)
        {
            crc = (ushort)((crc << 8) ^ Table[((crc >> 8) ^ b) & 0xFF]);
        }

        return crc;
    }

    private static void GenerateTable(ushort polynomial, Span<ushort> table)
    {
        for (var i = 0; i < 256; i++)
        {
            var v = (ushort)(i << 8);
            for (var b = 0; b < 8; b++)
            {
                if ((v & 0x8000) != 0)
                {
                    v = (ushort)((v << 1) ^ polynomial);
                }
                else
                {
                    v <<= 1;
                }
            }

            table[i] = v;
        }
    }
}