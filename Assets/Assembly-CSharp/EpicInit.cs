using System.Runtime.InteropServices;

public static class EpicInit
{
	[DllImport("EOSSDK-Win64-Shipping")]
	public static extern EOS_EResult EOS_Initialize(ref EOS_InitializeOptions Options);

	[DllImport("EOSSDK-Win64-Shipping")]
	public static extern EOS_EResult EOS_Shutdown();
}
