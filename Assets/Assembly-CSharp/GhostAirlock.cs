using UnityEngine;

public class GhostAirlock : MonoBehaviour
{
	[SerializeField]
	private AirlockInterface _interface;

	[SerializeField]
	private bool _startOpen;

	[SerializeField]
	private RotatingDoor _innerDoor;

	[SerializeField]
	private RotatingDoor _outerDoor;

	[SerializeField]
	private OWTriggerVolume _atmosphereVolume;

	[SerializeField]
	private OWTriggerVolume _airlockVolume;

	[SerializeField]
	private OWTriggerVolume _interiorSectorVolume;

	[Header("Audio")]
	[SerializeField]
	private AudioLoopCrossfader _lightSensorCrossfader;

	[SerializeField]
	private OWAudioSource _loopingAudio;

	[SerializeField]
	private OWAudioSource _innerOneShotAudio;

	[SerializeField]
	private OWAudioSource _outerOneShotAudio;

	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	private bool _bothDoorsClosed;

	private bool _pressurized;

	private bool _locked;

	private void Start()
	{
		SetOpenImmediate(_startOpen);
		if (_interface != null)
		{
			_interface.OnOpen += OnOpen;
			_interface.OnClose += OnClose;
			_interface.OnRotate += OnRotate;
			_interface.OnFirstSensorLight += new OWEvent.OWCallback(OnFirstSensorLight);
			_interface.OnAllSensorsDark += new OWEvent.OWCallback(OnAllSensorsDark);
		}
		_innerDoor.OnCloseFinish += new OWEvent.OWCallback(OnInnerDoorFinishClosing);
		_outerDoor.OnCloseFinish += new OWEvent.OWCallback(OnOuterDoorFinishClosing);
		if (_loopingAudio != null)
		{
			_loopingAudio.SetLocalVolume(0f);
		}
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFlood);
		}
		base.enabled = false;
		_locked = false;
	}

	private void OnDestroy()
	{
		if (_interface != null)
		{
			_interface.OnOpen -= OnOpen;
			_interface.OnClose -= OnClose;
			_interface.OnRotate -= OnRotate;
			_interface.OnFirstSensorLight -= new OWEvent.OWCallback(OnFirstSensorLight);
			_interface.OnAllSensorsDark -= new OWEvent.OWCallback(OnAllSensorsDark);
		}
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFlood);
		}
		_innerDoor.OnCloseFinish -= new OWEvent.OWCallback(OnInnerDoorFinishClosing);
		_outerDoor.OnCloseFinish -= new OWEvent.OWCallback(OnOuterDoorFinishClosing);
	}

	private void SetOpenImmediate(bool open)
	{
		if (_interface != null)
		{
			_interface.SetStartingPosition(_startOpen);
		}
		_atmosphereVolume.SetTriggerActivation(open);
		_pressurized = open;
		_innerDoor.SetOpenImmediate(_startOpen);
		_outerDoor.SetOpenImmediate(!_startOpen);
	}

	private void OnOpen()
	{
		if (!_locked)
		{
			if (_innerOneShotAudio != null)
			{
				_innerOneShotAudio.PlayOneShot(AudioType.Airlock_Open);
			}
			SetPressurization(pressurized: true);
			_bothDoorsClosed = false;
			_innerDoor.Open();
		}
	}

	private void OnClose()
	{
		if (_outerOneShotAudio != null)
		{
			_outerOneShotAudio.PlayOneShot(AudioType.Airlock_Open);
		}
		_bothDoorsClosed = false;
		_outerDoor.Open();
		for (int i = 0; i < _airlockVolume.getTrackedObjects().Count; i++)
		{
			_interiorSectorVolume.RemoveObjectFromVolume(_airlockVolume.getTrackedObjects()[i]);
		}
	}

	private void OnRotate()
	{
		if (_loopingAudio != null)
		{
			base.enabled = true;
		}
		if (!_bothDoorsClosed)
		{
			_bothDoorsClosed = true;
			if (_innerDoor.IsOpen() && _innerOneShotAudio != null)
			{
				_innerOneShotAudio.PlayOneShot(AudioType.Airlock_Close);
			}
			if (_outerDoor.IsOpen() && _outerOneShotAudio != null)
			{
				_outerOneShotAudio.PlayOneShot(AudioType.Airlock_Close);
			}
			_innerDoor.Close();
			_outerDoor.Close();
		}
	}

	private void OnFlood()
	{
		_interface.OnFloodDeactivate();
		_locked = true;
	}

	private void OnInnerDoorFinishClosing()
	{
		SetPressurization(pressurized: false);
	}

	private void OnOuterDoorFinishClosing()
	{
		for (int i = 0; i < _airlockVolume.getTrackedObjects().Count; i++)
		{
			_interiorSectorVolume.AddObjectToVolume(_airlockVolume.getTrackedObjects()[i]);
		}
	}

	private void SetPressurization(bool pressurized)
	{
		if (_pressurized != pressurized)
		{
			_pressurized = pressurized;
			_innerOneShotAudio.PlayOneShot(pressurized ? AudioType.Airlock_Pressurize : AudioType.Airlock_Depressurize);
			_atmosphereVolume.SetTriggerActivation(pressurized);
		}
	}

	private void Update()
	{
		float speedFraction = _interface.GetSpeedFraction();
		if (!_loopingAudio.isPlaying && speedFraction > 0f)
		{
			_loopingAudio.Play();
		}
		float localVolume = _loopingAudio.GetLocalVolume();
		localVolume = Mathf.MoveTowards(localVolume, speedFraction, 3f * Time.deltaTime);
		_loopingAudio.SetLocalVolume(localVolume);
		if (speedFraction <= 0f && localVolume <= 0f)
		{
			_loopingAudio.Stop();
			base.enabled = false;
		}
	}

	private void OnFirstSensorLight()
	{
		if (_lightSensorCrossfader != null)
		{
			_lightSensorCrossfader.Play();
		}
	}

	private void OnAllSensorsDark()
	{
		if (_lightSensorCrossfader != null)
		{
			_lightSensorCrossfader.Stop();
		}
	}
}
