using UnityEngine;

public class Marshmallow : MonoBehaviour
{
	public enum MallowState
	{
		Default = 0,
		Burning = 1,
		Shriveling = 2,
		Gone = 3
	}

	public const float RAW_TOASTED_FRACTION = 0.2f;

	public const float PERFECT_TOASTED_FRACTION = 0.7f;

	public const float HEAT_CAPACITY = 300f;

	public const float BURN_THRESHOLD = 25f;

	public const float BURN_DURATION = 5f;

	[SerializeField]
	private MeshRenderer _fireRenderer;

	[SerializeField]
	private ParticleSystem _smokeParticles;

	[SerializeField]
	private MeshRenderer _mallowRenderer;

	[SerializeField]
	private Color _rawColor;

	[SerializeField]
	private Color _toastedColor;

	[SerializeField]
	private Color _burntColor;

	private PlayerAudioController _audioController;

	private MallowState _mallowState;

	private ParticleSystem.MainModule _smokeParticlesSettings;

	private float _heatPerSecond;

	private float _toastedFraction;

	private float _initBurnTime;

	private float _initShrivelTime;

	private SunController _sunController;

	private void Awake()
	{
		_smokeParticlesSettings = _smokeParticles.main;
		_fireRenderer.enabled = false;
		_smokeParticles.Stop();
	}

	private void Start()
	{
		if (Locator.GetSunTransform() != null)
		{
			_sunController = Locator.GetSunTransform().GetComponent<SunController>();
		}
	}

	public void SpawnMallow(bool playSFX)
	{
		base.transform.localPosition = new Vector3(0f, 0f, 0.3f);
		base.transform.localScale = Vector3.one;
		_smokeParticles.Stop();
		_fireRenderer.enabled = false;
		_toastedFraction = 0f;
		_mallowRenderer.enabled = true;
		_mallowState = MallowState.Default;
		if (_audioController == null)
		{
			_audioController = Locator.GetPlayerTransform().GetComponentInChildren<PlayerAudioController>();
		}
		if (playSFX)
		{
			_audioController.PlayMarshmallowReplace();
		}
		base.enabled = true;
	}

	public void Disable()
	{
		_smokeParticles.Stop();
		_fireRenderer.enabled = false;
		base.enabled = false;
	}

	public void SetRawColor(Color color)
	{
		_rawColor = color;
	}

	public void SetTexture(Texture2D texture)
	{
		_mallowRenderer.material.SetTexture("_MainTex", texture);
	}

	public void SetSmoothness(float smoothness)
	{
		_mallowRenderer.material.SetFloat("_Glossiness", smoothness);
	}

	public Color GetBurntColor()
	{
		return _burntColor;
	}

	public bool IsEdible()
	{
		if (_mallowState == MallowState.Default)
		{
			return _toastedFraction > 0.2f;
		}
		return false;
	}

	public bool IsBurned()
	{
		return _toastedFraction >= 1f;
	}

	public MallowState GetState()
	{
		return _mallowState;
	}

	public void Eat()
	{
		GlobalMessenger<float>.FireEvent("EatMarshmallow", _toastedFraction);
		RemoveMallow();
		if (IsBurned())
		{
			_audioController.PlayMarshmallowEatBurnt();
		}
		else
		{
			_audioController.PlayMarshmallowEat();
		}
	}

	public void Remove()
	{
		RemoveMallow();
	}

	public void Extinguish()
	{
		if (_mallowState == MallowState.Burning)
		{
			_fireRenderer.enabled = false;
			_mallowState = MallowState.Default;
			_audioController.PlayMarshmallowBlowOut();
		}
	}

	public void UpdateRoast(Campfire campfire)
	{
		UpdateHeatExposure(campfire);
		UpdateVisuals();
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Vector3.zero, Time.deltaTime);
		if (_mallowState != 0)
		{
			if (_mallowState == MallowState.Burning)
			{
				if (Time.time > _initBurnTime + 5f)
				{
					Shrivel();
				}
			}
			else if (_mallowState == MallowState.Shriveling)
			{
				float num = Mathf.Clamp01((Time.time - _initShrivelTime) / 2f);
				if ((double)num >= 0.5 && _fireRenderer.enabled)
				{
					_fireRenderer.enabled = false;
				}
				if (num >= 1f)
				{
					RemoveMallow();
				}
				base.transform.localScale = Vector3.one * (1f - num);
			}
		}
		_heatPerSecond = 0f;
	}

	private void UpdateHeatExposure(Campfire campfire)
	{
		if (_sunController != null && _sunController.HasSupernovaStarted() && _sunController.GetDistanceToSupernova(base.transform.position) < 2000f)
		{
			Burn();
			return;
		}
		_heatPerSecond = campfire.GetHeatAtPosition(base.transform.position);
		_toastedFraction += _heatPerSecond * Time.deltaTime / 300f;
		_toastedFraction = Mathf.Clamp01(_toastedFraction);
		if (_heatPerSecond > 25f)
		{
			Burn();
		}
	}

	private void UpdateVisuals()
	{
		float num = _heatPerSecond / 25f;
		if (num > 0f)
		{
			if (!_smokeParticles.isEmitting)
			{
				_smokeParticles.Play();
			}
			Color color = new Color(1f, 1f, 1f, num);
			_smokeParticlesSettings.startColor = color;
		}
		else if (_smokeParticles.isEmitting)
		{
			_smokeParticles.Stop();
		}
		Color b;
		if (_toastedFraction < 0.7f)
		{
			float t = _toastedFraction / 0.7f;
			b = Color.Lerp(_rawColor, _toastedColor, t);
		}
		else
		{
			float t = (_toastedFraction - 0.7f) / 0.3f;
			b = Color.Lerp(_toastedColor, _burntColor, t);
		}
		_mallowRenderer.material.color = Color.Lerp(_mallowRenderer.material.color, b, 0.2f);
		_smokeParticles.transform.forward = Locator.GetPlayerTransform().up;
	}

	private void Burn()
	{
		if (_mallowState == MallowState.Default)
		{
			_fireRenderer.enabled = true;
			_toastedFraction = 1f;
			_initBurnTime = Time.time;
			_mallowState = MallowState.Burning;
			_audioController.PlayMarshmallowCatchFire();
		}
	}

	private void Shrivel()
	{
		if (_mallowState == MallowState.Burning)
		{
			_initShrivelTime = Time.time;
			_mallowState = MallowState.Shriveling;
		}
	}

	private void RemoveMallow()
	{
		_smokeParticles.Stop();
		_fireRenderer.enabled = false;
		_mallowRenderer.enabled = false;
		_mallowState = MallowState.Gone;
		base.enabled = false;
	}
}
