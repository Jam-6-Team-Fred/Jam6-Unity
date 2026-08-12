using System;
using UnityEngine;

public class DreamLanternController : MonoBehaviour, ILightSource
{
	[SerializeField]
	private float _minRange = 8f;

	[SerializeField]
	private float _maxRange = 30f;

	[SerializeField]
	private float _minAngle = 30f;

	[SerializeField]
	private float _maxAngle = 130f;

	[Space]
	[SerializeField]
	private GameObject _worldModelGroup;

	[SerializeField]
	private GameObject _viewModelGroup;

	[SerializeField]
	private OWLight2 _light;

	[SerializeField]
	private OWRenderer[] _flameRenderers = new OWRenderer[0];

	[SerializeField]
	private OWLight2[] _flameLights = new OWLight2[0];

	[SerializeField]
	private LensFlare _lensFlare;

	[SerializeField]
	private LightFlicker2 _flicker;

	[Space]
	[SerializeField]
	private Transform _focuserGroup;

	[SerializeField]
	private Transform[] _focuserPetals = new Transform[0];

	[SerializeField]
	private Transform[] _concealerRoots = new Transform[0];

	[SerializeField]
	private Transform[] _concealerCovers = new Transform[0];

	[SerializeField]
	private Transform[] _concealerCoversVMPrepass = new Transform[0];

	[SerializeField]
	private Vector3[] _concealerCoverTargets = new Vector3[0];

	[Space]
	[SerializeField]
	private GameObject _simLightConeUnfocused;

	[SerializeField]
	private GameObject _simLightConeFocused;

	private float _focus;

	private float _flameStrength;

	private float _concealment;

	private float _litTime;

	private float _lensFlareStrength;

	private bool _lit;

	private bool _concealed;

	private bool _heldByPlayer;

	private bool _socketed;

	private bool _grabbedByGhost;

	private bool _dirtyFlag_range = true;

	private bool _dirtyFlag_focus = true;

	private bool _dirtyFlag_flameStrength = true;

	private bool _dirtyFlag_concealment = true;

	private bool _dirtyFlag_lensFlareStrength = true;

	private bool _dirtyFlag_lit = true;

	private bool _dirtyFlag_concealed = true;

	private bool _dirtyFlag_heldByPlayer = true;

	private bool _dirtyFlag_socketed = true;

	private bool _dirtyFlag_grabbedByGhost = true;

	private int _propID_MainTex_ST = Shader.PropertyToID("_MainTex_ST");

	private float _origLensFlareBrightness;

	private Vector3[] _focuserPetalsBaseEulerAngles;

	private Vector3[] _concealerRootsBaseScale;

	private Vector3[] _concealerCoversStartPos;

	private LightSourceVolume _lightVolume;

	private SphereShape _lightSourceVolumeShape;

	private OWLight2[] _interfaceCompatibleList;

	public bool grabbedByGhost
	{
		get
		{
			return _grabbedByGhost;
		}
		set
		{
			_grabbedByGhost = value;
			_dirtyFlag_grabbedByGhost = true;
		}
	}

	private void Awake()
	{
		_origLensFlareBrightness = _lensFlare.brightness;
		_focuserPetalsBaseEulerAngles = new Vector3[_focuserPetals.Length];
		for (int i = 0; i < _focuserPetals.Length; i++)
		{
			_focuserPetalsBaseEulerAngles[i] = _focuserPetals[i].localEulerAngles;
		}
		_concealerRootsBaseScale = new Vector3[_concealerRoots.Length];
		for (int j = 0; j < _concealerRoots.Length; j++)
		{
			_concealerRootsBaseScale[j] = _concealerRoots[j].localScale;
		}
		_concealerCoversStartPos = new Vector3[_concealerCovers.Length];
		for (int k = 0; k < _concealerCovers.Length; k++)
		{
			_concealerCoversStartPos[k] = _concealerCovers[k].localPosition;
		}
		UpdateVisuals();
		_lightVolume = this.GetRequiredComponentInChildren<LightSourceVolume>();
	}

	private void Start()
	{
		_lightVolume.LinkLightSource(this);
		_lightVolume.SetVolumeActivation(_lit);
		_lightSourceVolumeShape = _lightVolume.GetComponent<SphereShape>();
		if (_lightSourceVolumeShape == null)
		{
			Debug.LogWarning("Could not find SphereShape for DreamLantern LightSourceVolume. Could not set Max Range for the volume.");
			return;
		}
		SetDetectorScaleCompensation(base.transform.lossyScale);
		SetDetectorPositionAndSize(_minRange, _light.GetLight().spotAngle);
	}

	private void OnEnable()
	{
		_lensFlare.enabled = _lensFlareStrength > 0f;
	}

	private void OnDisable()
	{
		_lensFlare.enabled = false;
	}

	public LightSourceType GetLightSourceType()
	{
		return LightSourceType.DREAM_LANTERN;
	}

