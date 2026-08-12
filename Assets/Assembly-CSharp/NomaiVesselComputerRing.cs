using System;
using UnityEngine;

public class NomaiVesselComputerRing : MonoBehaviour
{
	private static Color s_textColorTranslated = new Color(1.5f, 1.5f, 1.5f, 1f);

	private static Color s_textShadowColorTranslated = new Color(1f, 1f, 1f, 0.3f);

	private static Color s_projectorColorTranslated = new Color(3f, 3f, 3f, 1f);

	[SerializeField]
	private OWRenderer _ringRenderer;

	[SerializeField]
	private OWRenderer _projectorRenderer;

	[SerializeField]
	private float _radius = 0.5f;

	[SerializeField]
	private float _spinSpeedMin = 10f;

	[SerializeField]
	private float _spinSpeedMax = 30f;

	[SerializeField]
	private float _flickerLength = 1f;

	[SerializeField]
	private float _colorFadeTime = 1f;

	private NomaiVesselComputer _computer;

	private bool _activated;

	private int _entryID;

	private bool _translated;

	private float _spinSpeed;

	private float _delayTimer;

	private float _flickerTimer;

	private int _propID_ShadowColor;

	private Color _baseTextColor;

	private Color _baseTextShadowColor;

	private Color _baseProjectorColor;

	private float _colorT;

	private void Awake()
	{
		_computer = GetComponentInParent<NomaiVesselComputer>();
		_ringRenderer.SetActivation(active: false);
		_projectorRenderer.SetActivation(active: false);
		_activated = false;
		_entryID = -1;
		_translated = false;
		_delayTimer = 0f;
		_flickerTimer = 0f;
		_propID_ShadowColor = Shader.PropertyToID("_ShadowColor");
		_baseTextColor = _ringRenderer.GetOriginalColor();
		_baseTextShadowColor = _ringRenderer.sharedMaterial.GetColor(_propID_ShadowColor);
		_baseProjectorColor = _projectorRenderer.GetOriginalColor();
		_colorT = 1f;
		_baseTextColor = _baseTextColor.gamma;
		_baseTextShadowColor = _baseTextShadowColor.gamma;
		_baseProjectorColor = _baseProjectorColor.gamma;
		base.enabled = false;
	}

	private void Update()
	{
		_ringRenderer.transform.Rotate(0f, _spinSpeed * Time.deltaTime, 0f, Space.Self);
		if (_delayTimer > 0f)
		{
			_delayTimer -= Time.deltaTime;
		}
		else if (_flickerTimer > 0f)
		{
			_flickerTimer -= Time.deltaTime;
			if (_flickerTimer <= 0f)
			{
				_ringRenderer.SetActivation(_activated);
				_projectorRenderer.SetActivation(_activated);
				if (!_activated)
				{
					base.enabled = false;
				}
			}
			else
			{
				bool activation = UnityEngine.Random.value > 0.5f;
				_ringRenderer.SetActivation(activation);
				_projectorRenderer.SetActivation(activation);
			}
		}
		if (!_translated && _colorT < 1f)
		{
			_colorT = Mathf.MoveTowards(_colorT, 1f, Time.unscaledDeltaTime / _colorFadeTime);
			UpdateColor();
		}
		else if (_translated && _colorT > 0f)
		{
			_colorT = Mathf.MoveTowards(_colorT, 0f, Time.unscaledDeltaTime / _colorFadeTime);
			UpdateColor();
		}
	}

	private void UpdateColor()
	{
		_ringRenderer.SetColor(Color.Lerp(s_textColorTranslated, _baseTextColor, _colorT));
		_ringRenderer.SetMaterialProperty(_propID_ShadowColor, Color.Lerp(s_textShadowColorTranslated, _baseTextShadowColor, _colorT));
		_projectorRenderer.SetColor(Color.Lerp(s_projectorColorTranslated, _baseProjectorColor, _colorT));
	}

	public void ActivateInstant(int id, bool enabled)
	{
		_ringRenderer.transform.localEulerAngles = new Vector3(0f, UnityEngine.Random.Range(-180f, 180f), 0f);
		_spinSpeed = UnityEngine.Random.Range(_spinSpeedMin, _spinSpeedMax);
		_activated = true;
		_entryID = id;
		_translated = _computer.IsTranslated(id);
		_delayTimer = 0f;
		_flickerTimer = 0f;
		_colorT = (_translated ? 0f : 1f);
		_ringRenderer.SetActivation(active: true);
		_projectorRenderer.SetActivation(active: true);
		UpdateColor();
		base.enabled = enabled;
	}

	public void Activate(int id, float delay = 0f)
	{
		if (!_activated && _flickerTimer <= 0f)
		{
			_ringRenderer.transform.localEulerAngles = new Vector3(0f, UnityEngine.Random.Range(-180f, 180f), 0f);
		}
		_spinSpeed = UnityEngine.Random.Range(_spinSpeedMin, _spinSpeedMax);
		_activated = true;
		_entryID = id;
		_translated = _computer.IsTranslated(id);
		_delayTimer = delay;
		_flickerTimer = _flickerLength;
		_colorT = (_translated ? 0f : 1f);
		UpdateColor();
		base.enabled = true;
	}

	public void Deactivate(float delay = 0f)
	{
		_activated = false;
		_entryID = -1;
		_delayTimer = delay;
		_flickerTimer = _flickerLength;
		base.enabled = true;
	}

	public bool IsActivated()
	{
		return _activated;
	}

	public NomaiVesselComputer GetComputer()
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

	public bool IsDoneFlickering()
	{
		return _flickerTimer <= 0f;
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
