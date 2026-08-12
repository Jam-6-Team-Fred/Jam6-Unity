using UnityEngine;

public class ExplosionController : MonoBehaviour
{
	[SerializeField]
	private RadialForceVolume _forceVolume;

	[SerializeField]
	private MeshRenderer _renderer;

	[SerializeField]
	private Light _light;

	[Space]
	[SerializeField]
	private float _length = 1f;

	private int _propID_ExplosionTime;

	private MaterialPropertyBlock _matPropBlock;

	private float _lightIntensity;

	private float _lightRadius;

	private bool _playing;

	private float _timer;

	private ShipAudioController _audioController;

	private void Awake()
	{
		_propID_ExplosionTime = Shader.PropertyToID("_ExplosionTime");
		_matPropBlock = new MaterialPropertyBlock();
		_matPropBlock.SetFloat(_propID_ExplosionTime, 0f);
		_lightIntensity = _light.intensity;
		_lightRadius = _light.range;
		_renderer.enabled = false;
		_renderer.SetPropertyBlock(_matPropBlock);
		_light.enabled = false;
		_light.intensity = 0f;
		_light.range = 0.01f;
		_audioController = GameObject.FindGameObjectWithTag("Ship").GetComponentInChildren<ShipAudioController>();
		_playing = false;
		_timer = 0f;
	}

	private void Start()
	{
		base.enabled = false;
		_forceVolume.SetVolumeActivation(active: false);
	}

	private void Update()
	{
		if (!_playing)
		{
			base.enabled = false;
			return;
		}
		_timer += Time.deltaTime;
		float num = Mathf.Clamp01(_timer / _length);
		float value = (num - 2f) * (0f - num);
		_matPropBlock.SetFloat(_propID_ExplosionTime, value);
		_renderer.SetPropertyBlock(_matPropBlock);
		_light.intensity = _lightIntensity * (1f - num);
		_light.range = _lightRadius * Mathf.Clamp01(num * 10f);
		if (num > 0.5f)
		{
			_forceVolume.SetVolumeActivation(active: false);
		}
		if (num == 1f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Play()
	{
		_forceVolume.SetVolumeActivation(active: true);
		if (Vector3.Distance(base.transform.position, Locator.GetPlayerTransform().position) < base.transform.localScale.x * GetComponent<SphereCollider>().radius)
		{
			RumbleManager.PulseShipExplode();
		}
		_renderer.enabled = true;
		_light.enabled = true;
		float num = _audioController.PlayShipExplodeClip();
		if (num > _length)
		{
			_length = num;
		}
		_playing = true;
		base.enabled = true;
	}
}
