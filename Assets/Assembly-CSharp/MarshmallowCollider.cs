using UnityEngine;

public class MarshmallowCollider : MonoBehaviour
{
	[SerializeField]
	private Collider _collider;

	private void Awake()
	{
		GlobalMessenger<Collider>.FireEvent("IgnoreMarshmallowCollider", _collider);
	}
}
