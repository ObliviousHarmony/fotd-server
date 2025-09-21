using System.Runtime.InteropServices;
using System.Text;

namespace FOMServer.Shared.Core.Models.FOMData
{
	/// <summary>
	/// Represents an error encountered while processing a packet.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	unsafe public struct LoginRequest
	{
		public fixed byte username[19];
		public ushort ClientVersion;

		public string Username
		{
			get
			{
				unsafe
				{
					fixed (byte* ptr = username)
					{
						return CStringParser.ReadAscii(ref username[0], 19);
					}
				}
			}
		}
	}
}
