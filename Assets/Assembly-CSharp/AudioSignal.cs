using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Audio/Audio Signal", 400)]
[RequireComponent(typeof(OWAudioSource))]
public class AudioSignal : SectoredMonoBehaviour
{
	public const float NON_SIGNAL_TRACK_VOLUME = 0.2f;

	[SerializeField]
	private SignalFrequency _frequency = SignalFrequency.Default;

	[SerializeField]
	private SignalName _name;

	[SerializeField]
	private float _sourceRadius;

	[FormerlySerializedAs("_detectionRadius")]
	[SerializeField]
	private float _identificationDistance = 10f;

	[Space]
	[SerializeField]
	private float _waveformScalar = 1f;

	[SerializeField]
	private bool _startActive = true;

	[SerializeField]
	private bool _onlyAudibleToScope;

	[SerializeField]
	private bool _preventIdentification;

	[SerializeField]
	private OuterFogWarpVolume _outerFogWarpVolume;

	[SerializeField]
	private string _revealFactID = "";

	[Space]
	[SerializeField]
	private bool _debug;

	private OWAudioSource _owAudioSource;

	private SunController _sunController;

	private float _signalStrength;

	private float _signalVolume;

	private float _activeVolume;

	private float _degreesFromScope = 180f;

	private float _distToScope;

	private Vector3 _scopeToSignal = Vector3.zero;

	private bool _canBePickedUpByScope;

	private bool _unequipFading;

	private bool _active;

	private bool _activeFading;

	private float _activeFadeStartTime;

	private float _activeFadeDuration;

	private float _activeFadeStartVolume;

	private bool _showCrazyFarMaskDistance;

	public static string SignalNameToString(SignalName name)
	{
		switch (name)
		{
		case SignalName.EscapePod_BH:
			return UITextLibrary.GetString(UITextType.SignalPod1);
		case SignalName.EscapePod_CT:
			return UITextLibrary.GetString(UITextType.SignalPod2);
		case SignalName.EscapePod_DB:
			return UITextLibrary.GetString(UITextType.SignalPod3);
		case SignalName.HideAndSeek_Arkose:
			return UITextLibrary.GetString(UITextType.SignalArkose);
		case SignalName.HideAndSeek_Galena:
			return UITextLibrary.GetString(UITextType.SignalGalena);
		case SignalName.HideAndSeek_Tephra:
			return UITextLibrary.GetString(UITextType.SignalTephra);
		case SignalName.Quantum_TH_GroveShard:
			return UITextLibrary.GetString(UITextType.SignalQuantumGrove);
		case SignalName.Quantum_TH_MuseumShard:
			return UITextLibrary.GetString(UITextType.SignalQuantumMuseum);
		case SignalName.Quantum_BH_Shard:
			return UITextLibrary.GetString(UITextType.SignalQuantumBH);
		case SignalName.Quantum_CT_Shard:
			return UITextLibrary.GetString(UITextType.SignalQuantumCT);
		case SignalName.Quantum_GD_Shard:
			return UITextLibrary.GetString(UITextType.SignalQuantumGD);
		case SignalName.Quantum_QM:
			return UITextLibrary.GetString(UITextType.SignalQM);
		case SignalName.Traveler_Chert:
			return UITextLibrary.GetString(UITextType.SignalChert);
		case SignalName.Traveler_Esker:
			return UITextLibrary.GetString(UITextType.SignalEsker);
		case SignalName.Traveler_Feldspar:
			return UITextLibrary.GetString(UITextType.SignalFeldspar);
		case SignalName.Traveler_Gabbro:
			return UITextLibrary.GetString(UITextType.SignalGabbro);
		case SignalName.Traveler_Riebeck:
			return UITextLibrary.GetString(UITextType.SignalRiebeck);
		case SignalName.Traveler_Nomai:
			return UITextLibrary.GetString(UITextType.SignalSolanum);
		case SignalName.Traveler_Prisoner:
			return UITextLibrary.GetString(UITextType.SignalPrisoner);
		case SignalName.WhiteHole_WH:
			return UITextLibrary.GetString(UITextType.SignalWH);
		case SignalName.WhiteHole_SS_Receiver:
		case SignalName.WhiteHole_CT_Receiver:
		case SignalName.WhiteHole_CT_Experiment:
		case SignalName.WhiteHole_TT_Receiver:
		case SignalName.WhiteHole_TT_TimeLoopCore:
		case SignalName.WhiteHole_TH_Receiver:
		case SignalName.WhiteHole_BH_NorthPoleReceiver:
		case SignalName.WhiteHole_BH_ForgeReceiver:
		case SignalName.WhiteHole_GD_Receiver:
			return UITextLibrary.GetString(UITextType.SignalWHCore);
		case SignalName.RadioTower:
			return UITextLibrary.GetString(UITextType.SignalRadioTower);
		case SignalName.MapSatellite:
			return UITextLibrary.GetString(UITextType.SignalMapSatellite);
		default:
			return "ERROR";
		}
	}