	public OWLight2[] GetLights()
	{
		if (_interfaceCompatibleList == null)
		{
			_interfaceCompatibleList = new OWLight2[1] { _light };
		}
		return _interfaceCompatibleList;
	}

	public float GetMinRange()
	{
		return _minRange;
	}

	public float GetMaxRange()
	{
		return _maxRange;
	}

	public float GetMinAngle()
	{
		return _minAngle;
	}

	public float GetMaxAngle()
	{
		return _maxAngle;
	}

	public void SetRange(float minRange, float maxRange)
	{
		if (!OWMath.ApproxEquals(_minRange, minRange) || !OWMath.ApproxEquals(_maxRange, maxRange))
		{
			_minRange = minRange;
			_maxRange = maxRange;
			_dirtyFlag_range = true;
			UpdateVisuals();
		}
	}

	public void SetDetectorScaleCompensation(Vector3 worldScale)
	{
		if (_lightSourceVolumeShape != null)
		{
			_lightSourceVolumeShape.transform.localScale = new Vector3(1f / worldScale.x, 1f / worldScale.y, 1f / worldScale.z);
		}
	}

	private void SetDetectorPositionAndSize(float range, float angle)
	{
		if (_lightSourceVolumeShape != null)
		{
			_lightSourceVolumeShape.center = new Vector3(0f, 0f, range);
			float b = Mathf.Tan(angle * 0.5f * ((float)Math.PI / 180f)) * range;
			_lightSourceVolumeShape.radius = Mathf.Max(range, b);
		}
	}

	public bool IsFocused(float threshold = 0.9f)
	{
		return _focus >= threshold;
	}

	public OWLight2 GetLight()
	{
		return _light;
	}

	public Vector3 GetLightPosition()
	{
		return _light.transform.position;
	}

	public bool CheckIlluminationAtPoint(Vector3 point, float buffer = 0f, float maxDistance = float.PositiveInfinity)
	{
		if (!_concealed)
		{
			return _light.CheckIlluminationAtPoint(point, buffer, maxDistance);
		}
		return false;
	}

	public bool IsLit()
	{
		return _lit;
	}

	public void SetLit(bool lit)
	{
		if (_lit != lit)
		{
			_lit = lit;
			_dirtyFlag_lit = true;
			_lightVolume.SetVolumeActivation(_lit);
			if (lit)
			{
				_litTime = Time.time;
			}
			UpdateVisuals();
		}
	}

	public bool IsConcealed()
	{
		return _concealed;
	}

	public bool IsConcealed(float threshold)
	{
		return _concealment >= threshold;
	}

	public void SetConcealed(bool concealed)
	{
		if (_concealed != concealed)
		{
			_concealed = concealed;
			_dirtyFlag_concealed = true;
			UpdateVisuals();
		}
	}

	public bool IsSocketed()
	{
		return _socketed;
	}

	public void SetSocketed(bool socketed)
	{
		if (_socketed != socketed)
		{
			_socketed = socketed;
			_dirtyFlag_socketed = true;
			UpdateVisuals();
		}
	}

	public bool IsHeldByPlayer()
	{
		return _heldByPlayer;
	}

	public void SetHeldByPlayer(bool heldByPlayer)
	{
		if (_heldByPlayer != heldByPlayer)
		{
			_heldByPlayer = heldByPlayer;
			_dirtyFlag_heldByPlayer = true;
			_flicker.SetSector(null);
			_flicker.enabled = true;
			UpdateVisuals();
		}
	}

	public float GetFocus()
	{
		return _focus;
	}

	public void SetFocus(float focus)
	{
		focus = Mathf.Clamp01(focus);
		if (_focus != focus)
		{
			_focus = focus;
			_dirtyFlag_focus = true;
			UpdateVisuals();
		}
	}

	public void ChangeFocus(float rate)
	{
		SetFocus(_focus + rate * Time.deltaTime);
	}

	public void MoveTowardFocus(float targetFocus, float rate)
	{
		SetFocus(Mathf.MoveTowards(_focus, targetFocus, rate * Time.deltaTime));
	}

