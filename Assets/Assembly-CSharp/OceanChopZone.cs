using UnityEngine;

[ExecuteInEditMode]
public class OceanChopZone : MonoBehaviour
{
	[SerializeField]
	private OceanEffectController _ocean;

	[SerializeField]
	private float _radius = 10f;

	public float localRadius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = Mathf.Max(value, 0f);
		}
	}

	public float globalRadius
	{
		get
		{
			float num = Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.z);
			return _radius * num;
		}
	}

	private void OnEnable()
	{
		if (!(_ocean == null))
		{
			_ocean.AddChopZone(this);
		}
	}

	private void OnDisable()
	{
		if (!(_ocean == null))
		{
			_ocean.RemoveChopZone(this);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireSphere(Vector3.zero, _radius);
		}
	}
}
