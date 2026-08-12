using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MultiInteractReceiver : MultipleInteractionVolume, IRaycastInteractable
{
	[SerializeField]
	protected float _interactRange = 2f;

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
			Debug.LogError("InteractReceivers must be on the Interactible layer!");
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
			_focused = hit.distance < _interactRange;
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
