using UnityEngine;

public class TranslatorScanBeam : MonoBehaviour
{
	[SerializeField]
	private Renderer _projectorRenderer;

	[SerializeField]
	private Renderer _lightVolumeRenderer;

	[Space(10f)]
	[SerializeField]
	private float _focusedBeamWidth = 0.25f;

	[SerializeField]
	private float _maxBeamWidth = 1f;

	[SerializeField]
	private float _maxBeamLength = 10f;

	[SerializeField]
	private float _scanSpeed = 1f;

	[SerializeField]
	private float _scanOffset;

	[SerializeField]
	private float _switchLength = 1f;

	[SerializeField]
	private float _fadeLength = 1f;

	private bool _tooCloseToTarget;

	private NomaiTextLine _nomaiTextLine;

	private NomaiComputerRing _nomaiComputerRing;

	private NomaiVesselComputerRing _nomaiVesselComputerRing;

	private float _scanTime;

	private float _switchTime;

	private Quaternion _baseRotation;

	private Color _baseProjectorColor;

	private Color _baseLightColor;

	private Quaternion _prevRotation;

	private Vector3 _prevScale;

	private float _fade;

	private void Awake()
	{
		_tooCloseToTarget = false;
		_baseRotation = base.transform.localRotation;
		_prevRotation = Quaternion.identity;
		_prevScale = new Vector3(_maxBeamWidth, _maxBeamWidth, _maxBeamLength);
		_fade = 0f;
		if ((bool)_projectorRenderer)
		{
			_baseProjectorColor = _projectorRenderer.material.color;
			_projectorRenderer.material.SetAlpha(0f);
			_projectorRenderer.enabled = false;
		}
		if ((bool)_lightVolumeRenderer)
		{
			_baseLightColor = _lightVolumeRenderer.material.color;
			_lightVolumeRenderer.material.SetAlpha(0f);
			_lightVolumeRenderer.enabled = false;
		}
	}

	private void OnDisable()
	{
		_tooCloseToTarget = false;
		_nomaiTextLine = null;
		_prevRotation = Quaternion.identity;
		_prevScale = new Vector3(_maxBeamWidth, _maxBeamWidth, _maxBeamLength);
		_fade = 0f;
		if ((bool)_projectorRenderer)
		{
			_projectorRenderer.material.SetAlpha(0f);
			_projectorRenderer.enabled = false;
		}
		if ((bool)_lightVolumeRenderer)
		{
			_lightVolumeRenderer.material.SetAlpha(0f);
			_lightVolumeRenderer.enabled = false;
		}
		base.transform.localRotation = _baseRotation;
		base.transform.localScale = _prevScale;
	}

	public bool IsSwitching()
	{
		return _switchTime < 1f;
	}

