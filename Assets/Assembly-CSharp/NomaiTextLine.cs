using UnityEngine;

public class NomaiTextLine : MonoBehaviour, IGroupController
{
	public enum VisualState
	{
		HIDDEN = 0,
		UNREAD = 1,
		TRANSLATED = 2
	}

	public static Color s_originSpeakerColorUnread = new Color(0.5294114f, 0.5762672f, 1.5f, 1f);

	public static Color s_originSpeakerColorTranslated = new Color(0.6f, 0.6f, 0.6f, 1f);

	public static Color s_externalSpeakerColorUnread = new Color(1.5f, 0.9606341f, 39f / 68f, 1f);

	public static Color s_externalSpeakerColorTranslated = new Color(0.6f, 0.4f, 0.22f, 1f);

	[SerializeField]
	private int _entryID = -1;

	[SerializeField]
	private Vector3[] _points = new Vector3[0];

	private float _revealDuration;

	private float _animationTime;

	private float _animationStartTime;

	private Color _currentColor;

	private Color _startAnimationColor;

	private Color _targetColor;

	private NomaiText.Location _speakerLocation;

	private NomaiText.Location _textLineLocation;

	private VisualState _state;

	private bool _active;

	[SerializeField]
	[HideInInspector]
	private bool _prebuilt;

	[SerializeField]
	[HideInInspector]
	private float[] _lengths;

	[SerializeField]
	[HideInInspector]
	private float _totalLength;

	[SerializeField]
	[HideInInspector]
	private Vector3 _center = Vector3.zero;

	[SerializeField]
	[HideInInspector]
	private float _radius;

	private OWRenderer _renderer;

	private int _propID_RampBlend;

	public int groupControlMask => 1;

	public int GetEntryID()
	{
		return _entryID;
	}

	public void SetEntryID(int id)
	{
		_entryID = id;
	}

	public Vector3[] GetPoints()
	{
		return _points;
	}

	public void SetPoints(Vector3[] p)
	{
		_points = p;
	}

	public Vector3 GetLocalCenter()
	{
		return _center;
	}

	public Vector3 GetWorldCenter()
	{
		return base.transform.TransformPoint(_center);
	}

	public float GetLocalRadius()
	{
		return _radius;
	}

	public float GetWorldRadius()
	{
		return Mathf.Abs(base.transform.lossyScale.x) * _radius;
	}

	private void Awake()
	{
		if (!_prebuilt)
		{
			CalculateLengthAndCenter();
		}
		_revealDuration = 0.3f * _totalLength;
		_renderer = base.gameObject.GetAddComponent<OWRenderer>();
		_propID_RampBlend = Shader.PropertyToID("_RampBlend");
		_currentColor = DetermineTextLineColor(_state);
		_targetColor = _currentColor;
		base.enabled = false;
	}

	private void CalculateLengthAndCenter()
	{
		_lengths = new float[_points.Length - 1];
		_totalLength = 0f;
		for (int i = 0; i < _points.Length - 1; i++)
		{
			_lengths[i] = Vector3.Distance(_points[i], _points[i + 1]);
			_totalLength += _lengths[i];
		}
		_center = Vector3.zero;
		for (int j = 0; j < _points.Length; j++)
		{
			_center += _points[j];
		}
		_center /= (float)_points.Length;
		_radius = 0f;
		for (int k = 0; k < _points.Length; k++)
		{
			float sqrMagnitude = (_points[k] - _center).sqrMagnitude;
			if (sqrMagnitude > _radius)
			{
				_radius = sqrMagnitude;
			}
		}
		_radius = Mathf.Max(Mathf.Sqrt(_radius), 0.65f);
	}

