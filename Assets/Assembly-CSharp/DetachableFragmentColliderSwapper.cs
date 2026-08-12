using UnityEngine;

public class DetachableFragmentColliderSwapper : MonoBehaviour
{
	[SerializeField]
	private Collider[] _attachedColliders;

	[SerializeField]
	private Collider[] _detachedColliders;

	private DetachableFragment _detachableFragment;

	private void Awake()
	{
		_detachableFragment = this.GetRequiredComponent<DetachableFragment>();
		_detachableFragment.OnDetachFragment += OnDetachFragment;
	}

	private void Start()
	{
		for (int i = 0; i < _detachedColliders.Length; i++)
		{
			_detachedColliders[i].GetComponent<OWCollider>().SetActivation(active: false);
		}
	}

	private void OnDestroy()
	{
		_detachableFragment.OnDetachFragment -= OnDetachFragment;
	}

	private void OnDetachFragment(OWRigidbody fragmentBody, OWRigidbody parentBody)
	{
		for (int i = 0; i < _attachedColliders.Length; i++)
		{
			_attachedColliders[i].GetComponent<OWCollider>().SetActivation(active: false);
		}
		for (int j = 0; j < _detachedColliders.Length; j++)
		{
			_detachedColliders[j].GetComponent<OWCollider>().SetActivation(active: true);
		}
	}
}
