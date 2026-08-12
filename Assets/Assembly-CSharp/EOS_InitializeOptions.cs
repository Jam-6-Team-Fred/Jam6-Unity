using System;
using System.Runtime.InteropServices;

public struct EOS_InitializeOptions
{
	public int ApiVersion;

	public IntPtr AllocateMemoryFunction;

	public IntPtr ReallocateMemoryFunction;

	public IntPtr ReleaseMemoryFunction;

	[MarshalAs(UnmanagedType.LPStr)]
	public string ProductName;

	[MarshalAs(UnmanagedType.LPStr)]
	public string ProductVersion;
}