	private void Update()
	{
		if ((bool)_nomaiTextLine && !_tooCloseToTarget)
		{
			_switchTime = Mathf.MoveTowards(_switchTime, 1f, Time.deltaTime / _switchLength);
			float t = Mathf.SmoothStep(0f, 1f, _switchTime);
			_scanTime += Time.deltaTime * _scanSpeed;
			float num = Mathf.Cos(_scanTime + _scanOffset) * 0.5f + 0.5f;
			Vector3 pointAlongLine = _nomaiTextLine.GetPointAlongLine(num);
			Vector3 rhs = _nomaiTextLine.GetPointAlongLine(num + 0.1f) - _nomaiTextLine.GetPointAlongLine(num - 0.1f);
			Vector3 vector = pointAlongLine - base.transform.position;
			Vector3 upwards = Vector3.Cross(vector, rhs);
			float num2 = Vector3.Distance(base.transform.position, pointAlongLine);
			Quaternion q = Quaternion.LookRotation(vector, upwards);
			Quaternion b = base.transform.parent.InverseTransformRotation(q);
			base.transform.localRotation = Quaternion.Lerp(_prevRotation, b, t);
			base.transform.localScale = Vector3.Lerp(_prevScale, new Vector3(_focusedBeamWidth, _focusedBeamWidth, 1f + num2), t);
		}
		else if ((bool)_nomaiComputerRing && !_tooCloseToTarget)
		{
			_switchTime = Mathf.MoveTowards(_switchTime, 1f, Time.deltaTime / _switchLength);
			float t2 = Mathf.SmoothStep(0f, 1f, _switchTime);
			_scanTime += Time.deltaTime * _scanSpeed;
			float t3 = Mathf.Cos(_scanTime + _scanOffset) * 0.5f + 0.5f;
			t3 = Mathf.Lerp(0.25f, 0.75f, t3);
			Vector3 pointOnRing = _nomaiComputerRing.GetPointOnRing(t3, base.transform.position);
			Vector3 forward = pointOnRing - base.transform.position;
			Vector3 up = _nomaiComputerRing.transform.up;
			float num3 = Vector3.Distance(base.transform.position, pointOnRing);
			Quaternion q2 = Quaternion.LookRotation(forward, up);
			Quaternion b2 = base.transform.parent.InverseTransformRotation(q2);
			base.transform.localRotation = Quaternion.Lerp(_prevRotation, b2, t2);
			base.transform.localScale = Vector3.Lerp(_prevScale, new Vector3(_focusedBeamWidth, _focusedBeamWidth, 1f + num3), t2);
		}
		else if ((bool)_nomaiVesselComputerRing && !_tooCloseToTarget)
		{
			_switchTime = Mathf.MoveTowards(_switchTime, 1f, Time.deltaTime / _switchLength);
			float t4 = Mathf.SmoothStep(0f, 1f, _switchTime);
			_scanTime += Time.deltaTime * _scanSpeed;
			float t5 = Mathf.Cos(_scanTime + _scanOffset) * 0.5f + 0.5f;
			t5 = Mathf.Lerp(0.25f, 0.75f, t5);
			Vector3 pointOnRing2 = _nomaiVesselComputerRing.GetPointOnRing(t5, base.transform.position);
			Vector3 forward2 = pointOnRing2 - base.transform.position;
			Vector3 up2 = _nomaiVesselComputerRing.transform.up;
			float num4 = Vector3.Distance(base.transform.position, pointOnRing2);
			Quaternion q3 = Quaternion.LookRotation(forward2, up2);
			Quaternion b3 = base.transform.parent.InverseTransformRotation(q3);
			base.transform.localRotation = Quaternion.Lerp(_prevRotation, b3, t4);
			base.transform.localScale = Vector3.Lerp(_prevScale, new Vector3(_focusedBeamWidth, _focusedBeamWidth, 1f + num4), t4);
		}
		else
		{
			_switchTime = Mathf.MoveTowards(_switchTime, 1f, Time.deltaTime / _fadeLength);
			float t6 = Mathf.SmoothStep(0f, 1f, _switchTime * (2f - _switchTime));
			base.transform.localRotation = Quaternion.Lerp(_prevRotation, _baseRotation, _switchTime);
			base.transform.localScale = Vector3.Lerp(_prevScale, new Vector3(_maxBeamWidth, _maxBeamWidth, _maxBeamLength), t6);
		}
		bool flag = !_tooCloseToTarget && (_nomaiTextLine != null || _nomaiComputerRing != null || _nomaiVesselComputerRing != null);
		_fade = Mathf.MoveTowards(_fade, flag ? 1f : 0f, Time.deltaTime / _fadeLength * (_tooCloseToTarget ? 3f : 1f));
		if ((bool)_projectorRenderer)
		{
			bool flag2 = _fade > 0f;
			if (_projectorRenderer.enabled != flag2)
			{
				_projectorRenderer.enabled = flag2;
			}
			if (_projectorRenderer.enabled)
			{
				_projectorRenderer.material.SetAlpha(_fade * _fade * _baseProjectorColor.a);
			}
		}
		if ((bool)_lightVolumeRenderer)
		{
			bool flag3 = _fade > 0f;
			if (_lightVolumeRenderer.enabled != flag3)
			{
				_lightVolumeRenderer.enabled = flag3;
			}
			if (_lightVolumeRenderer.enabled)
			{
				_lightVolumeRenderer.material.SetAlpha(_fade * _fade * _baseLightColor.a);
			}
		}
	}

	public void SetTooCloseToTarget(bool tooClose)
	{
		if (_tooCloseToTarget != tooClose)
		{
			_tooCloseToTarget = tooClose;
			_switchTime = 0f;
			_prevRotation = base.transform.localRotation;
			_prevScale = base.transform.localScale;
		}
	}

	public void SetNomaiTextLine(NomaiTextLine line)
	{
		if (_nomaiTextLine != line)
		{
			_switchTime = 0f;
			_nomaiTextLine = line;
			_prevRotation = base.transform.localRotation;
			_prevScale = base.transform.localScale;
		}
	}

	public void SetNomaiComputerRing(NomaiComputerRing ring)
	{
		if (_nomaiComputerRing != ring)
		{
			_switchTime = 0f;
			_nomaiComputerRing = ring;
			_prevRotation = base.transform.localRotation;
			_prevScale = base.transform.localScale;
		}
	}

	public void SetNomaiVesselComputerRing(NomaiVesselComputerRing ring)
	{
		if (_nomaiVesselComputerRing != ring)
		{
			_switchTime = 0f;
			_nomaiVesselComputerRing = ring;
			_prevRotation = base.transform.localRotation;
			_prevScale = base.transform.localScale;
		}
	}
}
