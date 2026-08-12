using UnityEngine;

[AddComponentMenu("Streaming/Nomai Warp Streaming", 200)]
[RequireComponent(typeof(OWTriggerVolume))]
public class NomaiWarpStreaming : SectoredMonoBehaviour
{
	private OWTriggerVolume _owTriggerVolume;

	[SerializeField]
	private string _destinationSceneName = "";

	[Space]
	[SerializeField]
	private NomaiWarpTransmitter _warpTransmitter;

	[SerializeField]
	private float _streamingAngle = 15f;

	[Space]
	[SerializeField]
	private NomaiWarpReceiver _warpReceiver;

	private SurveyorProbe _probe;

	private StreamingGroup _streamingGroup;

	private bool _playerInVolume;

	private bool _probeInVolume;

	private bool _preloadingRequiredAssets;

	private bool _preloadingGeneralAssets;

	protected override void Awake()
	{
		base.Awake();
		if (_warpTransmitter != null && _warpReceiver != null)
		{
			Debug.LogError("NomaiWarpStreaming should be set up for either a NomaiWarpTransmitter or a NomaiWarpReceiver, not both!", this);
			Debug.Break();
		}
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
		_probe = Locator.GetProbe();
		_streamingGroup = StreamingGroup.GetStreamingGroup(_destinationSceneName);
		base.enabled = false;
	}

	private void FixedUpdate()
	{
		if (_warpTransmitter != null)
		{
			bool num = _warpTransmitter.GetViewAngleToTarget() < _streamingAngle;
			bool flag = _probe.IsLaunched() && (!_probe.IsAnchored() || _warpTransmitter.IsProbeOnPlatform());
			bool shouldBeLoadingRequiredAssets = num && (_playerInVolume || (_probeInVolume && flag));
			bool shouldBeLoadingGeneralAssets = num && _warpTransmitter.IsPlayerOnPlatform();
			UpdatePreloadingState(shouldBeLoadingRequiredAssets, shouldBeLoadingGeneralAssets);
		}
		if (_warpReceiver != null)
		{
			bool num2 = _warpReceiver.IsReturnWarpEnabled() || _warpReceiver.IsBlackHoleOpen();
			bool flag2 = _probe.IsLaunched() && (!_probe.IsAnchored() || _warpReceiver.IsProbeOnPlatform());
			bool shouldBeLoadingRequiredAssets2 = num2 && (_playerInVolume || (_probeInVolume && flag2));
			bool shouldBeLoadingGeneralAssets2 = num2 && _playerInVolume;
			UpdatePreloadingState(shouldBeLoadingRequiredAssets2, shouldBeLoadingGeneralAssets2);
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

	protected override void OnSectorOccupantsUpdated()
	{
		if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			if (StreamingManager.isStreamingEnabled && _streamingGroup != null)
			{
				base.enabled = true;
			}
		}
		else
		{
			UpdatePreloadingState(shouldBeLoadingRequiredAssets: false, shouldBeLoadingGeneralAssets: false);
			base.enabled = false;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
		if (attachedOWRigidbody != null)
		{
			if (attachedOWRigidbody.CompareTag("Player"))
			{
				_playerInVolume = true;
			}
			else if (attachedOWRigidbody.CompareTag("Probe"))
			{
				_probeInVolume = true;
			}
		}
	}

	private void OnExit(GameObject hitObj)
	{
		OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
		if (attachedOWRigidbody != null)
		{
			if (attachedOWRigidbody.CompareTag("Player"))
			{
				_playerInVolume = false;
			}
			else if (attachedOWRigidbody.CompareTag("Probe"))
			{
				_probeInVolume = false;
			}
		}
	}
}
