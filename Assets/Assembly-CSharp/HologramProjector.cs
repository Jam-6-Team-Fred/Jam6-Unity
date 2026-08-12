using UnityEngine;

public abstract class HologramProjector : MonoBehaviour
{
	[SerializeField]
	private Transform _hologramSpawnPoint;

	[SerializeField]
	private Transform _hologramDisplayPoint;

	[Space]
	[SerializeField]
	private OWAudioSource _oneShotAudioSource;

	[SerializeField]
	private OWAudioSource _loopingAudioSource;

	[Space]
	[SerializeField]
	private Renderer _poolRenderer;

	[SerializeField]
	private Material _poolInactiveMaterial;

	[SerializeField]
	private Material _poolActiveMaterial;

	[SerializeField]
	private float _poolTransitionLength = 1f;

	protected Hologram _activeHologram;

	private float _poolTimer;

	private void Start()
	{
		_loopingAudioSource.SetMaxVolume(0.4f);
		if (_loopingAudioSource != null)
		{
			_loopingAudioSource.SetLocalVolume(0f);
		}
		if (_poolRenderer != null)
		{
			_poolRenderer.material.Lerp(_poolInactiveMaterial, _poolActiveMaterial, 0f);
		}
		base.enabled = false;
	}

	public Hologram GetActiveHologram()
	{
		return _activeHologram;
	}

	protected void CreateHologram(GameObject hologramPrefab)
	{
		if (_poolRenderer != null)
		{
			base.enabled = true;
		}
		GameObject obj = Object.Instantiate(hologramPrefab);
		obj.transform.parent = base.transform;
		Hologram component = obj.GetComponent<Hologram>();
		component.SetSpawnAndDisplayPoints(_hologramSpawnPoint, _hologramDisplayPoint);
		_activeHologram = component;
		_activeHologram.Activate();
		_activeHologram.OnHologramComplete += OnHologramComplete;
		if (_oneShotAudioSource != null)
		{
			_oneShotAudioSource.PlayOneShot(AudioType.NomaiHologramActivate);
			_loopingAudioSource.FadeIn(5f);
		}
	}

	protected void DestroyActiveHologram()
	{
		if (_activeHologram != null)
		{
			_activeHologram.Deactivate();
			_activeHologram.OnHologramComplete -= OnHologramComplete;
			_activeHologram = null;
			if (_oneShotAudioSource != null)
			{
				_oneShotAudioSource.PlayOneShot(AudioType.NomaiHologramDeactivate);
				_loopingAudioSource.FadeOut(5f);
			}
		}
	}

	protected virtual void OnHologramComplete(Hologram hologram)
	{
	}

	protected virtual void Update()
	{
		if (_poolRenderer != null)
		{
			_poolTimer = Mathf.MoveTowards(_poolTimer, (_activeHologram != null) ? 1f : 0f, Time.deltaTime / _poolTransitionLength);
			_poolRenderer.material.Lerp(_poolInactiveMaterial, _poolActiveMaterial, _poolTimer);
			if (_activeHologram == null && _poolTimer <= 0f)
			{
				base.enabled = false;
			}
		}
	}
}