	public static SignalFrequency IndexToFrequency(int index)
	{
		switch (index)
		{
		case 1:
			return SignalFrequency.Traveler;
		case 2:
			return SignalFrequency.Quantum;
		case 3:
			return SignalFrequency.EscapePod;
		case 4:
			return SignalFrequency.WarpCore;
		case 5:
			return SignalFrequency.HideAndSeek;
		case 6:
			return SignalFrequency.Radio;
		default:
			return SignalFrequency.Default;
		}
	}

	public static int FrequencyToIndex(SignalFrequency frequency)
	{
		switch (frequency)
		{
		case SignalFrequency.Traveler:
			return 1;
		case SignalFrequency.Quantum:
			return 2;
		case SignalFrequency.EscapePod:
			return 3;
		case SignalFrequency.WarpCore:
			return 4;
		case SignalFrequency.HideAndSeek:
			return 5;
		case SignalFrequency.Radio:
			return 6;
		default:
			return 0;
		}
	}

	public static string FrequencyToString(SignalFrequency frequency, bool showIndexNumber = true)
	{
		string text = (showIndexNumber ? (FrequencyToIndex(frequency) + ". ") : "");
		switch (frequency)
		{
		case SignalFrequency.Traveler:
			return text + (PlayerData.KnowsFrequency(frequency) ? UITextLibrary.GetString(UITextType.SignalFreqOWV) : UITextLibrary.GetString(UITextType.SignalFreqUnidentified));
		case SignalFrequency.Quantum:
			return text + (PlayerData.KnowsFrequency(frequency) ? UITextLibrary.GetString(UITextType.SignalFreqQuantum) : UITextLibrary.GetString(UITextType.SignalFreqUnidentified));
		case SignalFrequency.EscapePod:
			return text + (PlayerData.KnowsFrequency(frequency) ? UITextLibrary.GetString(UITextType.SignalFreqPods) : UITextLibrary.GetString(UITextType.SignalFreqUnidentified));
		case SignalFrequency.WarpCore:
			return text + (PlayerData.KnowsFrequency(frequency) ? UITextLibrary.GetString(UITextType.SignalFreqWH) : UITextLibrary.GetString(UITextType.SignalFreqUnidentified));
		case SignalFrequency.HideAndSeek:
			return text + (PlayerData.KnowsFrequency(frequency) ? UITextLibrary.GetString(UITextType.SignalFreqHideNSeek) : UITextLibrary.GetString(UITextType.SignalFreqUnidentified));
		case SignalFrequency.Radio:
			return text + (PlayerData.KnowsFrequency(frequency) ? UITextLibrary.GetString(UITextType.SignalFreqRadio) : UITextLibrary.GetString(UITextType.SignalFreqUnidentified));
		default:
			return text + "DEFAULT";
		}
	}

