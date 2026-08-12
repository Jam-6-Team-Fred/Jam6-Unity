using UnityEngine;

[AddComponentMenu("Streaming/Dream Campfire Streaming", 200)]
public class DreamCampfireStreaming : SectoredMonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _streamingVolume;

	[SerializeField]
	private DreamCampfire _dreamCampfire;

	private StreamingGroup _streamingGroup;

	private ItemTool _itemTool;

	private bool _playerInVolume;

	private bool _preloadingAssets;

	protected override void Awake()
	{
		base.Awake();
		_streamingVolume.OnEntry += OnEntry;
		_streamingVolume.OnExit += OnExit;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_streamingVolume.OnEntry -= OnEntry;
		_streamingVolume.OnExit -= OnExit;
	}

	private void Start()
	{
		_streamingGroup = StreamingGroup.GetStreamingGroup("DreamWorld");
		_itemTool = Locator.GetToolModeSwapper().GetItemCarryTool();
		base.enabled = false;
	}

	private void FixedUpdate()
	{
		bool flag = _dreamCampfire.GetState() == Campfire.State.LIT;
		bool flag2 = _itemTool.GetHeldItemType() == ItemType.DreamLantern;
		bool flag3 = flag2 && (_itemTool.GetHeldItem() as DreamLanternItem).GetLanternType() != DreamLanternType.Nonfunctioning;
		if (_streamingGroup != null)
		{
			bool shouldBeLoadingAssets = _playerInVolume && flag && flag2 && flag3;
			UpdatePreloadingState(shouldBeLoadingAssets);
		}
	}

	private void UpdatePreloadingState(bool shouldBeLoadingAssets)
	{
		if (!_preloadingAssets && shouldBeLoadingAssets)
		{
			_streamingGroup.RequestRequiredAssets();
			_streamingGroup.RequestGeneralAssets();
			_preloadingAssets = true;
		}
		else if (_preloadingAssets && !shouldBeLoadingAssets)
		{
			_streamingGroup.ReleaseRequiredAssets();
			_streamingGroup.ReleaseGeneralAssets();
			_preloadingAssets = false;
		}
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player && StreamingManager.isStreamingEnabled)
		{
			base.enabled = true;
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			if (_streamingGroup != null)
			{
				UpdatePreloadingState(shouldBeLoadingAssets: false);
			}
			base.enabled = false;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
		if (attachedOWRigidbody != null && attachedOWRigidbody.CompareTag("Player"))
		{
			_playerInVolume = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
		if (attachedOWRigidbody != null && attachedOWRigidbody.CompareTag("Player"))
		{
			_playerInVolume = false;
		}
	}
}
