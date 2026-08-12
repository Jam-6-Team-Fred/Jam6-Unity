using UnityEngine;

[AddComponentMenu("Streaming/Nomai Remote Camera Streaming", 200)]
[RequireComponent(typeof(OWTriggerVolume))]
public class NomaiRemoteCameraStreaming : SectoredMonoBehaviour
{
	private OWTriggerVolume _owTriggerVolume;

	[SerializeField]
	private NomaiRemoteCameraPlatform _remoteCameraPlatform;

	private StreamingGroup _streamingGroup;

	private ItemTool _itemTool;

	private SharedStone _heldStone;

	private bool _playerInVolume;

	private bool _preloadingRequiredAssets;

	private bool _preloadingGeneralAssets;

	protected override void Awake()
	{
		base.Awake();
		_owTriggerVolume = GetComponent<OWTriggerVolume>();
		_owTriggerVolume.OnEntry += OnEntry;
		_owTriggerVolume.OnExit += OnExit;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_owTriggerVolume.OnEntry -= OnEntry;
		_owTriggerVolume.OnExit -= OnExit;
	}

	private void Start()
	{
		_itemTool = Locator.GetToolModeSwapper().GetItemCarryTool();
		base.enabled = false;
	}

	private void FixedUpdate()
	{
		OWItem heldItem = _itemTool.GetHeldItem();
		if (heldItem != _heldStone)
		{
			SharedStone sharedStone = null;
			if (heldItem != null && heldItem is SharedStone)
			{
				sharedStone = heldItem as SharedStone;
			}
			if (sharedStone != null && sharedStone != _heldStone && _remoteCameraPlatform.GetSocketedStone() == null)
			{
				if (_streamingGroup != null)
				{
					UpdatePreloadingState(shouldBeLoadingRequiredAssets: false, shouldBeLoadingGeneralAssets: false);
				}
				_streamingGroup = StreamingGroup.GetStreamingGroup(NomaiRemoteCameraPlatformIDToSceneName(sharedStone.GetRemoteCameraID()));
			}
			_heldStone = sharedStone;
		}
		if (_streamingGroup != null)
		{
			bool shouldBeLoadingRequiredAssets = _playerInVolume && (_heldStone != null || _remoteCameraPlatform.GetSocketedStone() != null);
			bool shouldBeLoadingGeneralAssets = _playerInVolume && _remoteCameraPlatform.IsPlatformActive();
			UpdatePreloadingState(shouldBeLoadingRequiredAssets, shouldBeLoadingGeneralAssets);
		}
	}

	private void UpdatePreloadingState(bool shouldBeLoadingRequiredAssets, bool shouldBeLoadingGeneralAssets)
	{
		if (!_preloadingRequiredAssets && shouldBeLoadingRequiredAssets)
		{
			_streamingGroup.RequestRequiredAssets();
			_preloadingRequiredAssets = true;
		}
		else if (_preloadingRequiredAssets && !shouldBeLoadingRequiredAssets)
		{
			_streamingGroup.ReleaseRequiredAssets();
			_preloadingRequiredAssets = false;
		}
		if (!_preloadingGeneralAssets && shouldBeLoadingGeneralAssets)
		{
			_streamingGroup.RequestGeneralAssets();
			_preloadingGeneralAssets = true;
		}
		else if (_preloadingGeneralAssets && !shouldBeLoadingGeneralAssets)
		{
			_streamingGroup.ReleaseGeneralAssets();
			_preloadingGeneralAssets = false;
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
				UpdatePreloadingState(shouldBeLoadingRequiredAssets: false, shouldBeLoadingGeneralAssets: false);
			}
			_streamingGroup = null;
			_heldStone = null;
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

	public static string NomaiRemoteCameraPlatformIDToSceneName(NomaiRemoteCameraPlatform.ID id)
	{
		switch (id)
		{
		case NomaiRemoteCameraPlatform.ID.SunStation:
			return "SolarSystem";
		case NomaiRemoteCameraPlatform.ID.HGT_TimeLoop:
		case NomaiRemoteCameraPlatform.ID.HGT_TLE:
			return "HourglassTwins";
		case NomaiRemoteCameraPlatform.ID.THM_EyeLocator:
		case NomaiRemoteCameraPlatform.ID.TH_Mine:
			return "TimberHearth";
		case NomaiRemoteCameraPlatform.ID.BH_Observatory:
		case NomaiRemoteCameraPlatform.ID.BH_GravityCannon:
		case NomaiRemoteCameraPlatform.ID.BH_QuantumFragment:
		case NomaiRemoteCameraPlatform.ID.BH_BlackHoleForge:
		case NomaiRemoteCameraPlatform.ID.VM_Interior:
			return "BrittleHollow";
		case NomaiRemoteCameraPlatform.ID.GD_ConstructionYardIsland1:
		case NomaiRemoteCameraPlatform.ID.GD_ConstructionYardIsland2:
		case NomaiRemoteCameraPlatform.ID.GD_StatueIsland:
		case NomaiRemoteCameraPlatform.ID.GD_ProbeCannonSunkenModule:
		case NomaiRemoteCameraPlatform.ID.GD_ProbeCannonDamagedModule:
		case NomaiRemoteCameraPlatform.ID.GD_ProbeCannonIntactModule:
			return "GiantsDeep";
		default:
			return "";
		}
	}
}
