using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IgnoreCollision : MonoBehaviour
{
	[SerializeField]
	private bool _ignorePlayer;

	[SerializeField]
	private bool _ignoreProbe;

	[SerializeField]
	private bool _ignoreProbeAnchor;

	[SerializeField]
	private bool _preventItemDrop;

	[SerializeField]
	private bool _ignoreMarshmallow;

	private Collider _collider;

	private void Start()
	{
		_collider = GetComponent<Collider>();
		if (_ignoreProbe)
		{
			Physics.IgnoreCollision(_collider, Locator.GetProbe().GetAnchor().GetCollider());
			Physics.IgnoreCollision(_collider, Locator.GetProbe().GetDetectorCollider().GetCollider());
		}
		if (_ignorePlayer)
		{
			Physics.IgnoreCollision(_collider, Locator.GetPlayerCollider());
			Physics.IgnoreCollision(_collider, Locator.GetPlayerController().GetAntiSinkingCollider());
			Physics.IgnoreCollision(_collider, Locator.GetPlayerDetector().GetComponent<Collider>());
			Physics.IgnoreCollision(_collider, Locator.GetPlayerCameraDetector().GetComponent<Collider>());
		}
		if (_ignoreMarshmallow)
		{
			GlobalMessenger<Collider>.AddListener("IgnoreMarshmallowCollider", OnIgnoreMarshmallowCollider);
		}
	}

	private void OnDestroy()
	{
		if (_ignoreMarshmallow)
		{
			GlobalMessenger<Collider>.RemoveListener("IgnoreMarshmallowCollider", OnIgnoreMarshmallowCollider);
		}
	}

	public bool PreventsItemDrop()
	{
		return _preventItemDrop;
	}

	public bool IgnoresProbe()
	{
		return _ignoreProbe;
	}

	public bool IgnoresPlayer()
	{
		return _ignorePlayer;
	}

	public bool IgnoresProbeAnchor()
	{
		return _ignoreProbeAnchor;
	}

	private void OnIgnoreMarshmallowCollider(Collider mallowCollider)
	{
		Physics.IgnoreCollision(_collider, mallowCollider);
	}
}
