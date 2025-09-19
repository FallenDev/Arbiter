namespace Arbiter.Net.Security;

// CRC-16/CCITT-FALSE
public class Crc16Provider
{
    public ushort[] Table { get; } = new ushort[256];

    public Crc16Provider(ushort polynomial = 0x1021)
    {
        GenerateTable(polynomial, Table);
    }

    public ushort Compute(ReadOnlySpan<byte> data, ushort initial = 0x0000)
    {
        var crc = initial;
        foreach (var value in data)
        {
            crc = (ushort)(value ^ (crc << 8) ^ Table[crc >> 8]);
        }

        return crc;
    }

    private static void GenerateTable(ushort polynomial, Span<ushort> table)
    {
        for (var i = 0; i < 256; i++)
        {
            var crc = (ushort)(i << 8);
            for (var bit = 0; bit < 8; bit++)
            {
                if ((crc & 0x8000) != 0)
                {
                    crc = (ushort)((crc << 1) ^ polynomial);
                }
                else
                {
                    crc <<= 1;
                }
            }

            table[i] = crc;
        }
    }
}