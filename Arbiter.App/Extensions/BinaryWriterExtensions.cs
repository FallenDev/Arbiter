using System.IO;
using System.Text;

namespace Arbiter.App.Extensions;

public static class BinaryWriterExtensions
{
    public static void WriteNullTerminated(this BinaryWriter writer, string value, Encoding? encoding = null)
    {
        var charBytes = (encoding ?? Encoding.UTF8).GetBytes(value);
        writer.Write(charBytes);
        writer.Write((byte)0);
    }
}