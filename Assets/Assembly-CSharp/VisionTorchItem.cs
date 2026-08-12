using UnityEngine;

public class VisionTorchItem : OWItem
{
	[SerializeField]
	private MindProjectorTrigger _mindProjectorTrigger;

	[SerializeField]
	private MindSlideProjector _mindSlideProjector;

	[SerializeField]
	private Transform _visionBeam;

	[Space]
	[SerializeField]
	private OWRenderer[] _worldModelRenderers = new OWRenderer[0];

	[SerializeField]
	private OWRenderer[] _viewModelRenderers = new OWRenderer[0];

	private VisionTorchSocket _originalSocket;

	private bool _wasProjecting;

	private bool _isProjecting;

	public MindProjectorTrigger mindProjectorTrigger => _mindProjectorTrigger;

	public MindSlideProjector mindSlideProjector => _mindSlideProjector;

	protected override void Awake()
	{
		_type = ItemType.VisionTorch;
		base.Awake();
	}

	private void Start()
	{
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_mindSlideProjector.OnProjectionComplete -= new OWEvent.OWCallback(OnProjectionComplete);
	}

	public override string GetDisplayName()
	{
		return UITextLibrary.GetString(UITextType.ItemVisionTorchPrompt);
	}

	public void SetOriginalSocket(VisionTorchSocket socket)
	{
		_originalSocket = socket;
	}

	public override void PickUpItem(Transform holdTranform)
	{
		base.PickUpItem(holdTranform);
		if (_visionBeam != null)
		{
			_visionBeam.localScale = Vector3.one * 5f;
		}
		for (int i = 0; i < _worldModelRenderers.Length; i++)
		{
			_worldModelRenderers[i].SetActivation(active: false);
		}
		for (int j = 0; j < _viewModelRenderers.Length; j++)
		{
			_viewModelRenderers[j].SetActivation(active: true);
		}
		base.enabled = true;
	}

	public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
	{
		if (_isProjecting)
		{
			_mindProjectorTrigger.SetProjectorActive(active: false);
			_isProjecting = false;
		}
		if (Locator.GetDreamWorldController().IsInDream())
		{
			base.DropItem(position, normal, parent, sector, customDropTarget);
		}
		if (_visionBeam != null)
		{
			_visionBeam.localScale = Vector3.one;
		}
		for (int i = 0; i < _worldModelRenderers.Length; i++)
		{
			_worldModelRenderers[i].SetActivation(active: true);
		}
		for (int j = 0; j < _viewModelRenderers.Length; j++)
		{
			_viewModelRenderers[j].SetActivation(active: false);
		}
		base.enabled = false;
	}

	public override void SocketItem(Transform socketTransform, Sector sector)
	{
		if (_isProjecting)
		{
			_mindProjectorTrigger.SetProjectorActive(active: false);
			_isProjecting = false;
		}
		base.SocketItem(socketTransform, sector);
		if (_visionBeam != null)
		{
			_visionBeam.localScale = Vector3.one;
		}
		for (int i = 0; i < _worldModelRenderers.Length; i++)
		{
			_worldModelRenderers[i].SetActivation(active: true);
		}
		for (int j = 0; j < _viewModelRenderers.Length; j++)
		{
			_viewModelRenderers[j].SetActivation(active: false);
		}
		base.enabled = false;
	}

	private void Update()
	{
		if (PlayerState.IsViewingProjector() && _mindSlideProjector.mindSlideCollection.slideCollectionContainer.slideIndex == 1)
		{
			OWInput.ChangeInputMode(InputMode.None);
			_mindSlideProjector.OnProjectionComplete += new OWEvent.OWCallback(OnProjectionComplete);
			base.enabled = false;
			return;
		}
		_wasProjecting = _isProjecting;
		_isProjecting = OWInput.IsPressed(InputLibrary.toolActionPrimary, InputMode.Character);
		if (_isProjecting && !_wasProjecting)
		{
			_mindProjectorTrigger.SetProjectorActive(active: true);
		}
		else if (!_isProjecting && _wasProjecting)
		{
			_mindProjectorTrigger.SetProjectorActive(active: false);
		}
	}

	private void OnProjectionComplete()
	{
		OWInput.ChangeInputMode(InputMode.Character);
		_isProjecting = true;
		base.enabled = true;
	}
}
