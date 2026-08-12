using System.Text;
using UnityEngine;

public class SystemInfoLogger : MonoBehaviour, IPermanentManagerWorker
{
	public void InitializeOnAwake()
	{
		Debug.Log("Outer Wilds Version: " + Application.version);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Device Model: ");
		stringBuilder.Append(SystemInfo.deviceModel.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("Device Name: ");
		stringBuilder.Append(SystemInfo.deviceName.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("Device Type: ");
		stringBuilder.Append(SystemInfo.deviceType.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("Device ID: ");
		stringBuilder.Append(SystemInfo.deviceUniqueIdentifier.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("Operating System: ");
		stringBuilder.Append(SystemInfo.operatingSystem.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("Processor Name: ");
		stringBuilder.Append(SystemInfo.processorType.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("Processor Count: ");
		stringBuilder.Append(SystemInfo.processorCount.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("Processor Frequency: ");
		stringBuilder.Append(SystemInfo.processorFrequency.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("System Memory: ");
		stringBuilder.Append(SystemInfo.systemMemorySize.ToString());
		stringBuilder.Append(" MB");
		stringBuilder.Append("\n");
		stringBuilder.Append("GFX Device Name: ");
		stringBuilder.Append(SystemInfo.graphicsDeviceName.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("GFX Device Type: ");
		stringBuilder.Append(SystemInfo.graphicsDeviceType.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("GFX Device ID: ");
		stringBuilder.Append(SystemInfo.graphicsDeviceID.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("GFX Device Vendor: ");
		stringBuilder.Append(SystemInfo.graphicsDeviceVendor.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("GFX Device Version: ");
		stringBuilder.Append(SystemInfo.graphicsDeviceVersion.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("GFX Device Memory: ");
		stringBuilder.Append(SystemInfo.graphicsMemorySize.ToString());
		stringBuilder.Append(" MB");
		stringBuilder.Append("\n");
		stringBuilder.Append("GFX Device Shader Level: ");
		stringBuilder.Append(SystemInfo.graphicsShaderLevel.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("Audio Device Available: ");
		stringBuilder.Append(SystemInfo.supportsAudio.ToString());
		stringBuilder.Append("\n");
		Debug.Log(stringBuilder.ToString());
	}
}
