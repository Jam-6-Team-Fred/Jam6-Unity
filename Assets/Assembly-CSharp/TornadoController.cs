using UnityEngine;

public class TornadoController : SectoredMonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _underwaterVolume;

	[Space]
	[SerializeField]
	private bool _snapBonesToSphere = true;

	[SerializeField]
	private bool _wander;

	[SerializeField]
	private float _wanderRate = 0.02f;

	[SerializeField]
	private float _wanderDegreesX = 45f;

	[SerializeField]
	private float _wanderDegreesZ = 20f;

	[Space]
	[SerializeField]
	private bool _startActive = true;

	[SerializeField]
	private GameObject _tornadoRoot;

	[SerializeField]
	private FluidVolume[] _fluids;

	[SerializeField]
	private OWTriggerVolume _collapseTrigger;

	[SerializeField]
	private OWAudioSource _audioSource;

	[Space]
	[SerializeField]
	private Transform _topBone;

	[SerializeField]
	private Transform _midBone;

	[SerializeField]
	private Transform _bottomBone;

	[SerializeField]
	private Renderer[] _topBlendRenderers;

	[SerializeField]
	private Renderer[] _bodyRenderers;

	[SerializeField]
	private Renderer[] _bottomBlendRenderers;

	[Space]
	[SerializeField]
	private float _topFadeTime = 0.3f;

	[SerializeField]
	private float _bodyFadeTime = 1f;

	[SerializeField]
	private float _bottomFadeTime = 0.3f;

	[Space]
	[SerializeField]
	private float _topScaleAmplitude = 0.3f;

	[SerializeField]
	private float _topScaleFrequency = 0.1f;

	[SerializeField]
	private float _midScaleAmplitude = 0.3f;

	[SerializeField]
	private float _midScaleFrequency = 0.1f;

	[SerializeField]
	private float _bottomScaleAmplitude = 0.3f;

	[SerializeField]
	private float _bottomScaleFrequency = 0.1f;

	[Space]
	[SerializeField]
	private float _rotationSpeed = 45f;

	[SerializeField]
	private float _midRotationAmplitude = 20f;

	[SerializeField]
	private Vector2 _midRotationXZFrequency = new Vector2(0.1f, 0.15f);

	private Vector3 _topStartPos;

	private Vector3 _midStartPos;

	private Vector3 _bottomStartPos;

	private Vector3 _topBasePos;

	private Vector3 _midBasePos;

	private Vector3 _bottomBasePos;

	private float _topElevation;

	private float _midElevation;

	private float _midStartElevation;

	private float _bottomElevation;

	private float _bottomStartElevation;

	private float _animTimeOffset;

	private bool _isSectorOccupied;

	private bool _isPlayerUnderwater;

	private bool _isPlayerAwayWhenFormed;

	private float _secondsUntilFormation;

	private bool _tornadoCollapsing;

	private bool _tornadoForming;

	private float _formationDuration = 12f;

	private float _collapseDuration = 12f;

	private float _collapseTime;

	private float _formationTime;

	private MaterialPropertyBlock _matPropBlock;

	private int _propID_CutoffFade;

	protected override void Awake()
	{
		base.Awake();
		if (!_startActive)
		{
			_tornadoRoot.SetActive(value: false);
			Vector3 localScale = _tornadoRoot.transform.localScale;
			localScale.x = 0f;
			localScale.z = 0f;
			_tornadoRoot.transform.localScale = localScale;
			_secondsUntilFormation = Random.Range(0.5f, 60f);
		}
		_topStartPos = _topBone.localPosition;
		_midStartPos = _midBone.localPosition;
		_bottomStartPos = _bottomBone.localPosition;
		_topElevation = Vector3.Distance(_topBone.position, base.transform.position);
		_midElevation = (_midStartElevation = Vector3.Distance(_midBone.position, base.transform.position));
		_bottomElevation = (_bottomStartElevation = Vector3.Distance(_bottomBone.position, base.transform.position));
		_animTimeOffset = Random.Range(-100f, 100f);
		_matPropBlock = new MaterialPropertyBlock();
		_propID_CutoffFade = Shader.PropertyToID("_CutoffFade");
		_collapseTrigger.OnEntry += OnEnterCollapseTrigger;
		if (_underwaterVolume != null)
		{
			_underwaterVolume.OnEntry += OnEnterUnderwater;
			_underwaterVolume.OnExit += OnExitUnderwater;
		}
	}

	private void Start()
	{
		if (_startActive)
		{
			_audioSource.SetLocalVolume(0f);
			_tornadoRoot.SetActive(value: true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_collapseTrigger.OnEntry -= OnEnterCollapseTrigger;
		if (_underwaterVolume != null)
		{
			_underwaterVolume.OnEntry -= OnEnterUnderwater;
			_underwaterVolume.OnExit -= OnExitUnderwater;
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool isSectorOccupied = _isSectorOccupied;
		_isSectorOccupied = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (!isSectorOccupied && _isSectorOccupied)
		{
			AttemptAudioFadeIn(0.5f);
			if (_isPlayerAwayWhenFormed)
			{
				_isPlayerAwayWhenFormed = false;
				_matPropBlock.SetFloat(_propID_CutoffFade, 0f);
				for (int i = 0; i < _topBlendRenderers.Length; i++)
				{
					_topBlendRenderers[i].SetPropertyBlock(_matPropBlock);
				}
				for (int j = 0; j < _bodyRenderers.Length; j++)
				{
					_bodyRenderers[j].SetPropertyBlock(_matPropBlock);
				}
				for (int k = 0; k < _bottomBlendRenderers.Length; k++)
				{
					_bottomBlendRenderers[k].SetPropertyBlock(_matPropBlock);
				}
			}
		}
		else if (isSectorOccupied && !_isSectorOccupied)
		{
			_audioSource.FadeOut(0.5f);
		}
	}

	private void StartFormation()
	{
		_secondsUntilFormation = 0f;
		_tornadoForming = true;
		_formationTime = Time.time;
		_tornadoRoot.SetActive(value: true);
		for (int i = 0; i < _fluids.Length; i++)
		{
			_fluids[i].SetVolumeActivation(active: true);
		}
		if (_isSectorOccupied)
		{
			_matPropBlock.SetFloat(_propID_CutoffFade, 1f);
			for (int j = 0; j < _topBlendRenderers.Length; j++)
			{
				_topBlendRenderers[j].SetPropertyBlock(_matPropBlock);
			}
			for (int k = 0; k < _bodyRenderers.Length; k++)
			{
				_bodyRenderers[k].SetPropertyBlock(_matPropBlock);
			}
			for (int l = 0; l < _bottomBlendRenderers.Length; l++)
			{
				_bottomBlendRenderers[l].SetPropertyBlock(_matPropBlock);
			}
			AttemptAudioFadeIn(_formationDuration);
		}
	}

	private void StartCollapse()
	{
		if (_secondsUntilFormation <= 0f)
		{
			_tornadoCollapsing = true;
			_collapseTime = Time.time;
			_tornadoForming = false;
			if (_isSectorOccupied)
			{
				_audioSource.FadeOut(_collapseDuration);
			}
			else
			{
				_isPlayerAwayWhenFormed = false;
			}
		}
	}

	private void OnEnterCollapseTrigger(GameObject hitObject)
	{
		if (hitObject.GetComponentInParent<OWRigidbody>().GetMass() > 50f)
		{
			StartCollapse();
		}
	}

	private void OnEnterUnderwater(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerCameraDetector"))
		{
			_isPlayerUnderwater = true;
			_audioSource.FadeOut(2f);
		}
	}

	private void OnExitUnderwater(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerCameraDetector"))
		{
			_isPlayerUnderwater = false;
			AttemptAudioFadeIn(2f);
		}
	}

	private void AttemptAudioFadeIn(float duration)
	{
		if (_tornadoRoot.activeInHierarchy && !_tornadoCollapsing && !_isPlayerUnderwater && _isSectorOccupied)
		{
			_audioSource.FadeIn(duration);
		}
	}

	private void FixedUpdate()
	{
		if (_secondsUntilFormation > 0f)
		{
			_secondsUntilFormation -= Time.fixedDeltaTime;
			if (_secondsUntilFormation < 0f)
			{
				StartFormation();
			}
			return;
		}
		if (_tornadoCollapsing)
		{
			UpdateCollapse();
		}
		else if (_tornadoForming)
		{
			UpdateFormation();
		}
		if (_isSectorOccupied)
		{
			UpdateAnimation();
			if (_wander)
			{
				float num = Mathf.PerlinNoise(Time.time * _wanderRate, 0f) * 2f - 1f;
				float num2 = Mathf.PerlinNoise(Time.time * _wanderRate, 5f) * 2f - 1f;
				Vector3 localEulerAngles = base.transform.localEulerAngles;
				localEulerAngles = new Vector3(num * _wanderDegreesX, 0f, num2 * _wanderDegreesZ);
				base.transform.localEulerAngles = localEulerAngles;
			}
		}
	}

	private void UpdateFormation()
	{
		float t = Mathf.Clamp01((Time.time - _formationTime) / _formationDuration);
		t = Mathf.SmoothStep(0f, 1f, t);
		_tornadoRoot.transform.localScale = Vector3.Lerp(new Vector3(0f, 1f, 0f), Vector3.one, t);
		_midElevation = Mathf.Lerp(_topElevation, _midStartElevation, t);
		_bottomElevation = Mathf.Lerp(_topElevation, _bottomStartElevation, t);
		if (_isSectorOccupied)
		{
			_matPropBlock.SetFloat(_propID_CutoffFade, Mathf.Clamp01(1f - t / _topFadeTime));
			for (int i = 0; i < _topBlendRenderers.Length; i++)
			{
				_topBlendRenderers[i].SetPropertyBlock(_matPropBlock);
			}
			_matPropBlock.SetFloat(_propID_CutoffFade, Mathf.Clamp01((1f - t) / _bodyFadeTime));
			for (int j = 0; j < _bodyRenderers.Length; j++)
			{
				_bodyRenderers[j].SetPropertyBlock(_matPropBlock);
			}
			_matPropBlock.SetFloat(_propID_CutoffFade, Mathf.Clamp01((1f - t) / _bottomFadeTime));
			for (int k = 0; k < _bottomBlendRenderers.Length; k++)
			{
				_bottomBlendRenderers[k].SetPropertyBlock(_matPropBlock);
			}
		}
		if (OWMath.ApproxEquals(t, 1f))
		{
			if (!_isSectorOccupied)
			{
				_isPlayerAwayWhenFormed = true;
			}
			_tornadoForming = false;
			_tornadoRoot.transform.localScale = Vector3.one;
		}
	}

	private void UpdateCollapse()
	{
		float t = Mathf.Clamp01((Time.time - _collapseTime) / _collapseDuration);
		t = Mathf.SmoothStep(0f, 1f, t);
		_tornadoRoot.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0f, 1f, 0f), t);
		if (_isSectorOccupied)
		{
			_matPropBlock.SetFloat(_propID_CutoffFade, Mathf.Clamp01(t / _bodyFadeTime));
			for (int i = 0; i < _topBlendRenderers.Length; i++)
			{
				_topBlendRenderers[i].SetPropertyBlock(_matPropBlock);
			}
			for (int j = 0; j < _bodyRenderers.Length; j++)
			{
				_bodyRenderers[j].SetPropertyBlock(_matPropBlock);
			}
			for (int k = 0; k < _bottomBlendRenderers.Length; k++)
			{
				_bottomBlendRenderers[k].SetPropertyBlock(_matPropBlock);
			}
		}
		if (OWMath.ApproxEquals(t, 1f))
		{
			_tornadoCollapsing = false;
			_tornadoRoot.SetActive(value: false);
			_secondsUntilFormation = 20f + Random.Range(20f, 80f);
			for (int l = 0; l < _fluids.Length; l++)
			{
				_fluids[l].SetVolumeActivation(active: false);
			}
		}
	}

	private void UpdateAnimation()
	{
		float num = Time.time + _animTimeOffset;
		_topBone.localPosition = _topStartPos + Spiro3D(num * 0.023f, 1.7f, 0.23f, 0.7f, 0f) * 7f;
		_midBone.localPosition = _midStartPos + Spiro3D(num * 0.03f, 1.9f, 0.3f, 1.1f, 0f) * 11f;
		_bottomBone.localPosition = _bottomStartPos + Spiro3D(num * 0.05f, 2.1f, 0.4f, 1.2f, 0f) * 13f;
		if (_snapBonesToSphere)
		{
			Vector3 toDirection = _topBone.position - base.transform.position;
			_topBone.position = base.transform.position + toDirection.normalized * _topElevation;
			Vector3 vector = _midBone.position - base.transform.position;
			_midBone.position = base.transform.position + vector.normalized * _midElevation;
			Vector3 toDirection2 = _bottomBone.position - base.transform.position;
			_bottomBone.position = base.transform.position + toDirection2.normalized * _bottomElevation;
			_topBone.rotation = Quaternion.FromToRotation(_topBone.up, toDirection) * _topBone.rotation;
			_bottomBone.rotation = Quaternion.FromToRotation(_bottomBone.up, toDirection2) * _bottomBone.rotation;
		}
		Quaternion quaternion = Quaternion.Euler(Mathf.Sin(num * _midRotationXZFrequency.x) * _midRotationAmplitude, 0f, Mathf.Sin(num * _midRotationXZFrequency.y) * _midRotationAmplitude);
		Quaternion quaternion2 = Quaternion.AngleAxis(num * _rotationSpeed, Vector3.up);
		_topBone.localRotation = quaternion2;
		_midBone.localRotation = quaternion2 * quaternion;
		_bottomBone.localRotation = quaternion2;
		_topBone.localScale = Vector3.one + new Vector3(_topScaleAmplitude * Mathf.Sin(num * _topScaleFrequency), 0f, _topScaleAmplitude * Mathf.Sin(num * _topScaleFrequency));
		_midBone.localScale = Vector3.one + new Vector3(_midScaleAmplitude * Mathf.Sin(num * _midScaleFrequency), 0f, _midScaleAmplitude * Mathf.Sin(num * _midScaleFrequency));
		_bottomBone.localScale = Vector3.one + new Vector3(_bottomScaleAmplitude * Mathf.Sin(num * _bottomScaleFrequency), 0f, _bottomScaleAmplitude * Mathf.Sin(num * _bottomScaleFrequency));
	}

	private Vector2 Spiro2D(float t, float R, float r, float d)
	{
		float num = Mathf.Sin(t);
		float num2 = Mathf.Cos(t);
		float num3 = Mathf.Sin((R - r) / r * t);
		float num4 = Mathf.Sin((R - r) / r * t);
		float x = (R - r) * num2 + d * num4;
		float y = (R - r) * num - d * num3;
		return new Vector2(x, y);
	}

	private Vector3 Spiro3D(float t, float R, float r, float d, float a)
	{
		float num = Mathf.Sin(t);
		float num2 = Mathf.Cos(t);
		float num3 = Mathf.Sin((R - r) / r * t);
		float num4 = Mathf.Sin((R - r) / r * t);
		float num5 = Mathf.Sin(a * t);
		float num6 = Mathf.Cos(a * t);
		float x = (R - r) * num2 + d * num4;
		float num7 = (R - r) * num - d * num3;
		float y = num7 * num5;
		num7 *= num6;
		return new Vector3(x, y, num7);
	}
}
