using UnityEngine;

public class LighthouseSignalController : MonoBehaviour
{
	[SerializeField]
	private DreamCampfire[] _dreamCampfires;

	[SerializeField]
	private OWRenderer[] _signalRenderers;

	[SerializeField]
	private float _fadeTime = 0.2f;

	private OWEvent.OWCallback[] _dreamFireCallbacks;

	private MaterialPropertyBlock[] _signalPropertyBlocks;

	private float[] _fadeStartTimes;

	private Color _defaultSignalColor;

	private static readonly int _propID_LinesColor = Shader.PropertyToID("_LinesColor");

	private static readonly int _propID_GemsColor = Shader.PropertyToID("_GemsColor");

	private void Awake()
	{
		_dreamFireCallbacks = new OWEvent.OWCallback[_dreamCampfires.Length];
		_signalPropertyBlocks = new MaterialPropertyBlock[_signalRenderers.Length];
		_fadeStartTimes = new float[_signalRenderers.Length];
		_defaultSignalColor = _signalRenderers[0].sharedMaterial.GetColor(_propID_LinesColor);
		for (int i = 0; i < _dreamCampfires.Length; i++)
		{
			_signalPropertyBlocks[i] = new MaterialPropertyBlock();
			_fadeStartTimes[i] = -1f;
			int currentIdx = i;
			_dreamFireCallbacks[i] = delegate
			{
				_fadeStartTimes[currentIdx] = Time.time;
				base.enabled = true;
			};
			_dreamCampfires[i].OnDreamCampfireExtinguished += _dreamFireCallbacks[i];
		}
	}

	private void Update()
	{
		bool flag = false;
		for (int i = 0; i < _fadeStartTimes.Length; i++)
		{
			if (_fadeStartTimes[i] > 0f)
			{
				if (Time.time >= _fadeStartTimes[i] + _fadeTime)
				{
					_fadeStartTimes[i] = -1f;
					continue;
				}
				SetSignalColor(i, Color.Lerp(_defaultSignalColor, Color.black, Mathf.InverseLerp(_fadeStartTimes[i], _fadeStartTimes[i] + _fadeTime, Time.time)));
				flag = true;
			}
		}
		if (!flag)
		{
			base.enabled = false;
		}
	}

	private void SetSignalColor(int idx, Color color)
	{
		_signalRenderers[idx].SetMaterialProperty(_propID_LinesColor, color);
		_signalRenderers[idx].SetMaterialProperty(_propID_GemsColor, color);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _dreamCampfires.Length; i++)
		{
			_dreamCampfires[i].OnDreamCampfireExtinguished -= _dreamFireCallbacks[i];
		}
		_dreamFireCallbacks = null;
	}
}