	private void Update()
	{
		float num = 0f;
		if (_lit && !_concealed && !_heldByPlayer)
		{
			Vector3 to = Locator.GetActiveCamera().transform.position - _light.transform.position;
			float num2 = 1f;
			if (to.sqrMagnitude > _light.GetLight().range * _light.GetLight().range)
			{
				num2 = 0f;
			}
			else if (Vector3.Angle(_light.transform.forward, to) > _light.GetLight().spotAngle * 0.5f)
			{
				num2 = 0f;
			}
			num = Mathf.MoveTowards(_lensFlare.brightness, _origLensFlareBrightness * num2, Time.deltaTime * 4f);
		}
		if (_lensFlareStrength != num)
		{
			_lensFlareStrength = num;
			_dirtyFlag_lensFlareStrength = true;
		}
		float target = 0f;
		float num3 = 0.1f;
		if (_lit)
		{
			target = (_concealed ? 0f : 1f);
			num3 = ((!(Time.time - _litTime <= 1f)) ? (_concealed ? 0.2f : 0.5f) : 1f);
		}
		float num4 = Mathf.MoveTowards(_flameStrength, target, Time.deltaTime / num3);
		if (_flameStrength != num4)
		{
			_flameStrength = num4;
			_dirtyFlag_flameStrength = true;
		}
		float num5 = Mathf.MoveTowards(_concealment, _concealed ? 1f : 0f, Time.deltaTime / (_concealed ? 0.15f : 0.5f));
		if (_concealment != num5)
		{
			_concealment = num5;
			_dirtyFlag_concealment = true;
		}
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (_dirtyFlag_heldByPlayer)
		{
			if (_worldModelGroup != null)
			{
				_worldModelGroup.SetActive(!_heldByPlayer);
			}
			if (_viewModelGroup != null)
			{
				_viewModelGroup.SetActive(_heldByPlayer);
			}
		}
		if (_dirtyFlag_lit || _dirtyFlag_flameStrength)
		{
			Vector4 value = new Vector4(1f, 1f, 0f, 0f);
			value.w = Mathf.Lerp(0.5f, 0f, _flameStrength);
			for (int i = 0; i < _flameRenderers.Length; i++)
			{
				_flameRenderers[i].SetActivation(_lit || _flameStrength > 0f);
				_flameRenderers[i].SetMaterialProperty(_propID_MainTex_ST, value);
			}
		}
		if (_dirtyFlag_lensFlareStrength)
		{
			_lensFlare.brightness = _lensFlareStrength;
			_lensFlare.enabled = _lensFlareStrength > 0f;
		}
		if (_dirtyFlag_focus)
		{
			Vector3 vector = new Vector3(0f, 0f, Mathf.Lerp(90f, 0f, _focus));
			for (int j = 0; j < _focuserPetals.Length; j++)
			{
				_focuserPetals[j].localEulerAngles = _focuserPetalsBaseEulerAngles[j] + vector;
			}
		}
		if (_dirtyFlag_concealment)
		{
			Vector3 b = new Vector3(1f, Mathf.Lerp(0.5f, 1f, _concealment), 1f);
			for (int k = 0; k < _concealerRoots.Length; k++)
			{
				_concealerRoots[k].localScale = Vector3.Scale(_concealerRootsBaseScale[k], b);
			}
			for (int l = 0; l < _concealerCovers.Length; l++)
			{
				_concealerCovers[l].localPosition = Vector3.Lerp(_concealerCoverTargets[l], _concealerCoversStartPos[l], _concealment);
				_concealerCoversVMPrepass[l].localPosition = Vector3.Lerp(_concealerCoverTargets[l], _concealerCoversStartPos[l], _concealment);
			}
		}
		if (_dirtyFlag_flameStrength)
		{
			bool activation = _flameStrength > 0f;
			_light.SetActivation(activation);
		}
		if (_dirtyFlag_focus || _dirtyFlag_flameStrength || _dirtyFlag_range)
		{
			float range = Mathf.Lerp(_minRange, _maxRange, Mathf.Pow(_focus, 5f)) * _flameStrength;
			float num = Mathf.Lerp(_maxAngle, _minAngle, _focus);
			_light.range = range;
			_light.GetLight().spotAngle = num;
			SetDetectorPositionAndSize(range, num);
		}
		if (_grabbedByGhost)
		{
			float intensity = Mathf.MoveTowards(_light.GetIntensity(), 1.2f, Time.deltaTime * 0.2f);
			_light.SetIntensity(intensity);
		}
		else if (_dirtyFlag_socketed || _dirtyFlag_grabbedByGhost)
		{
			_light.SetIntensity(_socketed ? 0f : 1f);
		}
		if (_dirtyFlag_flameStrength)
		{
			for (int m = 0; m < _flameLights.Length; m++)
			{
				_flameLights[m].SetActivation(_flameStrength > 0f);
				_flameLights[m].SetIntensityScale(_flameStrength);
			}
		}
		if ((_dirtyFlag_focus || _dirtyFlag_lit || _dirtyFlag_concealed) && _simLightConeUnfocused != null && _simLightConeFocused != null)
		{
			bool flag = IsFocused();
			_simLightConeUnfocused.SetActive(_lit && !_concealed && !flag);
			_simLightConeFocused.SetActive(_lit && !_concealed && flag);
		}
		ClearDirtyFlags();
	}

	private void ClearDirtyFlags()
	{
		_dirtyFlag_range = false;
		_dirtyFlag_focus = false;
		_dirtyFlag_flameStrength = false;
		_dirtyFlag_concealment = false;
		_dirtyFlag_lensFlareStrength = false;
		_dirtyFlag_lit = false;
		_dirtyFlag_concealed = false;
		_dirtyFlag_heldByPlayer = false;
		_dirtyFlag_socketed = false;
		_dirtyFlag_grabbedByGhost = false;
	}
}
