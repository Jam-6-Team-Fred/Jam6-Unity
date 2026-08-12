using System.Collections.Generic;
using UnityEngine;

public class OcclusionSphere : MonoBehaviour
{
	public static List<OcclusionSphere> _occluders;

	[SerializeField]
	private float _radius = 50f;

	private SphereBounds _sphereBounds;

	public SphereBounds GetSphereBounds()
	{
		return _sphereBounds;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one * Mathf.Min(base.transform.lossyScale.x, base.transform.lossyScale.y, base.transform.lossyScale.z));
			Gizmos.color = Color.black * 0.5f;
			Gizmos.DrawSphere(Vector3.zero, _radius);
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(Vector3.zero, _radius);
		}
	}
}
