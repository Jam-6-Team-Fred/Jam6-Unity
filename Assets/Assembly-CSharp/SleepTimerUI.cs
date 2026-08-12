using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SleepTimerUI : MonoBehaviour
{
	private struct EmberInstance
	{
		public Vector2 position;

		public float rotation;

		public float scale;

		public float radius;

		public Color tint;

		public bool alive;

		public float heat;

		public RectTransform rectTransform;

		public Image image;

		public EmberInstance(Image img)
		{
			position = Vector2.zero;
			rotation = 0f;
			scale = 1f;
			radius = 50f;
			tint = Color.white;
			alive = false;
			heat = 1f;
			rectTransform = img.rectTransform;
			image = img;
			image.enabled = false;
		}
	}

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private Text _text;

	[Space]
	[SerializeField]
	private Transform _emberRoot;

	[SerializeField]
	private Vector2 _emberDelay = new Vector2(0f, 3f);

	[Space]
	[SerializeField]
	private Vector2 _constantForce = new Vector2(0f, 100f);

	[SerializeField]
	private Vector2 _noiseForce = new Vector2(100f, 100f);

	[SerializeField]
	private Vector2 _noiseScreenScale = new Vector2(2f, 2f);

	[SerializeField]
	private Vector2 _noiseTimeScale = new Vector2(1f, 1f);

	[Space]
	[SerializeField]
	private Vector2 _randomScale = new Vector2(0.9f, 1.1f);

	[SerializeField]
	private Gradient _randomTint = new Gradient();

	[SerializeField]
	private Gradient _dreamFireRandomTint = new Gradient();

	[SerializeField]
	private Vector2 _heatVelRange = new Vector2(0f, 100f);

	[SerializeField]
	private Gradient _heatTint = new Gradient();

	[SerializeField]
	private Gradient _dreamFireHeatTint = new Gradient();

	[SerializeField]
	private Material _defaultEmberMaterial;

	[SerializeField]
	private Material _dreamEmberMaterial;

	private StringBuilder _stringBuilder = new StringBuilder(8);

	private float _sleepStartTime;

	private float _sleepStartTimeUnscaled;

	private Color _textColor;

	private EmberInstance[] _emberInstances;

	private float _emberTimer;

	private bool _dreamEmbers;

	private void Awake()
	{
		base.enabled = false;
		_canvas.enabled = false;
		_textColor = _text.color;
		if (_emberRoot != null)
		{
			Image[] componentsInChildren = _emberRoot.GetComponentsInChildren<Image>();
			_emberInstances = new EmberInstance[componentsInChildren.Length];
			for (int i = 0; i < _emberInstances.Length; i++)
			{
				_emberInstances[i] = new EmberInstance(componentsInChildren[i]);
			}
		}
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
	}

	private void OnDestroy()
	{
		if (_canvas.enabled)
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases;
		}
		GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
	}

	private void OnStartFastForward()
	{
		_sleepStartTime = Time.timeSinceLevelLoad;
		_sleepStartTimeUnscaled = Time.unscaledTime;
		_emberTimer = 3f;
		_dreamEmbers = PlayerState.IsSleepingAtDreamCampfire();
		base.enabled = true;
		_canvas.enabled = true;
		_text.text = "00:00";
		_text.color = new Color(_textColor.r, _textColor.g, _textColor.b, 0f);
		Canvas.willRenderCanvases += OnWillRenderCanvases;
		if (_emberInstances != null)
		{
			for (int i = 0; i < _emberInstances.Length; i++)
			{
				_emberInstances[i].image.material = (_dreamEmbers ? _dreamEmberMaterial : _defaultEmberMaterial);
			}
		}
	}

	private void OnEndFastForward()
	{
		base.enabled = false;
		_canvas.enabled = false;
		Canvas.willRenderCanvases -= OnWillRenderCanvases;
		if (_emberInstances != null)
		{
			for (int i = 0; i < _emberInstances.Length; i++)
			{
				_emberInstances[i].alive = false;
				_emberInstances[i].image.enabled = false;
			}
		}
	}

	private void Update()
	{
		if (_emberInstances == null)
		{
			base.enabled = false;
			return;
		}
		Rect rect = new Rect(_canvas.pixelRect.size * -0.5f, _canvas.pixelRect.size);
		Vector2 vector = new Vector2(1f / rect.width * _noiseScreenScale.x, 1f / rect.height * _noiseScreenScale.y);
		float unscaledTime = Time.unscaledTime;
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		_emberTimer -= unscaledDeltaTime;
		if (_emberTimer <= 0f)
		{
			for (int i = 0; i < _emberInstances.Length; i++)
			{
				if (!_emberInstances[i].alive)
				{
					_emberInstances[i].position = new Vector2(Random.Range(rect.xMin, rect.xMax) * 0.5f, rect.yMin - _emberInstances[i].radius);
					_emberInstances[i].rotation = Random.value * 360f;
					_emberInstances[i].scale = Random.Range(_randomScale.x, _randomScale.y);
					_emberInstances[i].tint = GetRandomTint(Random.value);
					_emberInstances[i].alive = true;
					_emberInstances[i].image.enabled = true;
					break;
				}
			}
			_emberTimer = Random.Range(_emberDelay.x, _emberDelay.y);
		}
		for (int j = 0; j < _emberInstances.Length; j++)
		{
			if (_emberInstances[j].alive)
			{
				float num = Mathf.PerlinNoise(_emberInstances[j].position.x * vector.x, _noiseTimeScale.x * unscaledTime) * 2f - 1f;
				float num2 = Mathf.PerlinNoise(_emberInstances[j].position.y * vector.y, _noiseTimeScale.y * unscaledTime);
				Vector2 vector2 = new Vector2(_constantForce.x + num * _noiseForce.x, _constantForce.y + num2 * _noiseForce.y);
				_emberInstances[j].position += vector2 * unscaledDeltaTime;
				_emberInstances[j].rotation += num * 360f * unscaledDeltaTime;
				_emberInstances[j].heat = Mathf.InverseLerp(_heatVelRange.x, _heatVelRange.y, vector2.magnitude);
				if (_emberInstances[j].position.x < rect.xMin - _emberInstances[j].radius || _emberInstances[j].position.x > rect.xMax + _emberInstances[j].radius || _emberInstances[j].position.y > rect.yMax + _emberInstances[j].radius)
				{
					_emberInstances[j].alive = false;
					_emberInstances[j].image.enabled = false;
				}
			}
		}
	}

	private Color GetRandomTint(float value)
	{
		if (!_dreamEmbers)
		{
			return _randomTint.Evaluate(value);
		}
		return _dreamFireRandomTint.Evaluate(value);
	}

	private Color GetHeatTint(float value)
	{
		if (!_dreamEmbers)
		{
			return _heatTint.Evaluate(value);
		}
		return _dreamFireHeatTint.Evaluate(value);
	}

	private void OnWillRenderCanvases()
	{
		float num = Mathf.Max(Time.timeSinceLevelLoad - _sleepStartTime, 0f);
		int num2 = Mathf.FloorToInt(num / 60f);
		int num3 = Mathf.FloorToInt(num) % 60;
		_stringBuilder.Length = 0;
		_stringBuilder.Append(num2.ToString("D2"));
		_stringBuilder.Append(":");
		_stringBuilder.Append(num3.ToString("D2"));
		_text.text = _stringBuilder.ToString();
		float a = Mathf.Clamp01((Time.unscaledTime - _sleepStartTimeUnscaled) / 3f);
		_text.color = new Color(_textColor.r, _textColor.g, _textColor.b, a);
		if (_emberInstances == null)
		{
			return;
		}
		for (int i = 0; i < _emberInstances.Length; i++)
		{
			if (_emberInstances[i].alive)
			{
				_emberInstances[i].image.color = _emberInstances[i].tint * GetHeatTint(_emberInstances[i].heat);
				_emberInstances[i].rectTransform.localPosition = _emberInstances[i].position;
				_emberInstances[i].rectTransform.localEulerAngles = new Vector3(0f, 0f, _emberInstances[i].rotation);
				_emberInstances[i].rectTransform.localScale = new Vector3(_emberInstances[i].scale, _emberInstances[i].scale, _emberInstances[i].scale);
			}
		}
	}
}
