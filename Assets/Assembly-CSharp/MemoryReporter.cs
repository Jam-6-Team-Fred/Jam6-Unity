using System.IO;
using UnityEngine.Profiling;

public class MemoryReporter
{
	public static long ReportUsedMemory()
	{
		return Profiler.GetTotalAllocatedMemoryLong() / 1000000;
	}

	public static long GetSizeOfBundle(string path)
	{
		return new FileInfo(path).Length / 1000000;
	}
}
