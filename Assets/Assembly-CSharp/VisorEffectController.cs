using UnityEngine;

public class VisorEffectController : MonoBehaviour
{
	[SerializeField]
	private OWCamera _owCamera;

	[SerializeField]
	private VisorEffectDetector _visorEffectDetector;

	[SerializeField]
	private Renderer _visorEffectRenderer;

	[Space]
	[SerializeField]
	private ParticleSystem _rainDropletsParticleSystem;

	[SerializeField]
	private ParticleSystem _rainStreaksParticleSystem;

	[Space]
	[SerializeField]
	private float _dirtClearLength = 1f;

	[SerializeField]
	private float _equilibriumDirt = 50f;

	[SerializeField]
	private float _maxDirt = 100f;

	[SerializeField]
	private float _dirtDecayRate = 10f;

	[Space]
	[SerializeField]
	private float _waterFadeInLength = 0.25f;

	[SerializeField]
	private float _waterClearLength = 1f;

	[SerializeField]
	private float _waterOffsetStart = -0.5f;

	[SerializeField]
	private float _waterOffsetEnd = 1f;

	[SerializeField]
	private int _waterNumDroplets = 16;

	[SerializeField]
	private float _waterNumStreaks = 8f;

	[Space]
	[SerializeField]
	private float _frostThawRate = 0.1f;

	[SerializeField]
	private float _defrostRate = 1f;

	[Space]
	[SerializeField]
	private PlayerCameraFluidDetector _cameraFluidDetector;

	[SerializeField]
	private AnimationCurve _breathFogCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private float _breathFogRandomness = 0.1f;

	[SerializeField]
	private float _breathFogFadeInLength = 30f;

	[SerializeField]
	private float _breathFogFadeOutLength = 1f;

	[Space]
	[SerializeField]
	private Renderer _crackEffectRenderer;

	[SerializeField]
	private AnimationCurve _impactCrackAnimCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _crushedCrackAnimCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private ParticleSystem.EmissionModule _rainDropletsEmission;

	private ParticleSystem.EmissionModule _rainStreaksEmission;

	private Vector3 _rainDropletsInitShapeScale;

	private Vector3 _rainStreaksInitShapeScale;

	private float _dirt;

	private bool _clearingDirt;

	private float _dirtClearTimer;

	private float _dirtClearValue;

	private int _propID_WaterTex;

	private int _propID_WaterCutoff;

	private bool _isUnderwater;

	private float _waterClearTimer;

	private float _waterRandomOffset;

	private int _propID_FrostRamp;

	private float _frost;

	private bool _defrosting;

	private bool _breathFogEnabled;

	private float _breathFogFade;

	private int _propID_Cutoff;

	private bool _cracked;

	private float _crackStartTime;

	private AnimationCurve _crackAnimCurve;

	private void Awake()
	{
		_rainDropletsEmission = _rainDropletsParticleSystem.emission;
		_rainStreaksEmission = _rainStreaksParticleSystem.emission;
		_rainDropletsInitShapeScale = _rainDropletsParticleSystem.shape.scale;
		_rainStreaksInitShapeScale = _rainStreaksParticleSystem.shape.scale;
		_rainDropletsParticleSystem.Stop();
		_rainStreaksParticleSystem.Stop();
		_rainDropletsEmission.rateOverTimeMultiplier = 0f;
		_rainStreaksEmission.rateOverTimeMultiplier = 0f;
		_dirt = 0f;
		_clearingDirt = false;
		_dirtClearTimer = 0f;
		_dirtClearValue = 0f;
		_propID_WaterTex = Shader.PropertyToID("_WaterTex");
		_propID_WaterCutoff = Shader.PropertyToID("_WaterCutoff");
		_isUnderwater = false;
		_waterClearTimer = _waterClearLength;
		_waterRandomOffset = 0f;
		_propID_FrostRamp = Shader.PropertyToID("_FrostMaskRamp");
		_frost = 0f;
		_breathFogEnabled = false;
		_breathFogFade = 0f;
		_propID_Cutoff = Shader.PropertyToID("_Cutoff");
		_cracked = false;
		_crackStartTime = 0f;
		_visorEffectRenderer.enabled = false;
		_visorEffectRenderer.material.SetTextureOffset(_propID_WaterTex, new Vector2(_waterRandomOffset, _waterOffsetEnd));
		_crackEffectRenderer.enabled = false;
		_crackEffectRenderer.material.SetFloat(_propID_Cutoff, 1f);
		GlobalMessenger<float>.AddListener("PlayerCameraEnterWater", OnCameraEnterWater);
		GlobalMessenger.AddListener("PlayerCameraExitWater", OnCameraExitWater);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.AddListener("BigBangHelmetCrack", OnBigBangHelmetCrack);
		GlobalMessenger<GraphicSettings>.AddListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
		SecretSettings.TryGetBool("EnableVisorBreathFog", out _breathFogEnabled);
	}

