using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KinematicCollider : MonoBehaviour
{
	private Collider _collider;

	private OWRigidbody _owRigidbody;

	private KinematicRigidbody _kinematicRigidbody;

	public Collider collider => _collider;

	public OWRigidbody owRigidbody => _owRigidbody;

	public KinematicRigidbody kinematicRigidbody => _kinematicRigidbody;

	private void Reset()
	{
		base.gameObject.layer = LayerMask.NameToLayer("ProxyPrimitive");
	}

	private void Awake()
	{
		_collider = GetComponent<Collider>();
		if (_collider is MeshCollider && !(_collider as MeshCollider).convex)
		{
			Debug.LogError("Kinematic Mesh Collider must be convex.", this);
		}
		_owRigidbody = this.GetAttachedOWRigidbody();
		if (_owRigidbody == null || !_owRigidbody.IsKinematic())
		{
			Debug.LogError("Kinematic Collider needs to be placed on or under a kinematic Rigidbody.", this);
		}
		else
		{
			_kinematicRigidbody = _owRigidbody.GetComponent<KinematicRigidbody>();
		}
	}
}
