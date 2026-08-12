using UnityEngine;

public class NomaiAirlock : NomaiMultiPartDoor
{
	public delegate void AirlockEvent();

	[Space(10f)]
	[SerializeField]
	private OWTriggerVolume _oxygenVolume;

	[SerializeField]
	private GameObject _underwaterStencil;

	[SerializeField]
	private bool _startOxygenated;

	[SerializeField]
	private OWAudioSource _airSFXAudioSource;

	[SerializeField]
	[Range(0f, 1f)]
	private float _oxygenationPoint;

	[Space]
	[SerializeField]
	private AudioType _airPourInSound = AudioType.NomaiAirLockAirPourIn;

	[SerializeField]
	private AudioType _airPourOutSound = AudioType.NomaiAirLockAirPourOut;

	protected bool _currentOxygenState;

	protected bool _nextOxygenState;

	public event AirlockEvent OnAirPourIn;

	public event AirlockEvent OnAirPourOut;

	protected override void Start()
	{
		if (_oxygenVolume == null)
		{
			Debug.LogError("Airlock has no oxygen volume!", this);
		}
		else if (_oxygenVolume.gameObject.activeInHierarchy)
		{
			_oxygenVolume.SetTriggerActivation(_startOxygenated);
		}
		_currentOxygenState = _startOxygenated;
		_nextOxygenState = _startOxygenated;
		base.Start();
	}

	public void ResetToOpenState()
	{
		if (_currentRotationState != 0)
		{
			Open(null);
			_listInterfaceOrb[0].SetOrbPosition(_openSwitches[0].transform.position);
		}
	}

	protected override void PlayMovementStartAudio()
	{
		if (_audioSource != null)
		{
			_audioSource.PlayOneShot(AudioType.NomaiDoorAirLockOpen);
		}
	}

	protected override void PlayMovementStopAudio()
	{
		if (_audioSource != null)
		{
			_audioSource.PlayOneShot(AudioType.NomaiDoorStopBig);
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_nextOxygenState == _currentOxygenState || !(_cycleTimer / _totalCycleTime >= _oxygenationPoint))
		{
			return;
		}
		if (_oxygenVolume != null && _oxygenVolume.gameObject.activeInHierarchy)
		{
			_oxygenVolume.SetTriggerActivation(_nextOxygenState);
		}
		_currentOxygenState = _nextOxygenState;
		if (_underwaterStencil != null)
		{
			_underwaterStencil.SetActive(_nextOxygenState);
		}
		if (!_oxygenVolume.gameObject.activeInHierarchy)
		{
			return;
		}
		if (_currentOxygenState)
		{
			_airSFXAudioSource.PlayOneShot(_airPourInSound);
			if (this.OnAirPourIn != null)
			{
				this.OnAirPourIn();
			}
		}
		else
		{
			_airSFXAudioSource.PlayOneShot(_airPourOutSound);
			if (this.OnAirPourOut != null)
			{
				this.OnAirPourOut();
			}
		}
	}

	public override void Cycle(NomaiInterfaceSlot slot)
	{
		base.Cycle(slot);
		_nextOxygenState = !_currentOxygenState;
	}
}
