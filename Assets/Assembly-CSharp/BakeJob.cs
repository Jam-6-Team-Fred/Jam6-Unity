using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct BakeJob : IJobParallelFor
{
	[ReadOnly]
	private NativeArray<int> meshIds;

	public BakeJob(NativeArray<int> meshIds)
	{
		this.meshIds = meshIds;
	}

	public void Execute(int index)
	{
		Physics.BakeMesh(meshIds[index], convex: false);
	}
}
