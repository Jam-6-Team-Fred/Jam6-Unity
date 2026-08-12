using UnityEngine;

public class GeyserController : MonoBehaviour
{
	public delegate void GeyserActivateEvent();

	public delegate void GeyserDeactivateEvent();

	[SerializeField]
	private bool _activeOnAwake = true;

	[SerializeField]
	private float _activeDuration = 10f;

	[SerializeField]
	private float _inactiveDuration = 10f;

	[SerializeField]
	private QuantumObject _quantumRoot;

	private float _initTime;

	private bool _isActive;

	private float _deactivateTime;

	private ParticleSystem[] _particleSystems;

	private GeyserFluidVolume _fluidVolume;

	public event GeyserActivateEvent OnGeyserActivateEvent;

	public event GeyserDeactivateEvent OnGeyserDeactivateEvent;

	private void Awake()
	{
		_particleSystems = GetComponentsInChildren<ParticleSystem>();
		_fluidVolume = this.GetRequiredComponentInChildren<GeyserFluidVolume>();
	}

	private void Start()
	{
		if (_activeOnAwake)
		{
			ActivateGeyser();
			return;
		}
		DeactivateGeyser();
		_initTime = Random.Range(0f, _inactiveDuration);
	}

	public void Update()
	{
		if (_isActive && Time.time > _initTime + _activeDuration)
		{
			DeactivateGeyser();
		}
		else if (!_isActive && Time.time > _initTime + _inactiveDuration)
		{
			ActivateGeyser();
		}
		if (_quantumRoot != null && !_quantumRoot.IsQuantum() && !_isActive && Time.time > _deactivateTime + 3f)
		{
			_quantumRoot.SetIsQuantum(isQuantum: true);
		}
	}

	public void ActivateGeyser()
	{
		if (_quantumRoot != null)
		{
			_quantumRoot.SetIsQuantum(isQuantum: false);
		}
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			_particleSystems[i].Play();
		}
		_fluidVolume.SetFiring(firing: true);
		_isActive = true;
		_initTime = Time.time;
		if (this.OnGeyserActivateEvent != null)
		{
			this.OnGeyserActivateEvent();
		}
	}

	public void DeactivateGeyser()
	{
		_deactivateTime = Time.time;
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			_particleSystems[i].Stop();
		}
		_fluidVolume.SetFiring(firing: false);
		_isActive = false;
		_initTime = Time.time;
		if (this.OnGeyserDeactivateEvent != null)
		{
			this.OnGeyserDeactivateEvent();
		}
	}
}
