using UnityEngine;

public class DreamObjectProjector : MonoBehaviour
{
	[SerializeField]
	protected bool _startLit;

	[SerializeField]
	protected bool _extinguishOnly;

	[SerializeField]
	protected DreamObjectProjection[] _projections = new DreamObjectProjection[0];

	[SerializeField]
	protected DreamObjectProjector[] _extinguishedProjectors = new DreamObjectProjector[0];

	[SerializeField]
	protected AudioVolume[] _lightsOutAudioVolumes = new AudioVolume[0];

	[SerializeField]
	protected DreamCandle[] _dreamCandles = new DreamCandle[0];

	[Space]
	[SerializeField]
	protected LightSensor _lightSensor;

	[SerializeField]
	protected InteractReceiver _interactReceiver;

	[SerializeField]
	protected OWTriggerVolume _triggerVolume;

	[SerializeField]
	protected OWFlameController _flameController;

	[SerializeField]
	protected AlarmTotem _alarmTotem;

	[Space]
	[Header("Audio Sources")]
	[SerializeField]
	private OWAudioSource _farOneShotSource;

	[SerializeField]
	private OWAudioSource _closeOneShotSource;

	protected bool _lit;

	protected bool _wasSensorIlluminated;

	protected float _litTime;

	protected float _unlitTime;

	public OWEvent OnProjectorLit;

	public OWEvent OnProjectorExtinguished;

	public bool isLit => _lit;

	protected virtual void Awake()
	{
		_lit = _startLit;
		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;
		_interactReceiver.OnPressInteract += OnPressInteract;
	}

	protected virtual void Start()
	{
		base.enabled = false;
		for (int i = 0; i < _projections.Length; i++)
		{
			_projections[i].SetVisibleImmediate(_lit, forceUpdate: true);
		}
		for (int j = 0; j < _lightsOutAudioVolumes.Length; j++)
		{
			_lightsOutAudioVolumes[j].SetVolumeActivation(!_lit);
		}
		if (!_lit)
		{
			for (int k = 0; k < _extinguishedProjectors.Length; k++)
			{
				_extinguishedProjectors[k].SetLit(_lit);
			}
		}
		if (_alarmTotem != null)
		{
			_alarmTotem.SetFaceOpen(_lit);
		}
		_flameController.SetIntensity(_lit ? 1f : 0f);
		_interactReceiver.DisableInteraction();
	}

	protected virtual void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEntry;
		_triggerVolume.OnExit -= OnExit;
		_interactReceiver.OnPressInteract -= OnPressInteract;
	}

	public virtual void SetLit(bool lit)
	{
		if (_lit == lit)
		{
			return;
		}
		if (_extinguishOnly && lit)
		{
			for (int i = 0; i < _projections.Length; i++)
			{
				_projections[i].PulseOnAndOff();
			}
			_farOneShotSource.PlayOneShot(AudioType.ProjectorTotem_Pulse);
			return;
		}
		_lit = lit;
		if (_alarmTotem != null)
		{
			_alarmTotem.SetFaceOpen(_lit);
		}
		_flameController.FadeTo(_lit ? 1f : 0f, 1f);
		for (int j = 0; j < _projections.Length; j++)
		{
			_projections[j].SetVisible(_lit);
		}
		for (int k = 0; k < _lightsOutAudioVolumes.Length; k++)
		{
			_lightsOutAudioVolumes[k].SetVolumeActivation(!_lit);
		}
		for (int l = 0; l < _dreamCandles.Length; l++)
		{
			_dreamCandles[l].SetLit(_lit, playAudio: false);
		}
		if (_lit)
		{
			_litTime = Time.time;
			OnProjectorLit.Invoke();
			_farOneShotSource.PlayOneShot(AudioType.ProjectorTotem_Light);
			return;
		}
		for (int m = 0; m < _extinguishedProjectors.Length; m++)
		{
			_extinguishedProjectors[m].SetLit(lit: false);
		}
		_unlitTime = Time.time;
		_interactReceiver.DisableInteraction();
		OnProjectorExtinguished.Invoke();
		_closeOneShotSource.PlayOneShot(AudioType.ProjectorTotem_Blow);
		_farOneShotSource.PlayOneShot(AudioType.ProjectorTotem_Extinguish);
	}

	protected virtual void FixedUpdate()
	{
		bool flag = _lightSensor.IsIlluminated();
		if (!_lit && flag && !_wasSensorIlluminated)
		{
			SetLit(lit: true);
		}
		_wasSensorIlluminated = flag;
	}

	protected virtual void Update()
	{
		_interactReceiver.SetInteractionEnabled(_lit && Time.time > _litTime + 1f && !_lightSensor.IsIlluminated());
	}

	private void OnPressInteract()
	{
		SetLit(lit: false);
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = false;
			_interactReceiver.DisableInteraction();
		}
	}
}
