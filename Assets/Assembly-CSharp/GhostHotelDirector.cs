using UnityEngine;

public class GhostHotelDirector : GhostDirector
{
	[Space]
	[SerializeField]
	private DreamObjectProjector _hotelProjector;

	[SerializeField]
	private OWAudioSource _ghostHowlAudioSource;

	[SerializeField]
	private OWTriggerVolume _depthsExtensionTrigger;

	[Space]
	[SerializeField]
	private GhostBrain[] _hotelDepthsGhosts = new GhostBrain[0];

	[Header("Theater Ghost")]
	[SerializeField]
	private GhostEffects _theaterGhostEffects;

	[SerializeField]
	private LightSensor _theaterScreenSensor;

	[SerializeField]
	private AutoSlideProjector _slideProjector;

	[SerializeField]
	private GameObject _raycastBlocker;

	[Header("Cafe Ghost")]
	[SerializeField]
	private GhostBrain _cafeGhost;

	[SerializeField]
	private GhostNode.NodeLayer _cafeHiddenNodeLayer = GhostNode.NodeLayer.Purple;

	[SerializeField]
	private GhostNode.NodeLayer _cafeEntranceNodeLayer = GhostNode.NodeLayer.Green;

	[SerializeField]
	private GhostNode.NodeLayer _cafeInteriorNodeLayer = GhostNode.NodeLayer.Orange;

	[SerializeField]
	private OWTriggerVolume _depthsVolume;

	[SerializeField]
	private DreamObjectProjector _bridgeProjector;

	private bool _hotelProjectorExtinguished;

	private bool _ghostsAlerted;

	private float _ghostAlertTime;

	private bool _playerIdentifiedInDepths;

	private float _screenIlluminationMeter;

	private bool _turnOffProjectorAfterDelay;

	private float _turnOffProjectorTime;

	protected override void Awake()
	{
		base.Awake();
		_hotelProjector.OnProjectorExtinguished += new OWEvent.OWCallback(OnHotelProjectorExtinguished);
		_bridgeProjector.OnProjectorLit += new OWEvent.OWCallback(OnBridgeProjectorLit);
		_depthsVolume.OnEntry += OnEnterDepths;
		_depthsVolume.OnExit += OnExitDepths;
		for (int i = 0; i < _hotelDepthsGhosts.Length; i++)
		{
			_hotelDepthsGhosts[i].OnIdentifyIntruder += new OWEvent<GhostBrain, GhostData>.OWCallback(OnHotelDepthsGhostsIdentifiedIntruder);
		}
	}

	protected override void Start()
	{
		base.Start();
		_depthsExtensionTrigger.SetTriggerActivation(active: false);
		if (!_hotelProjector.isLit)
		{
			OnHotelProjectorExtinguished();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_hotelProjector.OnProjectorExtinguished -= new OWEvent.OWCallback(OnHotelProjectorExtinguished);
		_bridgeProjector.OnProjectorLit -= new OWEvent.OWCallback(OnBridgeProjectorLit);
		_depthsVolume.OnEntry -= OnEnterDepths;
		_depthsVolume.OnExit -= OnExitDepths;
		for (int i = 0; i < _hotelDepthsGhosts.Length; i++)
		{
			_hotelDepthsGhosts[i].OnIdentifyIntruder -= new OWEvent<GhostBrain, GhostData>.OWCallback(OnHotelDepthsGhostsIdentifiedIntruder);
		}
	}

	private void Update()
	{
		if (_ghostsAreAwake && !_ghostsAlerted && Time.time >= _ghostAlertTime)
		{
			_ghostHowlAudioSource.PlayOneShot(AudioType.Ghost_SomeoneIsInHereHowl);
			_ghostsAlerted = true;
		}
		if (_hotelProjectorExtinguished)
		{
			return;
		}
		if (_slideProjector.IsPlaying() && !_turnOffProjectorAfterDelay && _theaterScreenSensor.IsIlluminated())
		{
			_screenIlluminationMeter += Time.deltaTime;
			if (_screenIlluminationMeter > 2f)
			{
				_turnOffProjectorAfterDelay = true;
				_turnOffProjectorTime = Time.time + 1f;
				_theaterGhostEffects.PlayVoiceAudioNear(AudioType.Ghost_Stalk);
			}
		}
		else if (_turnOffProjectorAfterDelay && Time.time >= _turnOffProjectorTime)
		{
			_turnOffProjectorAfterDelay = false;
			_slideProjector.TurnOff();
		}
	}

	private void OnHotelProjectorExtinguished()
	{
		_hotelProjectorExtinguished = true;
		_depthsExtensionTrigger.SetTriggerActivation(active: true);
		_raycastBlocker.SetActive(value: false);
		WakeGhosts();
		for (int i = 0; i < _directedGhosts.Length; i++)
		{
			_directedGhosts[i].EscalateThreatAwareness(GhostData.ThreatAwareness.SomeoneIsInHere);
		}
		for (int j = 0; j < _hotelDepthsGhosts.Length; j++)
		{
			_hotelDepthsGhosts[j].EscalateThreatAwareness(GhostData.ThreatAwareness.IntruderConfirmed);
		}
		_ghostAlertTime = Time.time + 3f;
	}

	private void OnHotelDepthsGhostsIdentifiedIntruder(GhostBrain ghostBrain, GhostData ghostData)
	{
		if (_playerIdentifiedInDepths)
		{
			return;
		}
		float num = Random.Range(2f, 3f);
		for (int i = 0; i < _hotelDepthsGhosts.Length; i++)
		{
			if (!(_hotelDepthsGhosts[i] == ghostBrain) && _hotelDepthsGhosts[i].HearGhostCall(ghostData.playerLocation.localPosition, num))
			{
				num += Random.Range(2f, 3f);
			}
		}
	}

	private void OnBridgeProjectorLit()
	{
		if (_hotelProjectorExtinguished)
		{
			_cafeGhost.nodeLayer = _cafeEntranceNodeLayer;
		}
	}

	private void OnEnterDepths(GameObject hitObj)
	{
		if (!hitObj.CompareTag("PlayerDetector"))
		{
			return;
		}
		if (_hotelProjectorExtinguished)
		{
			_cafeGhost.nodeLayer = _cafeInteriorNodeLayer;
			return;
		}
		_screenIlluminationMeter = 0f;
		_turnOffProjectorAfterDelay = false;
		if (!_slideProjector.IsPlaying())
		{
			_slideProjector.Play(reset: true);
		}
	}

	private void OnExitDepths(GameObject hitObj)
	{
		if (_hotelProjectorExtinguished && hitObj.CompareTag("PlayerDetector"))
		{
			_cafeGhost.nodeLayer = _cafeHiddenNodeLayer;
		}
	}
}
