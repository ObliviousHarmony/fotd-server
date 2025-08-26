using System;
using System.Runtime.InteropServices;

class Program
{
	[DllImport("mylib", EntryPoint = "add", CallingConvention = CallingConvention.Cdecl)]
	public static extern int Add(int a, int b);

	static void Main(string[] args)
	{
		Console.WriteLine($"3 + 4 = {Add(3, 4)}");
	}
}
