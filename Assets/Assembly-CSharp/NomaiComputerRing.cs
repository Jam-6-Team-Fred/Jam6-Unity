using System;
using UnityEngine;

public class NomaiComputerRing : MonoBehaviour
{
	private static Color s_colorTranslated = new Color(0.6f, 0.6f, 0.6f, 1f);

	private static MaterialPropertyBlock s_matPropBlock = null;

	private static int s_propID_Detail1EmissionColor;

	[SerializeField]
	private float _radius = 0.5f;

	[SerializeField]
	private DampedSpring _translationSpring;

	[SerializeField]
	private DampedSpring _rotationSpring;

	[SerializeField]
	private float _idleSpinMinDegrees = 15f;

	[SerializeField]
	private float _idleSpinMaxDegrees = 270f;

	[SerializeField]
	private float _activeSpinSpeed = 30f;

	[SerializeField]
	private float _colorFadeTime = 1f;

	private Renderer _renderer;

	private NomaiComputer _computer;

	private bool _activated;

	private int _entryID;

	private bool _translated;

	private bool _inPosition;

	private float _targetHeight;

	private float _currentHeight;

	private float _targetRotation;

	private float _currentRotation;

	private Color _baseEmissionColor;

	private float _emissionColorT;

	public void Initialize()
	{
		_renderer = base.gameObject.GetRequiredComponent<Renderer>();
		_computer = GetComponentInParent<NomaiComputer>();
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_Detail1EmissionColor = Shader.PropertyToID("_Detail1EmissionColor");
		}
		_activated = false;
		_entryID = -1;
		_translated = false;
		_targetHeight = 0f;
		_currentHeight = 0f;
		_targetRotation = 0f;
		_currentRotation = 0f;
		_baseEmissionColor = _renderer.sharedMaterial.GetColor(s_propID_Detail1EmissionColor);
		_baseEmissionColor = _baseEmissionColor.gamma;
		_emissionColorT = 1f;
	}

	private void FixedUpdate()
	{
		_currentHeight = _translationSpring.Update(_currentHeight, _targetHeight, Time.deltaTime);
		_currentRotation = _rotationSpring.Update(_currentRotation, _targetRotation, Time.deltaTime);
		if (_activated)
		{
			_targetRotation += _activeSpinSpeed * Time.deltaTime;
		}
		else if (Mathf.Abs(_currentRotation - _targetRotation) < 0.001f)
		{
			_targetRotation += UnityEngine.Random.Range(_idleSpinMinDegrees, _idleSpinMaxDegrees) * ((UnityEngine.Random.value > 0.5f) ? 1f : (-1f));
		}
		if (!_inPosition)
		{
			bool flag = (_currentHeight > _targetHeight && base.transform.localPosition.y < _targetHeight) || (_currentHeight < _targetHeight && base.transform.localPosition.y > _targetHeight);
			_inPosition = flag || Mathf.Abs(_currentHeight - _targetHeight) < 0.001f;
		}
		base.transform.localPosition = new Vector3(0f, _currentHeight, 0f);
		base.transform.localRotation = Quaternion.AngleAxis(_currentRotation, Vector3.up);
	}

	private void Update()
	{
		if ((!_activated || !_translated) && _emissionColorT < 1f)
		{
			_emissionColorT = Mathf.MoveTowards(_emissionColorT, 1f, Time.unscaledDeltaTime / _colorFadeTime);
			s_matPropBlock.SetColor(s_propID_Detail1EmissionColor, Color.Lerp(s_colorTranslated, _baseEmissionColor, _emissionColorT));
			_renderer.SetPropertyBlock(s_matPropBlock);
		}
		else if (_activated && _translated && _emissionColorT > 0f)
		{
			_emissionColorT = Mathf.MoveTowards(_emissionColorT, 0f, Time.unscaledDeltaTime / _colorFadeTime);
			s_matPropBlock.SetColor(s_propID_Detail1EmissionColor, Color.Lerp(s_colorTranslated, _baseEmissionColor, _emissionColorT));
			_renderer.SetPropertyBlock(s_matPropBlock);
		}
	}

	public void Activate(int id, float hoverHeight = 0f)
	{
		_activated = true;
		_entryID = id;
		_translated = _computer.IsTranslated(id);
		_inPosition = false;
		_targetHeight = hoverHeight;
		_targetRotation = _currentRotation + 360f;
	}

	public void Deactivate(float dockedHeight)
	{
		_activated = false;
		_entryID = -1;
		_translated = false;
		_inPosition = false;
		_targetHeight = dockedHeight;
		_targetRotation = _currentRotation + 360f;
	}

	public bool IsActivated()
	{
		return _activated;
	}

	public NomaiComputer GetComputer()
	{
		return _computer;
	}

	public int GetEntryID()
	{
		return _entryID;
	}

	public void SetAsTranslated()
	{
		_translated = true;
	}

	public bool IsInPosition()
	{
		return _inPosition;
	}

	public void SetHoverHeight(float hoverHeight)
	{
		if (_activated)
		{
			_targetHeight = hoverHeight;
		}
	}

	public float GetCurrentHeight()
	{
		return _currentHeight;
	}

	public Vector3 GetPointOnRing(float t, Vector3 fromWorldPos)
	{
		Vector3 vector = Vector3.ProjectOnPlane(fromWorldPos - base.transform.position, base.transform.up);
		if (vector.sqrMagnitude > 1.0000001E-06f)
		{
			vector.Normalize();
			Vector3 vector2 = Vector3.Cross(vector, base.transform.up);
			Vector3 vector3 = vector * Mathf.Sin(t * (float)Math.PI) + vector2 * Mathf.Cos(t * (float)Math.PI);
			return base.transform.position + vector3 * _radius;
		}
		Vector3 vector4 = base.transform.forward * Mathf.Sin(t * (float)Math.PI * 2f) + base.transform.right * Mathf.Cos(t * (float)Math.PI * 2f);
		return base.transform.position + vector4 * _radius;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.red;
			OWGizmos.DrawWireCircle(base.transform.position, base.transform.up, _radius);
		}
	}
}
