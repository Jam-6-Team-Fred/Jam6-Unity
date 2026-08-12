using UnityEngine;

public abstract class CustomCollisionChecker : MonoBehaviour
{
	private Transform _transform;

	private OWCustomCollider _customCollider;

	private bool _colliding;

	public OWEvent OnEnterCustomCollider = new OWEvent(32);

	public OWEvent OnExitCustumCollider = new OWEvent(32);

	private void Start()
	{
		_transform = base.transform;
		_customCollider = FindCustomCollider();
		if (_customCollider == null)
		{
			Debug.LogError("Failed to locate custom collider");
			Debug.Break();
		}
	}

	private void OnEnable()
	{
		FixedUpdateManager.Register(this);
	}

	private void OnDisable()
	{
		FixedUpdateManager.Unregister(this);
	}

	protected abstract OWCustomCollider FindCustomCollider();

	public void ManagedFixedUpdate()
	{
		bool flag = _customCollider.IsPointInCollider(_transform.position);
		if (flag != _colliding)
		{
			_colliding = flag;
			if (_colliding)
			{
				OnEnterCustomCollider.Invoke();
			}
			else
			{
				OnExitCustumCollider.Invoke();
			}
		}
	}
}
