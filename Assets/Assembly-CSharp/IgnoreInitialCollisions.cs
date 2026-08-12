using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class IgnoreInitialCollisions : MonoBehaviour
{
	[SerializeField]
	private float _ignoreDuration = 0.01f;

	private Collider[] _childColliders;

	private float _initTime;

	public void SetIgnoreDuration(float ignoreDuration)
	{
		_ignoreDuration = ignoreDuration;
	}

	private void Start()
	{
		_childColliders = GetComponentsInChildren<Collider>();
		for (int i = 0; i < _childColliders.Length; i++)
		{
			if (OWLayerMask.IsLayerInMask(_childColliders[i].gameObject.layer, OWLayerMask.physicalMask))
			{
				_childColliders[i].enabled = false;
			}
		}
		_initTime = Time.time;
	}

	private void Update()
	{
		if (!(Time.time > _initTime + _ignoreDuration))
		{
			return;
		}
		for (int i = 0; i < _childColliders.Length; i++)
		{
			if ((OWLayerMask.physicalMask.value & (1 << _childColliders[i].gameObject.layer)) > 0)
			{
				_childColliders[i].enabled = true;
			}
		}
		Object.Destroy(this);
	}
}
