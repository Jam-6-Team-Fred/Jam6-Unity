using UnityEngine;

[ExecuteInEditMode]
public class OceanCalmZone : MonoBehaviour
{
	[SerializeField]
	private OceanEffectController _ocean;

	[SerializeField]
	private float _radius = 10f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _fadeFactor = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _strength = 0.25f;

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
			float num = Mathf.Max(Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.y), base.transform.lossyScale.z);
			return _radius * num;
		}
	}

	public float fadeFactor
	{
		get
		{
			return _fadeFactor;
		}
		set
		{
			_fadeFactor = Mathf.Max(value, 0f);
		}
	}

	public float strength
	{
		get
		{
			return _strength;
		}
		set
		{
			_strength = value;
		}
	}

	private void OnValidate()
	{
		if (_radius < 0f)
		{
			_radius = 0f;
		}
	}

	private void OnEnable()
	{
		if (!(_ocean == null))
		{
			_ocean.AddCalmZone(this);
		}
	}

	private void OnDisable()
	{
		if (!(_ocean == null))
		{
			_ocean.RemoveCalmZone(this);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireSphere(Vector3.zero, _radius);
			Gizmos.DrawWireSphere(Vector3.zero, _radius * (1f - _fadeFactor));
		}
	}
}
