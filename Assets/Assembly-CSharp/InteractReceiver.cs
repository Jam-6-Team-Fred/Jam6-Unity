using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractReceiver : SingleInteractionVolume, IRaycastInteractable
{
	[SerializeField]
	protected float _interactRange = 2f;

	[Space]
	[SerializeField]
	private bool _checkViewAngle;

	[SerializeField]
	private float _maxViewAngle = 180f;

	[Space]
	[SerializeField]
	private bool _usableInShip;

	private OWCollider _owCollider;

	protected override void Awake()
	{
		base.Awake();
		_owCollider = base.gameObject.GetAddComponent<OWCollider>();
		_owCollider.SetLODActivationMask(DynamicOccupant.Player);
		if (!OWLayerMask.IsLayerInMask(base.gameObject.layer, OWLayerMask.blockableInteractMask))
		{
			Debug.LogError("InteractReceivers must be on the Interactible layer!", this);
			Debug.Break();
		}
	}

	protected override void Start()
	{
		base.Start();
		base.enabled = false;
	}

	public void SetInteractRange(float interactRange)
	{
		_interactRange = interactRange;
	}

	public void Observe(RaycastHit hit)
	{
		if (!_usableInShip && PlayerState.IsInsideShip())
		{
			_focused = false;
		}
		else
		{
			_focused = hit.distance < _interactRange && (!_checkViewAngle || Vector3.Angle(base.transform.forward, _playerCam.transform.position - base.transform.position) <= _maxViewAngle);
		}
	}

	public override void EnableInteraction()
	{
		base.EnableInteraction();
		_owCollider.SetActivation(active: true);
	}

	public override void DisableInteraction()
	{
		base.DisableInteraction();
		_owCollider.SetActivation(active: false);
	}
}
