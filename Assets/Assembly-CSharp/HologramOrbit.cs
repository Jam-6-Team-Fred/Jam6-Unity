using UnityEngine;

public class HologramOrbit : MonoBehaviour
{
	[SerializeField]
	private AstroObject.Name _name;

	[SerializeField]
	private bool _startVisible;

	[Header("Eye Orbit")]
	[SerializeField]
	private Transform _outerTransform;

	[SerializeField]
	private Transform _innerTransform;

	private Transform _planetTransform;

	private bool _visible;

	private bool _rotatingEyeMode;

	private float _startEyeRotationTime;

	private float _eyeRotationDuration;

	private float _lastInnerDegrees;

	private float _targetInnerDegrees;

	private Quaternion _lastOuterRot;

	private Quaternion _targetOuterRot;

	private void Awake()
	{
		_visible = _startVisible;
		base.transform.localScale = (_visible ? Vector3.one : Vector3.zero);
	}

	private void Start()
	{
		if (_name != AstroObject.Name.Eye)
		{
			AstroObject astroObject = Locator.GetAstroObject(_name);
			if (astroObject != null)
			{
				_planetTransform = astroObject.transform;
			}
		}
		else
		{
			base.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, Random.insideUnitSphere) * Quaternion.identity;
		}
	}

	public AstroObject.Name GetName()
	{
		return _name;
	}

	public void SetVisible(bool visible, bool rotatingEyeMode = false)
	{
		_visible = visible;
		_rotatingEyeMode = rotatingEyeMode;
		if (_rotatingEyeMode && _visible && _name == AstroObject.Name.Eye)
		{
			_eyeRotationDuration = 1f;
			_startEyeRotationTime = Time.time;
			_lastOuterRot = (_targetOuterRot = Quaternion.identity);
			_lastInnerDegrees = (_targetInnerDegrees = 0f);
		}
	}

	public bool IsVisible()
	{
		return _visible;
	}

	private void FixedUpdate()
	{
		float num = ((_rotatingEyeMode && _name == AstroObject.Name.Eye && !_visible) ? 4f : 1f);
		base.transform.localScale = Vector3.MoveTowards(base.transform.localScale, _visible ? Vector3.one : Vector3.zero, num * Time.deltaTime);
		if (_rotatingEyeMode && _name == AstroObject.Name.Eye)
		{
			float t = Mathf.InverseLerp(_startEyeRotationTime, _startEyeRotationTime + _eyeRotationDuration, Time.time);
			t = Mathf.SmoothStep(0f, 1f, t);
			_outerTransform.localRotation = Quaternion.Slerp(_lastOuterRot, _targetOuterRot, t);
			float num2 = _lastInnerDegrees;
			if (_targetInnerDegrees - num2 > 180f)
			{
				num2 += 360f;
			}
			else if (_targetInnerDegrees - num2 < -180f)
			{
				num2 -= 360f;
			}
			_innerTransform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(num2, _targetInnerDegrees, t));
			if (t >= 1f)
			{
				ChooseNewTargetRotations();
			}
		}
		if (_planetTransform != null)
		{
			Vector3 toDirection = _planetTransform.position - Locator.GetSunTransform().position;
			base.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, toDirection) * Quaternion.identity;
		}
	}

	private void ChooseNewTargetRotations()
	{
		float num = Random.Range(60f, 180f);
		_startEyeRotationTime = Time.time;
		_eyeRotationDuration = Mathf.Lerp(1f, 3f, Mathf.InverseLerp(60f, 180f, num));
		_lastOuterRot = _targetOuterRot;
		Vector3 rhs = _outerTransform.parent.InverseTransformDirection(_outerTransform.forward);
		Vector3 axis = Vector3.Cross(Random.insideUnitSphere, rhs);
		_targetOuterRot = Quaternion.AngleAxis(num, axis) * _lastOuterRot;
		_lastInnerDegrees = _targetInnerDegrees;
		_targetInnerDegrees = _lastInnerDegrees + Mathf.Sign(Random.Range(-1f, 1f)) * num;
		_targetInnerDegrees = OWMath.WrapAngle(_targetInnerDegrees);
	}
}
