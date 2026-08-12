using UnityEngine;

public class Flashlight : MonoBehaviour, ILightSource
{
	[SerializeField]
	private OWLight2[] _lights;

	[SerializeField]
	private OWLight2 _illuminationCheckLight;

	[SerializeField]
	private Transform _root;

	[SerializeField]
	private Transform _basePivot;

	[SerializeField]
	private Transform _wobblePivot;

	private bool _flashlightOn;

	private Vector3 _baseForward;

	private Quaternion _baseRotation;

	private LightSourceVolume _lightSourceVolume;

	private void Awake()
	{
		GlobalMessenger.AddListener("EnterShip", TurnOff);
		GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.AddListener("EnterSatelliteCameraMode", TurnOff);
		GlobalMessenger.AddListener("PlayerRepositioned", OnPlayerRepositioned);
	}

	private void Start()
	{
		_lightSourceVolume = this.GetRequiredComponentInChildren<LightSourceVolume>();
		_lightSourceVolume.LinkLightSource(this);
		_lightSourceVolume.SetVolumeActivation(_flashlightOn);
		_baseForward = _basePivot.forward;
		_baseRotation = _basePivot.rotation;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterShip", TurnOff);
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.RemoveListener("EnterSatelliteCameraMode", TurnOff);
		GlobalMessenger.RemoveListener("PlayerRepositioned", OnPlayerRepositioned);
	}

	public LightSourceType GetLightSourceType()
	{
		return LightSourceType.FLASHLIGHT;
	}

	public OWLight2[] GetLights()
	{
		return _lights;
	}

	public bool CheckIlluminationAtPoint(Vector3 point, float buffer = 0f, float maxDistance = float.PositiveInfinity)
	{
		if (!_flashlightOn)
		{
			return false;
		}
		return _illuminationCheckLight.CheckIlluminationAtPoint(point, buffer, maxDistance);
	}

	public bool IsFlashlightOn()
	{
		return _flashlightOn;
	}

	public void TurnOn(bool playAudio = true)
	{
		if (!_flashlightOn)
		{
			for (int i = 0; i < _lights.Length; i++)
			{
				_lights[i].SetActivation(active: true);
			}
			_flashlightOn = true;
			if (playAudio)
			{
				Locator.GetPlayerAudioController().PlayTurnOnFlashlight();
			}
			Quaternion baseRotation = (_basePivot.rotation = _root.rotation);
			_baseRotation = baseRotation;
			_baseForward = _basePivot.forward;
			GlobalMessenger.FireEvent("TurnOnFlashlight");
			_lightSourceVolume.SetVolumeActivation(_flashlightOn);
		}
	}

	public void TurnOff()
	{
		TurnOff(playAudio: true);
	}

	public void TurnOff(bool playAudio)
	{
		if (_flashlightOn)
		{
			for (int i = 0; i < _lights.Length; i++)
			{
				_lights[i].SetActivation(active: false);
			}
			_flashlightOn = false;
			if (playAudio)
			{
				Locator.GetPlayerAudioController().PlayTurnOffFlashlight();
			}
			GlobalMessenger.FireEvent("TurnOffFlashlight");
			_lightSourceVolume.SetVolumeActivation(_flashlightOn);
		}
	}

	private void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		TurnOff();
	}

	private void FixedUpdate()
	{
		Quaternion b = Quaternion.FromToRotation(_basePivot.up, _root.up) * Quaternion.FromToRotation(_baseForward, _root.forward) * _baseRotation;
		_baseRotation = Quaternion.Slerp(_baseRotation, b, 6f * Time.deltaTime);
		_basePivot.rotation = _baseRotation;
		_baseForward = _basePivot.forward;
		if (_wobblePivot != null)
		{
			_wobblePivot.localRotation = OWUtilities.GetWobbleRotation(0.3f, 0.15f) * Quaternion.identity;
		}
	}

	private void Update()
	{
		if (!PlayerState.InDreamWorld() && OWInput.IsNewlyPressed(InputLibrary.flashlight, InputMode.Character | InputMode.Roasting))
		{
			if (!_flashlightOn)
			{
				TurnOn();
			}
			else
			{
				TurnOff();
			}
		}
	}

	private void OnPlayerRepositioned()
	{
		Quaternion baseRotation = (_basePivot.rotation = _root.rotation);
		_baseRotation = baseRotation;
		_baseForward = _basePivot.forward;
	}
}
