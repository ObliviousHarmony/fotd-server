using System.Text;

internal static class CStringParser
{
    /// <summary>
    /// Reads a null-terminated ASCII string from a fixed-size byte buffer.
    /// </summary>
    public static unsafe string ToString(byte* buffer, int len)
    {
        var span = new ReadOnlySpan<byte>(buffer, len);

        var strLength = span.IndexOf((byte)0);
        if (strLength < 0)
        {
            strLength = len;
        }

        return Encoding.ASCII.GetString(span[..strLength]);
    }

    /// <summary>
    /// Copies a C# string into a fixed-size byte buffer as a null-terminated ASCII string.
    /// </summary>
    public static unsafe void FromString(string str, byte* buffer, int len)
    {
        var bytes = Encoding.ASCII.GetBytes(str);

        var copyLength = Math.Min(bytes.Length, len - 1);
        for (var i = 0; i < copyLength; i++)
        {
            buffer[i] = bytes[i];
        }

        buffer[copyLength] = 0;
    }
}
