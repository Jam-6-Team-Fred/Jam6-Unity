using UnityEngine;
using UnityEngine.UI;

public class SignalscopeUI : MonoBehaviour
{
	public static float s_distanceTextThreshold = 0.8f;

	private const int SAMPLE_COUNT = 256;

	[SerializeField]
	private Canvas _signalscopeCanvas;

	[SerializeField]
	private LineRenderer _waveformRenderer;

	[SerializeField]
	private Image _leftBoundLine;

	[SerializeField]
	private Image _rightBoundLine;

	[SerializeField]
	private Text _signalscopeLabel;

	[SerializeField]
	private float _amplitude = 1f;

	[SerializeField]
	private bool _showFrequencyLabel = true;

	[SerializeField]
	private Text _distanceLabel;

	[SerializeField]
	private RectTransform _signalZoomArrow;

	[SerializeField]
	private float _signalZoomArrowStartRot = 135f;

	[SerializeField]
	private float _signalZoomArrowEndRot = 45f;

	[SerializeField]
	private bool _isPlayerScope;

	[SerializeField]
	private Canvas _signalZoomGroup;

	private Texture2D _switchFreqTexLeft;

	private Texture2D _switchFreqTexRight;

	private Signalscope _signalscopeTool;

	private SignalscopeReticleController _reticuleController;

	private SignalFrequency _currentFreq;

	private RectTransform _rectTransform;

	private float[] _audioSamples = new float[256];

	private Vector3[] _linePoints = new Vector3[256];

	private bool _enableWaveformUI = true;

