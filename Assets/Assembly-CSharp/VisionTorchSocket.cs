public class VisionTorchSocket : OWItemSocket
{
	private OWCollider _owCollider;

	protected override void Awake()
	{
		base.Awake();
		_acceptableType = ItemType.VisionTorch;
		_owCollider = GetComponent<OWCollider>();
	}

	protected override void Start()
	{
		base.Start();
		if (_socketedItem != null)
		{
			(_socketedItem as VisionTorchItem).SetOriginalSocket(this);
		}
	}

	public override bool UsesGiveTakePrompts()
	{
		return true;
	}

	public override void EnableInteraction(bool value)
	{
		base.EnableInteraction(value);
		_owCollider.SetActivation(value);
	}
}
