using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Signalscope : PlayerTool
{
	[FormerlySerializedAs("_zoomModeUI")]
	[SerializeField]
	private SignalscopeUI _signalscopeUI;

	[SerializeField]
	private SignalscopeAudioController _signalscopeAudio;

	[SerializeField]
	private GameObject _scopeGameObject;

	public const int MIN_FREQ_INDEX = 1;

	public const int MAX_FREQ_INDEX = 6;

	public const float MIN_FOV = 10f;

	public const float MAX_FOV = 60f;

	public const float ZOOM_IN_SECONDS = 0.5f;

	private Transform _transform;

	private OWCamera _camera;

	private PlayerCameraController _playerCamController;

	private float _targetFOV;

	private float _targetWarpFOV;

	private float _fovWarpOffset;

	private float _minZoomFactor;

	private bool _inZoomMode;

	private bool _useWarpFov;

	private int _frequencyFilterIndex = 1;

	private AudioSignal[] _strongestSignals = new AudioSignal[7];

	private bool _isUnknownFreqNearby;

	private SignalFrequency _unknownFrequency;

	private bool _isolatingTrack;

	private void Awake()
	{
		_transform = base.transform;
		_playerCamController = GetComponentInParent<PlayerCameraController>();
	}

	protected override void Start()
	{
		base.Start();
		_targetFOV = 10f;
		_camera = Locator.GetPlayerCamera();
		_minZoomFactor = Mathf.Tan(_camera.fieldOfView * ((float)Math.PI / 180f) * 0.5f) * 2f;
	}

	private void OnEnable()
	{
		if (!PlayerState.AtFlightConsole())
		{
			_scopeGameObject.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		_scopeGameObject.SetActive(value: false);
	}

	public bool InZoomMode()
	{
		return _inZoomMode;
	}

	public float GetStrongestSignalStrength(int freqIndex)
	{
		if (!(_strongestSignals[freqIndex] == null))
		{
			return _strongestSignals[freqIndex].GetSignalStrength();
		}
		return 0f;
	}

	public AudioSignal GetStrongestSignal()
	{
		return _strongestSignals[_frequencyFilterIndex];
	}

	public void SelectFrequency(SignalFrequency frequency)
	{
		_frequencyFilterIndex = AudioSignal.FrequencyToIndex(frequency);
	}

	public bool AllowChangeFrequency()
	{
		if (PlayerData.KnowsMultipleFrequencies() || _isUnknownFreqNearby)
		{
			return !PlayerState.IsInsideTheEye();
		}
		return false;
	}

	public void SetTargetWarpFov(float warpFov)
	{
		_targetWarpFOV = warpFov;
		_useWarpFov = true;
	}

	public void ClearTargetWarpFov()
	{
		_targetWarpFOV = 0f;
		_useWarpFov = false;
	}

	public override void EquipTool()
	{
		base.EquipTool();
		_signalscopeAudio.PlaySignalscopeStatic();
		if (!PlayerState.AtFlightConsole())
		{
			Locator.GetPlayerAudioController().PlayEquipTool();
		}
		GlobalMessenger<Signalscope>.FireEvent("EquipSignalscope", this);
	}

	public override void UnequipTool()
	{
		base.UnequipTool();
		if (_inZoomMode)
		{
			ExitSignalscopeZoom();
		}
		_signalscopeAudio.StopSignalscopeStatic();
		if (!PlayerState.AtFlightConsole())
		{
			Locator.GetPlayerAudioController().PlayUnequipTool();
		}
		if (_isolatingTrack)
		{
			Locator.GetAudioMixer().UnmixSignal();
			_isolatingTrack = false;
		}
		GlobalMessenger.FireEvent("UnequipSignalscope");
	}

	public SignalFrequency GetFrequencyFilter()
	{
		return AudioSignal.IndexToFrequency(_frequencyFilterIndex);
	}

	public float GetMagnification()
	{
		return _minZoomFactor / (Mathf.Tan(_camera.fieldOfView * ((float)Math.PI / 180f) * 0.5f) * 2f);
	}

	public float GetZoomFraction()
	{
		return (60f - _camera.fieldOfView) / 50f;
	}

	public Vector3 GetScopeDirection()
	{
		if (!PlayerState.AtFlightConsole())
		{
			return base.transform.forward;
		}
		return Locator.GetShipTransform().forward;
	}

	public void OnEnterSignalDetectionTrigger(AudioSignal signal)
	{
		if (!PlayerData.KnowsFrequency(signal.GetFrequency()))
		{
			_isUnknownFreqNearby = true;
			_unknownFrequency = signal.GetFrequency();
		}
	}

	public void OnExitSignalDetectionTrigger(AudioSignal signal)
	{
		if (_isUnknownFreqNearby && !PlayerData.KnowsFrequency(_unknownFrequency))
		{
			_isUnknownFreqNearby = false;
			SwitchFrequencyFilter(-1);
		}
		_isUnknownFreqNearby = false;
	}

	private void EnterSignalscopeZoom()
	{
		_inZoomMode = true;
		_playerCamController.SnapToFieldOfView(_targetFOV, 0.5f, smoothStep: true);
		_signalscopeAudio.OnZoomIn(_playerCamController);
		_scopeGameObject.SetActive(value: false);
		GlobalMessenger<Signalscope>.FireEvent("EnterSignalscopeZoom", this);
	}

	private void ExitSignalscopeZoom()
	{
		_inZoomMode = false;
		_fovWarpOffset = 0f;
		_useWarpFov = false;
		_signalscopeAudio.OnZoomOut();
		_playerCamController.SnapToInitFieldOfView(0.5f, smoothStep: true);
		_scopeGameObject.SetActive(value: true);
		GlobalMessenger.FireEvent("ExitSignalscopeZoom");
	}

	protected override void Update()
	{
		base.Update();
		if (!_isEquipped || _isPuttingAway)
		{
			return;
		}
		UpdateAudioSignals();
		if (Locator.GetPlayerSuit().IsWearingHelmet() && !_inZoomMode && _signalscopeUI.IsWaveformActive())
		{
			_signalscopeUI.SetWaveformUIActive(value: false);
		}
		else if ((_inZoomMode || (!Locator.GetPlayerSuit().IsWearingHelmet() && !PlayerState.AtFlightConsole())) && !_signalscopeUI.IsWaveformActive())
		{
			_signalscopeUI.SetWaveformUIActive(value: true);
		}
		if (!_isPuttingAway && !_inZoomMode && OWInput.IsInputMode(InputMode.Character))
		{
			if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary))
			{
				EnterSignalscopeZoom();
			}
		}
		else if (_inZoomMode)
		{
			float num = OWInput.GetValue(InputLibrary.toolOptionDown, InputMode.ScopeZoom) - OWInput.GetValue(InputLibrary.toolOptionUp, InputMode.ScopeZoom);
			float target = (_useWarpFov ? (_targetWarpFOV - _targetFOV) : 0f);
			_fovWarpOffset = Mathf.MoveTowards(_fovWarpOffset, target, Time.deltaTime * 30f);
			_targetFOV = Mathf.Clamp(_targetFOV + num * Time.deltaTime * 50f, 10f, 60f);
			_playerCamController.SetTargetFieldOfView(_targetFOV + _fovWarpOffset);
			if (OWInput.IsNewlyReleased(InputLibrary.toolActionPrimary, InputMode.ScopeZoom))
			{
				ExitSignalscopeZoom();
			}
		}
		int frequencyFilterIndex = _frequencyFilterIndex;
		if (!PlayerState.IsInsideTheEye() && OWInput.IsInputMode(InputMode.Character | InputMode.ScopeZoom | InputMode.ShipCockpit))
		{
			if (OWInput.IsNewlyPressed(InputLibrary.toolOptionRight))
			{
				SwitchFrequencyFilter();
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.toolOptionLeft))
			{
				SwitchFrequencyFilter(-1);
			}
		}
		if (_frequencyFilterIndex != frequencyFilterIndex)
		{
			_signalscopeAudio.PlayChangeSignalscopeFrequency();
		}
	}

	private void SwitchFrequencyFilter(int increment = 1)
	{
		_frequencyFilterIndex += increment;
		_frequencyFilterIndex = ((_frequencyFilterIndex > 6) ? 1 : _frequencyFilterIndex);
		_frequencyFilterIndex = ((_frequencyFilterIndex < 1) ? 6 : _frequencyFilterIndex);
		SignalFrequency signalFrequency = AudioSignal.IndexToFrequency(_frequencyFilterIndex);
		if (_frequencyFilterIndex != 0 && !PlayerData.KnowsFrequency(signalFrequency) && (!_isUnknownFreqNearby || _unknownFrequency != signalFrequency))
		{
			SwitchFrequencyFilter(increment);
		}
	}

	private void UpdateAudioSignals()
	{
		float distToClosestScopeObstruction = float.PositiveInfinity;
		for (int i = 0; i < _strongestSignals.Length; i++)
		{
			_strongestSignals[i] = null;
		}
		bool[] array = new bool[5];
		List<AudioSignal> audioSignals = Locator.GetAudioSignals();
		for (int j = 0; j < audioSignals.Count; j++)
		{
			audioSignals[j].UpdateSignalStrength(this, distToClosestScopeObstruction);
			int num = AudioSignal.FrequencyToIndex(audioSignals[j].GetFrequency());
			if (audioSignals[j].GetFrequency() == SignalFrequency.Traveler && audioSignals[j].GetSignalStrength() > 0.7f)
			{
				int num2 = (int)(audioSignals[j].GetName() - 10);
				if (num2 >= 0 && num2 < array.Length)
				{
					array[num2] = true;
				}
			}
			if (_strongestSignals[num] == null || audioSignals[j].GetSignalStrength() > _strongestSignals[num].GetSignalStrength())
			{
				_strongestSignals[num] = audioSignals[j];
			}
		}
		bool flag = true;
		for (int k = 0; k < array.Length; k++)
		{
			if (!array[k])
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			Achievements.Earn(Achievements.Type.HARMONIC_CONVERGENCE);
		}
		if (GetStrongestSignal() != null)
		{
			float s_distanceTextThreshold = SignalscopeUI.s_distanceTextThreshold;
			if (!_isolatingTrack && GetStrongestSignal().GetSignalStrength() > s_distanceTextThreshold)
			{
				Locator.GetAudioMixer().MixSignal();
				_isolatingTrack = true;
			}
			else if (_isolatingTrack && GetStrongestSignal().GetSignalStrength() <= s_distanceTextThreshold)
			{
				Locator.GetAudioMixer().UnmixSignal();
				_isolatingTrack = false;
			}
		}
	}

	private float GetDistanceToClosestObstruction()
	{
		if (Physics.Raycast(_transform.position, _transform.forward, out var hitInfo, 1000f, OWLayerMask.physicalMask))
		{
			return hitInfo.distance;
		}
		return float.PositiveInfinity;
	}
}
