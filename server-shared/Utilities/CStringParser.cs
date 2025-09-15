using System;
using System.Text;

public static class CStringParser
{
	/// <summary>
	/// Reads a null-terminated ASCII string from a fixed-size byte buffer.
	/// </summary>
	public static unsafe string ReadAscii(ref byte firstByte, int length)
	{
		fixed (byte* ptr = &firstByte)
		{
			var span = new ReadOnlySpan<byte>(ptr, length);
			int strLength = span.IndexOf((byte)0);
			if (strLength < 0)
				strLength = length;
			return Encoding.ASCII.GetString(span.Slice(0, strLength));
		}
	}
}
