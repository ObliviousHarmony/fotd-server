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
    /// Any space beyond the terminator is zeroed so that reused buffers do not leak the
    /// remains of a previous value onto the wire.
    /// </summary>
    public static unsafe void FromString(string str, byte* buffer, int len)
    {
        // The buffer has to hold a terminator even when the string is empty.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(len);

        var destination = new Span<byte>(buffer, len);

        // Encoded straight into the destination; ASCII is one byte per char, so truncating
        // the source to len - 1 chars leaves exactly one byte for the terminator.
        var source = str.AsSpan(0, Math.Min(str.Length, len - 1));
        var written = Encoding.ASCII.GetBytes(source, destination);

        destination[written..].Clear();
    }
}