	private bool _uiActivated;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		GlobalMessenger<Signalscope>.AddListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.AddListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
	}

	private void OnDestroy()
	{
		GlobalMessenger<Signalscope>.RemoveListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		if (_uiActivated)
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases;
		}
	}

	private void Start()
	{
		if (_signalscopeCanvas != null)
		{
			_signalscopeCanvas.enabled = false;
		}
		_signalscopeLabel.text = string.Empty;
		_signalscopeLabel.enabled = false;
		_distanceLabel.text = string.Empty;
		_distanceLabel.enabled = false;
		_waveformRenderer.enabled = false;
		_leftBoundLine.enabled = false;
		_rightBoundLine.enabled = false;
		if (_signalZoomGroup != null)
		{
			_signalZoomGroup.enabled = false;
		}
		_waveformRenderer.positionCount = 256;
		float x = _rectTransform.sizeDelta.x;
		float num = x * 0.5f - 0.05f * x;
		float x2 = num * -1f;
		float y = _leftBoundLine.GetRequiredComponent<RectTransform>().anchoredPosition.y;
		_rightBoundLine.GetRequiredComponent<RectTransform>().anchoredPosition = new Vector2(num, y);
		_leftBoundLine.GetRequiredComponent<RectTransform>().anchoredPosition = new Vector2(x2, y);
		ActivateUI(value: false);
	}

	public void Activate(Signalscope scope, SignalscopeReticleController reticleCtrl)
	{
		if (_isPlayerScope)
		{
			if (PlayerState.AtFlightConsole())
			{
				return;
			}
		}
		else if (!PlayerState.AtFlightConsole())
		{
			return;
		}
		if (_signalscopeCanvas != null)
		{
			_signalscopeCanvas.enabled = true;
		}
		_reticuleController = reticleCtrl;
		_signalscopeTool = scope;
		_signalscopeLabel.enabled = true;
		_distanceLabel.enabled = true;
		_leftBoundLine.enabled = true;
		_rightBoundLine.enabled = true;
		_currentFreq = _signalscopeTool.GetFrequencyFilter();
		ActivateUI(value: true);
		if (_reticuleController != null)
		{
			_reticuleController.SetUIActive(value: true);
		}
	}

	public void Deactivate()
	{
		if (_signalscopeCanvas != null)
		{
			_signalscopeCanvas.enabled = false;
		}
		_waveformRenderer.enabled = false;
		_signalscopeLabel.enabled = false;
		_distanceLabel.enabled = false;
		_leftBoundLine.enabled = false;
		_rightBoundLine.enabled = false;
		if (_reticuleController != null)
		{
			_reticuleController.SetUIActive(value: false);
		}
		ActivateUI(value: false);
	}

	private void ActivateUI(bool value)
	{
		if (_uiActivated != value)
		{
			_uiActivated = value;
			if (_uiActivated)
			{
				Canvas.willRenderCanvases += OnWillRenderCanvases;
			}
			else
			{
				Canvas.willRenderCanvases -= OnWillRenderCanvases;
			}
		}
		if (_uiActivated != base.enabled)
		{
			base.enabled = _uiActivated;
		}
	}

	public bool IsActivated()
	{
		return _uiActivated;
	}

	private void OnWillRenderCanvases()
	{
		if (_enableWaveformUI)
		{
			UpdateLabels();
		}
		_reticuleController.UpdateSignalscopeReticuleCanvases();
		if (_signalZoomArrow != null)
		{
			_signalZoomArrow.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(_signalZoomArrowStartRot, _signalZoomArrowEndRot, _signalscopeTool.GetZoomFraction()));
		}
	}

	private void Update()
	{
		if (_enableWaveformUI)
		{
			UpdateWaveform();
		}
		_reticuleController.UpdateSignalscopeReticule();
	}

	private void UpdateLabels()
	{
		_currentFreq = _signalscopeTool.GetFrequencyFilter();
		string text = string.Empty;
		if (_showFrequencyLabel)
		{
			text = text + UITextLibrary.GetString(UITextType.FrequencyLabel) + "\n";
		}
		text = ((!_signalscopeTool.AllowChangeFrequency()) ? (text + "<color=orange>" + AudioSignal.FrequencyToString(_currentFreq, showIndexNumber: false) + "</color>") : (text + "< <color=orange>" + AudioSignal.FrequencyToString(_currentFreq, showIndexNumber: false) + "</color> >"));
		_signalscopeLabel.text = text;
		if (!_isPlayerScope)
		{
			AudioSignal strongestSignal = _signalscopeTool.GetStrongestSignal();
			if (strongestSignal != null && strongestSignal.GetSignalStrength() > s_distanceTextThreshold)
			{
				float distanceFromScope = strongestSignal.GetDistanceFromScope();
				string text2 = "m";
				string text3 = (PlayerData.KnowsSignal(strongestSignal.GetName()) ? AudioSignal.SignalNameToString(strongestSignal.GetName()) : UITextLibrary.GetString(UITextType.UnknownSignal));
				_distanceLabel.text = text3 + ": " + Mathf.Round(distanceFromScope) + text2;
			}
			else
			{
				_distanceLabel.text = "";
			}
		}
	}

	private void UpdateWaveform()
	{
		_waveformRenderer.enabled = true;
		float num = _rectTransform.sizeDelta.x * 0.9f;
		float num2 = num * 0.5f;
		float num3 = num / 256f;
		AudioSignal strongestSignal = _signalscopeTool.GetStrongestSignal();
		float num4 = ((strongestSignal == null) ? 0f : strongestSignal.GetSignalVolume());
		if (num4 > 0f)
		{
			strongestSignal.GetOWAudioSource().GetOutputData(_audioSamples, 0);
			float num5 = _rectTransform.sizeDelta.y * _amplitude * num4 * strongestSignal.GetWaveformScalar();
			for (int i = 0; i < 256; i++)
			{
				_linePoints[i].x = (float)i * num3 - num2;
				_linePoints[i].y = _audioSamples[i] * num5;
			}
		}
		else
		{
			for (int j = 0; j < 256; j++)
			{
				_linePoints[j].x = (float)j * num3 - num2;
				_linePoints[j].y = 0f;
			}
		}
		_waveformRenderer.SetPositions(_linePoints);
	}

	private void OnEnterSignalscopeZoom(Signalscope telescope)
	{
		if (_signalZoomGroup != null)
		{
			_signalZoomGroup.enabled = true;
		}
	}

	private void OnExitSignalscopeZoom()
	{
		if (_signalZoomGroup != null)
		{
			_signalZoomGroup.enabled = false;
		}
	}

	public void SetWaveformUIActive(bool value)
	{
		_enableWaveformUI = value;
		if (_enableWaveformUI)
		{
			_signalscopeLabel.enabled = true;
			return;
		}
		_waveformRenderer.enabled = false;
		_leftBoundLine.enabled = false;
		_rightBoundLine.enabled = false;
		_signalscopeLabel.enabled = false;
	}

	public bool IsWaveformActive()
	{
		return _enableWaveformUI;
	}
}
