using System;
using UnityEngine;

[Serializable]
public struct RiverMarkerGroup
{
	public Vector3[] localPositions;

	public Vector3[] localRightDirs;

	public float[] magnitudes;

	public float[] degrees;

	public int Count
	{
		get
		{
			if (localPositions != null)
			{
				return localPositions.Length;
			}
			return 0;
		}
	}

	public RiverMarkerGroup(RiverFlowMarker[] markers, Transform riverTransform, OWRingRiverCollider riverCollider)
	{
		localPositions = new Vector3[markers.Length];
		localRightDirs = new Vector3[markers.Length];
		magnitudes = new float[markers.Length];
		degrees = new float[markers.Length];
		for (int i = 0; i < markers.Length; i++)
		{
			localPositions[i] = riverTransform.InverseTransformPoint(markers[i].transform.position);
			localRightDirs[i] = riverTransform.InverseTransformDirection(markers[i].transform.right);
			magnitudes[i] = markers[i].magnitude;
			degrees[i] = riverCollider.LocalPositionToDegrees(localPositions[i]);
		}
	}
}
