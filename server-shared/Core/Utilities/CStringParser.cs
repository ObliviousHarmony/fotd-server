using System.Text;

public static class CStringParser
{
    /// <summary>
    /// Reads a null-terminated ASCII string from a fixed-size byte buffer.
    /// </summary>
    public static unsafe string ToString(byte* buffer, int len)
    {
        var span = new ReadOnlySpan<byte>(buffer, len);

        int strLength = span.IndexOf((byte)0);
        if (strLength < 0)
            strLength = len;

        return Encoding.ASCII.GetString(span.Slice(0, strLength));
    }

    /// <summary>
    /// Copies a C# string into a fixed-size byte buffer as a null-terminated ASCII string.
    /// </summary>
    public static unsafe void FromString(string str, byte* buffer, int len)
    {
        var bytes = Encoding.ASCII.GetBytes(str);

        int copyLength = Math.Min(bytes.Length, len - 1);
        for (int i = 0; i < copyLength; i++)
            buffer[i] = bytes[i];

        buffer[copyLength] = 0;
    }
}