	public Vector3 ClosestPointOnLine(Vector3 worldPos)
	{
		Vector3 vector = base.transform.InverseTransformPoint(worldPos);
		Vector3 position = vector;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _points.Length - 1; i++)
		{
			Vector3 vector2 = OWMath.ClosestPointOnSegment(vector, _points[i], _points[i + 1]);
			float sqrMagnitude = (vector - vector2).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				position = vector2;
				num = sqrMagnitude;
			}
		}
		return base.transform.TransformPoint(position);
	}

	public float GetDistToLine(Vector3 worldPos)
	{
		return Vector3.Distance(worldPos, ClosestPointOnLine(worldPos));
	}

	public Vector3 GetPointAlongLine(float t)
	{
		Vector3 position = _points[0];
		if (t <= 0f)
		{
			position = _points[0];
		}
		else if (t >= 1f)
		{
			position = _points[_points.Length - 1];
		}
		else
		{
			float num = t * _totalLength;
			float num2 = 0f;
			for (int i = 0; i < _points.Length - 1; i++)
			{
				if (num >= num2 && num <= num2 + _lengths[i])
				{
					position = Vector3.Lerp(_points[i], _points[i + 1], Mathf.InverseLerp(num2, num2 + _lengths[i], num));
					break;
				}
				num2 += _lengths[i];
			}
		}
		return base.transform.TransformPoint(position);
	}

	public float GetDistToCenter(Vector3 worldPos)
	{
		return Vector3.Distance(worldPos, GetWorldCenter());
	}

	public bool IsHidden()
	{
		return _state == VisualState.HIDDEN;
	}

	public bool IsTranslated()
	{
		return _state == VisualState.TRANSLATED;
	}

	public bool IsActive()
	{
		return _active;
	}

	public void SetActive(bool active, bool playAnimation)
	{
		SetActive(active, playAnimation, _revealDuration);
	}

	public void SetActive(bool active, bool playAnimation, float animDuration)
	{
		if (playAnimation)
		{
			if (active)
			{
				_active = active;
				StartColorChangeAnim(DetermineTextLineColor(VisualState.HIDDEN), DetermineTextLineColor(_state), animDuration);
			}
			else
			{
				StartColorChangeAnim(DetermineTextLineColor(_state), DetermineTextLineColor(VisualState.HIDDEN), animDuration);
				_active = active;
			}
		}
		else
		{
			_active = active;
			UpdateColorImmediate();
		}
	}

	public void SetUnreadState(bool updateImmediate = false)
	{
		if (updateImmediate || _state != 0)
		{
			_state = VisualState.UNREAD;
			UpdateColorImmediate();
		}
		else
		{
			StartColorChangeAnim(DetermineTextLineColor(_state), DetermineTextLineColor(VisualState.UNREAD));
			_state = VisualState.UNREAD;
		}
	}

	public void SetTranslatedState(bool updateImmediate = false)
	{
		if (updateImmediate || _state != VisualState.UNREAD)
		{
			_state = VisualState.TRANSLATED;
			UpdateColorImmediate();
		}
		else
		{
			StartColorChangeAnim(DetermineTextLineColor(_state), DetermineTextLineColor(VisualState.TRANSLATED));
			_state = VisualState.TRANSLATED;
		}
	}

	public bool IsAnimationComplete()
	{
		return _currentColor == _targetColor;
	}

	public bool IsRevealing()
	{
		if (base.enabled)
		{
			return _state == VisualState.UNREAD;
		}
		return false;
	}

	public bool IsAnimating()
	{
		return base.enabled;
	}

	public void SetLocations(NomaiText.Location speakerLocation, NomaiText.Location nomaiTextLocation)
	{
		_speakerLocation = speakerLocation;
		_textLineLocation = nomaiTextLocation;
	}

	public void UpdateColorImmediate()
	{
		_currentColor = DetermineTextLineColor(_state);
		_renderer.SetMaterialProperty(_propID_RampBlend, _currentColor.a);
		_renderer.SetEmissionColor(_currentColor);
		_renderer.SetActivation(_currentColor.a > 0f);
		base.enabled = false;
	}

	private void StartColorChangeAnim(Color startcolor, Color targetColor)
	{
		StartColorChangeAnim(startcolor, targetColor, _revealDuration);
	}

	private void StartColorChangeAnim(Color startcolor, Color targetColor, float duration)
	{
		_startAnimationColor = startcolor;
		_targetColor = targetColor;
		_animationStartTime = Time.unscaledTime;
		_animationTime = duration;
		base.enabled = true;
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.unscaledTime - _animationStartTime) / _animationTime);
		float r = num * (_targetColor.r - _startAnimationColor.r) + _startAnimationColor.r;
		float g = num * (_targetColor.g - _startAnimationColor.g) + _startAnimationColor.g;
		float b = num * (_targetColor.b - _startAnimationColor.b) + _startAnimationColor.b;
		float num2 = num * (_targetColor.a - _startAnimationColor.a) + _startAnimationColor.a;
		_currentColor = new Color(r, g, b, num2);
		_renderer.SetMaterialProperty(_propID_RampBlend, num2);
		_renderer.SetEmissionColor(_currentColor);
		_renderer.SetActivation(_currentColor.a > 0f);
		if (_currentColor == _targetColor)
		{
			base.enabled = false;
		}
	}

	private Color DetermineTextLineColor(VisualState state)
	{
		Color result = Color.white;
		bool flag = false;
		if (_active)
		{
			switch (state)
			{
			case VisualState.UNREAD:
				flag = true;
				result = ((_speakerLocation != 0 && _textLineLocation != 0) ? ((_speakerLocation != _textLineLocation) ? s_externalSpeakerColorUnread : s_originSpeakerColorUnread) : s_originSpeakerColorUnread);
				break;
			case VisualState.TRANSLATED:
				result = ((_speakerLocation == NomaiText.Location.UNSPECIFIED || _textLineLocation == NomaiText.Location.UNSPECIFIED) ? s_originSpeakerColorTranslated : ((_speakerLocation != _textLineLocation) ? s_externalSpeakerColorTranslated : s_originSpeakerColorTranslated));
				flag = true;
				break;
			case VisualState.HIDDEN:
				result = Color.white;
				flag = false;
				break;
			}
		}
		if (flag)
		{
			result.a = 1f;
		}
		else
		{
			result.a = 0f;
		}
		return result;
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < _points.Length; i++)
		{
			zero += _points[i];
		}
		zero /= (float)_points.Length;
		Gizmos.color = Color.red;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		for (int j = 0; j < _points.Length - 1; j++)
		{
			Gizmos.DrawLine(_points[j], _points[j + 1]);
		}
		for (int k = 0; k < _points.Length; k++)
		{
			Gizmos.DrawSphere(_points[k], 0.01f);
		}
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(zero, 0.05f);
		OWGizmos.DrawWireCircle(zero, Vector3.forward, _radius);
	}
}