	private void Start()
	{
		float num = (float)Screen.width / (float)Screen.height;
		if (!OWMath.ApproxEquals(num, 1.777f, 0.01f))
		{
			Vector3 b = new Vector3(num / 1.7777778f, 1f, 1f);
			ParticleSystem.ShapeModule shape = _rainDropletsParticleSystem.shape;
			shape.scale = Vector3.Scale(_rainDropletsInitShapeScale, b);
			ParticleSystem.ShapeModule shape2 = _rainStreaksParticleSystem.shape;
			shape2.scale = Vector3.Scale(_rainStreaksInitShapeScale, b);
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger<float>.RemoveListener("PlayerCameraEnterWater", OnCameraEnterWater);
		GlobalMessenger.RemoveListener("PlayerCameraExitWater", OnCameraExitWater);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.RemoveListener("BigBangHelmetCrack", OnBigBangHelmetCrack);
		GlobalMessenger<GraphicSettings>.RemoveListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
	}

	private void LateUpdate()
	{
		_rainDropletsEmission.rateOverTimeMultiplier = _visorEffectDetector.netRainDropletRate;
		_rainStreaksEmission.rateOverTimeMultiplier = _visorEffectDetector.netRainStreakRate;
		if (_clearingDirt)
		{
			_dirtClearTimer = Mathf.Clamp01(_dirtClearTimer + Time.deltaTime / _dirtClearLength);
			_dirt = Mathf.Lerp(_dirtClearValue, 0f, _dirtClearTimer);
			if (_dirtClearTimer >= 1f)
			{
				_clearingDirt = false;
			}
		}
		else
		{
			_dirt += _visorEffectDetector.netDirtRate * Time.deltaTime;
			if (_dirt > _equilibriumDirt)
			{
				_dirt -= _dirtDecayRate * Time.deltaTime;
			}
			_dirt = Mathf.Clamp(_dirt, 0f, _maxDirt);
		}
		_owCamera.postProcessingSettings.lensDirt.intensity = _dirt;
		_waterClearTimer = Mathf.Min(_waterClearTimer + Time.deltaTime, _waterClearLength);
		float num = Mathf.Clamp01(_waterClearTimer / _waterClearLength);
		float num2 = Mathf.Clamp01(_waterClearTimer / _waterFadeInLength);
		_visorEffectRenderer.material.SetTextureOffset(_propID_WaterTex, new Vector2(_waterRandomOffset, Mathf.Lerp(_waterOffsetStart, _waterOffsetEnd, num)));
		_visorEffectRenderer.material.SetFloat(_propID_WaterCutoff, Mathf.Max(num, 1f - num2));
		_rainStreaksEmission.rateOverTimeMultiplier += (1f - num) * _waterNumStreaks;
		if (PlayerState.IsInsideShip() || _isUnderwater)
		{
			_frost = Mathf.Max(_frost - _defrostRate * Time.deltaTime, 0f);
		}
		else if (_visorEffectDetector.frostRate > 0f && _frost <= _visorEffectDetector.maxFrost)
		{
			_frost = Mathf.Min(_frost + _visorEffectDetector.frostRate * Time.deltaTime, _visorEffectDetector.maxFrost);
		}
		else
		{
			_frost = Mathf.Max(_frost - _frostThawRate * Time.deltaTime, 0f);
		}
		_visorEffectRenderer.material.SetFloat(_propID_FrostRamp, _frost);
		_owCamera.postProcessingSettings.frost.frostRamp = _frost;
		if ((num < 1f || _frost > 0f) && !_visorEffectRenderer.enabled)
		{
			_visorEffectRenderer.enabled = true;
		}
		if (num >= 1f && _frost <= 0f && _visorEffectRenderer.enabled)
		{
			_visorEffectRenderer.enabled = false;
		}
		if (!_breathFogEnabled || _cameraFluidDetector.InFluidType(FluidVolume.Type.AIR) || Locator.GetToolModeSwapper().GetToolMode() == ToolMode.SignalScope || Locator.GetToolModeSwapper().GetToolMode() == ToolMode.Translator || GUIMode.IsCaptureMode() || GUIMode.IsHiddenMode())
		{
			_breathFogFade = Mathf.Max(_breathFogFade - Time.deltaTime / _breathFogFadeOutLength, 0f);
		}
		else
		{
			_breathFogFade = Mathf.Min(_breathFogFade + Time.deltaTime / _breathFogFadeInLength, 1f);
		}
		float num3 = 1f - (Mathf.Sin(Time.timeSinceLevelLoad) * 0.5f + 0.5f) * _breathFogRandomness;
		float breathFogRamp = _breathFogCurve.Evaluate(Time.timeSinceLevelLoad) * num3 * _breathFogFade;
		_owCamera.postProcessingSettings.breathFog.breathFogRamp = breathFogRamp;
		if (_cracked)
		{
			_crackEffectRenderer.material.SetFloat(_propID_Cutoff, _crackAnimCurve.Evaluate(Time.time - _crackStartTime));
		}
		if (_rainDropletsEmission.rateOverTimeMultiplier > 0f && !_rainDropletsParticleSystem.isPlaying)
		{
			_rainDropletsParticleSystem.Play();
		}
		else if (_rainDropletsEmission.rateOverTimeMultiplier <= 0f && _rainDropletsParticleSystem.isPlaying)
		{
			_rainDropletsParticleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		}
		if (_rainStreaksEmission.rateOverTimeMultiplier > 0f && !_rainStreaksParticleSystem.isPlaying)
		{
			_rainStreaksParticleSystem.Play();
		}
		else if (_rainStreaksEmission.rateOverTimeMultiplier <= 0f && _rainStreaksParticleSystem.isPlaying)
		{
			_rainStreaksParticleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		}
	}

	private void OnCameraEnterWater(float relativeSpeed)
	{
		_isUnderwater = true;
	}

	private void OnCameraExitWater()
	{
		_isUnderwater = false;
		if (!Locator.GetPlayerSuit().IsWearingHelmet())
		{
			return;
		}
		if (_waterClearTimer >= _waterClearLength)
		{
			if (!_rainDropletsParticleSystem.isPlaying)
			{
				_rainDropletsParticleSystem.Play();
			}
			_rainDropletsParticleSystem.Emit(_waterNumDroplets);
		}
		_waterClearTimer = 0f;
		_waterRandomOffset = Random.value;
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		if (Locator.GetDeathManager().GetCrackHelmetOnDeath())
		{
			_cracked = true;
			_crackStartTime = Time.time;
			_crackEffectRenderer.enabled = true;
			_crackAnimCurve = ((deathType == DeathType.Crushed) ? _crushedCrackAnimCurve : _impactCrackAnimCurve);
		}
	}

	private void OnBigBangHelmetCrack()
	{
		_cracked = true;
		_crackStartTime = Time.time;
		_crackEffectRenderer.enabled = true;
		_crackAnimCurve = _impactCrackAnimCurve;
	}

	private void OnGraphicSettingsUpdated(GraphicSettings graphicSettings)
	{
		float num = (float)graphicSettings.displayResWidth / (float)graphicSettings.displayResHeight;
		if (!OWMath.ApproxEquals(num, 1.777f, 0.01f))
		{
			Vector3 b = new Vector3(num / 1.7777778f, 1f, 1f);
			ParticleSystem.ShapeModule shape = _rainDropletsParticleSystem.shape;
			shape.scale = Vector3.Scale(shape.scale, b);
			ParticleSystem.ShapeModule shape2 = _rainStreaksParticleSystem.shape;
			shape2.scale = Vector3.Scale(shape2.scale, b);
		}
		else
		{
			ParticleSystem.ShapeModule shape3 = _rainDropletsParticleSystem.shape;
			shape3.scale = _rainDropletsInitShapeScale;
			ParticleSystem.ShapeModule shape4 = _rainStreaksParticleSystem.shape;
			shape4.scale = _rainStreaksInitShapeScale;
		}
	}

	public void ClearVisorDirt()
	{
		if (!_clearingDirt)
		{
			_clearingDirt = true;
			_dirtClearTimer = 0f;
			_dirtClearValue = _dirt;
		}
	}

	public float GetDirtFraction()
	{
		return _dirt / _maxDirt;
	}
}
