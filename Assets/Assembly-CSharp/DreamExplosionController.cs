using UnityEngine;

public class DreamExplosionController : MonoBehaviour
{
	[SerializeField]
	private Transform _scaleRoot;

	[SerializeField]
	private OWRenderer _flameRenderer;

	[SerializeField]
	private OWLight2 _flameLight;

	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private OWTriggerVolume _deathTrigger;

	private float _explodeTime;

	private bool _exploding;

	private void Awake()
	{
		_deathTrigger.OnEntry += OnEntry;
	}

	private void Start()
	{
		_flameRenderer.SetActivation(active: false);
		_flameLight.SetActivation(active: false);
		_deathTrigger.SetTriggerActivation(active: false);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_deathTrigger.OnEntry -= OnEntry;
	}

	public void OnEnterDreamWorld(DreamLanternItem lantern)
	{
		if (lantern.GetLanternType() == DreamLanternType.Malfunctioning)
		{
			base.enabled = true;
			_explodeTime = Time.time + 3f;
		}
	}

	public void OnExitDreamWorld()
	{
		if (_exploding)
		{
			ResetExplosion();
		}
	}

	private void ResetExplosion()
	{
		_scaleRoot.localScale = Vector3.one;
		_flameRenderer.SetActivation(active: false);
		_flameLight.SetActivation(active: false);
		_deathTrigger.SetTriggerActivation(active: false);
		_exploding = false;
		base.enabled = false;
	}

	private void FixedUpdate()
	{
		if (!_exploding && Time.time > _explodeTime)
		{
			_exploding = true;
			_audioSource.PlayOneShot(AudioType.DreamFire_Explosion);
			_scaleRoot.localScale = Vector3.one;
			_flameRenderer.SetActivation(active: true);
			_flameLight.SetActivation(active: true);
			_flameLight.SetIntensityScale(0f);
			_deathTrigger.SetTriggerActivation(active: true);
		}
		if (_exploding)
		{
			float num = Time.time - _explodeTime;
			_scaleRoot.localScale = Vector3.one + Vector3.one * 10f * num * num;
			_flameLight.SetIntensityScale(Mathf.InverseLerp(0f, 0.5f, num));
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			Vector3 vector = Locator.GetPlayerTransform().position - _scaleRoot.position;
			vector.y = 0f;
			Locator.GetPlayerBody().AddVelocityChange(vector.normalized * 16f + Vector3.up * 8f);
			Locator.GetDeathManager().KillPlayer(DeathType.DreamExplosion);
		}
	}
}
