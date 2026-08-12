using UnityEngine;

public class RaftFluidDetector : AlignToSurfaceFluidDetector
{
	[SerializeField]
	private Vector3[] _groundRaycastCheckPoints;

	private RaftController _raftController;

	public void RegisterRaftController(RaftController raftController)
	{
		_raftController = raftController;
	}

	public override bool AffectsRumble()
	{
		return _raftController.IsPlayerRiding();
	}

	public bool IsAnyPointGrounded()
	{
		bool result = false;
		Vector3[] groundRaycastCheckPoints = _groundRaycastCheckPoints;
		foreach (Vector3 position in groundRaycastCheckPoints)
		{
			if (Physics.Raycast(new Ray(base.transform.TransformPoint(position), -base.transform.up), out var _, 0.25f))
			{
				result = true;
			}
		}
		return result;
	}
}