	private void OnValidate()
	{
		AudioSource component = GetComponent<AudioSource>();
		if (!component.loop)
		{
			component.loop = true;
		}
		if (component.spatialBlend < 1f)
		{
			component.spatialBlend = 1f;
		}
		if (component.dopplerLevel > 0f)
		{
			component.dopplerLevel = 0f;
		}
		OWAudioSource component2 = GetComponent<OWAudioSource>();
		if (component2.GetTrack() != OWAudioMixer.TrackName.Signal)
		{
			component2.SetTrack(OWAudioMixer.TrackName.Signal);
		}
		if ((_onlyAudibleToScope || !_startActive) && component.playOnAwake)
		{
			component.playOnAwake = false;
		}
		if ((_frequency == SignalFrequency.Quantum || _frequency == SignalFrequency.Statue || _frequency == SignalFrequency.WarpCore) && !_onlyAudibleToScope)
		{
			_onlyAudibleToScope = true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (_sector == null)
		{
			Debug.LogWarning("AudioSignal sectors should not be null!!!", this);
		}
		_active = _startActive;
		_activeVolume = (_startActive ? 1f : 0f);
		_owAudioSource = this.GetRequiredComponent<OWAudioSource>();
		if (_owAudioSource.GetTrack() != OWAudioMixer.TrackName.Signal)
		{
			Debug.Log("Audio signal not set to Signal track!", base.gameObject);
			Debug.Break();
		}
		_showCrazyFarMaskDistance = _name == SignalName.Traveler_Nomai && _sourceRadius > 100f;
		Locator.RegisterAudioSignal(this);
		GlobalMessenger<Signalscope>.AddListener("EquipSignalscope", OnEquipSignalscope);
		GlobalMessenger.AddListener("UnequipSignalscope", OnUnequipSignalScope);
	}

	private void Start()
	{
		if (Locator.GetSunTransform() != null)
		{
			_sunController = Locator.GetSunController();
		}
		if (_onlyAudibleToScope)
		{
			_owAudioSource.SetLocalVolume(0f);
		}
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Locator.UnregisterAudioSignal(this);
		GlobalMessenger<Signalscope>.RemoveListener("EquipSignalscope", OnEquipSignalscope);
		GlobalMessenger.RemoveListener("UnequipSignalscope", OnUnequipSignalScope);
	}

	public void CheckAgainstStrongestSignal(AudioSignal strongestSignal)
	{
		if (_signalStrength > 0f && _signalStrength < strongestSignal.GetSignalStrength())
		{
			_signalStrength = 0f;
			_canBePickedUpByScope = false;
		}
	}

	public bool IsActive()
	{
		return _active;
	}

	public SignalName GetName()
	{
		return _name;
	}

	public SignalFrequency GetFrequency()
	{
		return _frequency;
	}

	public float GetSignalStrength()
	{
		return _signalStrength;
	}

	public float GetSignalVolume()
	{
		return Mathf.InverseLerp(0.8f, 1f, _signalStrength);
	}

	public float GetDegreesFromScope()
	{
		return _degreesFromScope;
	}

	public float GetWaveformScalar()
	{
		return _waveformScalar;
	}

	public float GetDistanceFromScope()
	{
		if (!_showCrazyFarMaskDistance)
		{
			return _distToScope;
		}
		return _distToScope + 100000f;
	}

	public Vector3 GetDisplacementFromScope()
	{
		return _scopeToSignal;
	}

	public bool CanBePickedUpByScope()
	{
		return _canBePickedUpByScope;
	}

	public bool IsOnlyAudibleToScope()
	{
		return _onlyAudibleToScope;
	}

	public OWAudioSource GetOWAudioSource()
	{
		return _owAudioSource;
	}

	public bool IsInsideDarkBramble()
	{
		return _outerFogWarpVolume != null;
	}

	public void SetSignalActivation(bool active, float fadeDuration = 2f)
	{
		if (_active == active)
		{
			return;
		}
		_active = active;
		if (fadeDuration > 0f)
		{
			_activeFading = true;
			_activeFadeDuration = fadeDuration;
			_activeFadeStartTime = Time.time;
			_activeFadeStartVolume = _activeVolume;
			base.enabled = true;
		}
		else
		{
			_activeVolume = (_active ? 1f : 0f);
			_owAudioSource.SetLocalVolume(_signalVolume * _activeVolume);
			if (_active && !_owAudioSource.isPlaying)
			{
				_owAudioSource.Play();
			}
		}
	}

	public void UpdateSignalStrength(Signalscope scope, float distToClosestScopeObstruction)
	{
		_canBePickedUpByScope = false;
		if (_sunController != null && _sunController.IsPointInsideSupernova(base.transform.position))
		{
			_signalStrength = 0f;
			_degreesFromScope = 180f;
			return;
		}
		if (Locator.GetQuantumMoon() != null && Locator.GetQuantumMoon().IsPlayerInside() && _name != SignalName.Quantum_QM)
		{
			_signalStrength = 0f;
			_degreesFromScope = 180f;
			return;
		}
		if (!_active || !base.gameObject.activeInHierarchy || _outerFogWarpVolume != PlayerState.GetOuterFogWarpVolume() || (scope.GetFrequencyFilter() & _frequency) != _frequency)
		{
			_signalStrength = 0f;
			_degreesFromScope = 180f;
			return;
		}
		_scopeToSignal = base.transform.position - scope.transform.position;
		_distToScope = _scopeToSignal.magnitude;
		if (_outerFogWarpVolume == null && distToClosestScopeObstruction < 1000f && _distToScope > 1000f)
		{
			_signalStrength = 0f;
			_degreesFromScope = 180f;
			return;
		}
		_canBePickedUpByScope = true;
		if (_distToScope < _sourceRadius)
		{
			_signalStrength = 1f;
		}
		else
		{
			_degreesFromScope = Vector3.Angle(scope.GetScopeDirection(), _scopeToSignal);
			float t = Mathf.InverseLerp(2000f, 1000f, _distToScope);
			float a = Mathf.Lerp(45f, 90f, t);
			float a2 = 57.29578f * Mathf.Atan2(_sourceRadius, _distToScope);
			float b = Mathf.Lerp(Mathf.Max(a2, 5f), Mathf.Max(a2, 1f), scope.GetZoomFraction());
			_signalStrength = Mathf.Clamp01(Mathf.InverseLerp(a, b, _degreesFromScope));
		}
		if (Locator.GetCloakFieldController() != null)
		{
			float num = 1f - Locator.GetCloakFieldController().playerCloakFactor;
			_signalStrength *= num;
			if (OWMath.ApproxEquals(num, 0f))
			{
				_signalStrength = 0f;
				_degreesFromScope = 180f;
				return;
			}
		}
		if (_distToScope < _identificationDistance + _sourceRadius && _signalStrength > 0.9f)
		{
			if (!PlayerData.KnowsFrequency(_frequency) && !_preventIdentification)
			{
				IdentifyFrequency();
			}
			if (!PlayerData.KnowsSignal(_name) && !_preventIdentification)
			{
				IdentifySignal();
			}
			if (_revealFactID.Length > 0)
			{
				Locator.GetShipLogManager().RevealFact(_revealFactID);
			}
		}
	}

	private void IdentifyFrequency()
	{
		PlayerData.LearnFrequency(_frequency);
		string displayMsg = UITextLibrary.GetString(UITextType.NotificationNewFreq) + " <color=orange>" + FrequencyToString(_frequency, showIndexNumber: false) + "</color>";
		NotificationData data = new NotificationData(NotificationTarget.All, displayMsg, 10f);
		NotificationManager.SharedInstance.PostNotification(data);
	}

	private void IdentifySignal()
	{
		PlayerData.LearnSignal(_name);
		GlobalMessenger.FireEvent("IdentifySignal");
		string displayMsg = UITextLibrary.GetString(UITextType.NotificationSignalIdentified) + " <color=orange>" + SignalNameToString(_name) + "</color>";
		NotificationData data = new NotificationData(NotificationTarget.All, displayMsg, 10f);
		NotificationManager.SharedInstance.PostNotification(data);
	}

	private void FixedUpdate()
	{
		bool flag = !_unequipFading && Locator.GetToolModeSwapper().IsInToolMode(ToolMode.SignalScope);
		if (!flag && !_unequipFading && !_activeFading)
		{
			base.enabled = false;
			return;
		}
		if (_onlyAudibleToScope && !_owAudioSource.isPlaying && _active)
		{
			_owAudioSource.SetLocalVolume(0f);
			_owAudioSource.Play();
		}
		float num = ((flag || _onlyAudibleToScope) ? GetSignalVolume() : 1f);
		float num2 = (flag ? (1f - num) : 1f);
		_owAudioSource.spatialBlend = Mathf.MoveTowards(_owAudioSource.spatialBlend, num2, 0.05f);
		_signalVolume = Mathf.MoveTowards(_signalVolume, num, 0.05f);
		if (_debug)
		{
			MonoBehaviour.print("target signal volume: " + num + "   unequip fading: " + _unequipFading.ToString() + "   signalscope: " + flag.ToString());
		}
		if (_activeFading)
		{
			float num3 = Mathf.InverseLerp(_activeFadeStartTime, _activeFadeStartTime + _activeFadeDuration, Time.time);
			float b = (_active ? 1f : 0f);
			_activeVolume = Mathf.Lerp(_activeFadeStartVolume, b, Mathf.SmoothStep(0f, 1f, num3));
			if (num3 >= 1f)
			{
				_activeFading = false;
				if (_onlyAudibleToScope && !_active)
				{
					_owAudioSource.Stop();
				}
			}
		}
		if (_unequipFading && OWMath.ApproxEquals(_owAudioSource.spatialBlend, num2) && OWMath.ApproxEquals(_signalVolume, num))
		{
			_unequipFading = false;
			_signalVolume = num;
			_owAudioSource.spatialBlend = num2;
			if (_onlyAudibleToScope)
			{
				_owAudioSource.Stop();
			}
		}
		_owAudioSource.SetLocalVolume(_signalVolume * _activeVolume);
	}

	private void OnEquipSignalscope(Signalscope scope)
	{
		base.enabled = true;
		_distToScope = float.PositiveInfinity;
	}

	private void OnUnequipSignalScope()
	{
		_canBePickedUpByScope = false;
		_unequipFading = true;
		_signalStrength = 0f;
		_degreesFromScope = 180f;
		_distToScope = float.PositiveInfinity;
		_scopeToSignal = Vector3.zero;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = new ColorHSV(50f, 0.82f, 0.5f).ToColorRGB();
			Gizmos.color = Color.green;
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _sourceRadius + _identificationDistance);
			Gizmos.color = Color.yellow;
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _sourceRadius);
		}
	}
}
