using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class StreamingCollisionMeshBakeManager : MonoBehaviour
{
	private const int MESHES_PER_BATCH = 5;

	private const int MESHES_PER_JOB = 1;

	private const int SCHEDULED_JOBS_LIMIT = 1;

	private List<int> _instanceIDQueue = new List<int>();

	private List<int> _readyMeshes = new List<int>();

	private List<(JobHandle job, List<int> array)> _runningJobs = new List<(JobHandle, List<int>)>(1);

	private int[] _batchedIDs = new int[5];

	private void Update()
	{
		if (_runningJobs.Count < 1)
		{
			ScheduleBatch();
		}
	}

	private void LateUpdate()
	{
		CheckJobs();
	}

	private void ScheduleBatch()
	{
		if (_instanceIDQueue.Count > 0)
		{
			int num = Mathf.Min(_instanceIDQueue.Count, 5);
			int num2 = 0;
			while (num > 0)
			{
				_batchedIDs[num2] = _instanceIDQueue[0];
				_instanceIDQueue.RemoveAt(0);
				num2++;
				num--;
			}
			ScheduleJob(num2);
		}
	}

	private void CheckJobs(bool callComplete = true)
	{
		int num = 0;
		while (_runningJobs.Count > 0 && num < _runningJobs.Count)
		{
			if (_runningJobs[num].job.IsCompleted)
			{
				if (callComplete)
				{
					_runningJobs[num].job.Complete();
				}
				for (int i = 0; i < _runningJobs[num].array.Count; i++)
				{
					_readyMeshes.SafeAdd(_runningJobs[num].array[i]);
				}
				_runningJobs[num].array.Clear();
				_runningJobs.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public bool ReadyOrAdd(int instanceID)
	{
		for (int i = 0; i < _readyMeshes.Count; i++)
		{
			if (_readyMeshes[i] == instanceID)
			{
				return true;
			}
		}
		for (int j = 0; j < _runningJobs.Count; j++)
		{
			for (int k = 0; k < _runningJobs[j].array.Count; k++)
			{
				if (_runningJobs[j].array[k] == instanceID)
				{
					return false;
				}
			}
		}
		for (int l = 0; l < _instanceIDQueue.Count; l++)
		{
			if (_instanceIDQueue[l] == instanceID)
			{
				return false;
			}
		}
		_instanceIDQueue.Add(instanceID);
		return false;
	}

	public bool IsBeingWorkedOn(int instanceID)
	{
		for (int i = 0; i < _runningJobs.Count; i++)
		{
			for (int j = 0; j < _runningJobs[i].array.Count; j++)
			{
				if (_runningJobs[i].array[j] == instanceID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void StreamingBundleWillUnload(List<int> meshesToUnload)
	{
		while (meshesToUnload.Count > 0)
		{
			if (_readyMeshes.Contains(meshesToUnload[0]))
			{
				_readyMeshes.Remove(meshesToUnload[0]);
			}
			meshesToUnload.RemoveAt(0);
		}
	}

	private void ScheduleJob(int batchCount)
	{
		NativeArray<int> meshIds = new NativeArray<int>(batchCount, Allocator.TempJob);
		List<int> list = new List<int>(batchCount);
		for (int i = 0; i < batchCount; i++)
		{
			meshIds[i] = _batchedIDs[i];
			list.Add(_batchedIDs[i]);
		}
		BakeJob jobData = new BakeJob(meshIds);
		_runningJobs.Add((jobData.Schedule(batchCount, 1), list));
		meshIds.Dispose();
	}

	public void ForceAllJobsComplete()
	{
		for (int i = 0; i < _runningJobs.Count; i++)
		{
			_runningJobs[i].job.Complete();
		}
		CheckJobs(callComplete: false);
	}
}
