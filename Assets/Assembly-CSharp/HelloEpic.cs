using System;
using UnityEngine;

public class HelloEpic : MonoBehaviour
{
	private void Start()
	{
		EOS_InitializeOptions Options = default(EOS_InitializeOptions);
		Options.ApiVersion = 1;
		Options.AllocateMemoryFunction = IntPtr.Zero;
		Options.ReallocateMemoryFunction = IntPtr.Zero;
		Options.ReleaseMemoryFunction = IntPtr.Zero;
		Options.ProductName = Application.productName;
		Options.ProductVersion = Application.version;
		Debug.Log("Epic Online Services Initialize: " + EpicInit.EOS_Initialize(ref Options));
		Debug.Log("Epic Online Services Shutdown: " + EpicInit.EOS_Shutdown());
	}
}
