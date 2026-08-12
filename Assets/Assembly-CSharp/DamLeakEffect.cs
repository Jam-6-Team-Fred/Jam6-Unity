using UnityEngine;

public class DamLeakEffect : MonoBehaviour
{
	[SerializeField]
	private float _delay;

	[SerializeField]
	private OWRenderer[] _renderers = new OWRenderer[0];

	[SerializeField]
	private ParticleSystem[] _particles = new ParticleSystem[0];

	private bool _started;

	private bool _stopped;

	private float _damageTime;

	private void Start()
	{
		RingWorldController ringWorldController = Locator.GetRingWorldController();
		if (ringWorldController != null)
		{
			ringWorldController.OnDamDamaged += new OWEvent.OWCallback(OnDamDamaged);
			ringWorldController.OnDamBreak += new OWEvent.OWCallback(OnDamBreak);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		RingWorldController ringWorldController = Locator.GetRingWorldController();
		if (ringWorldController != null)
		{
			ringWorldController.OnDamDamaged -= new OWEvent.OWCallback(OnDamDamaged);
			ringWorldController.OnDamBreak -= new OWEvent.OWCallback(OnDamBreak);
		}
	}

	private void Update()
	{
		if (!_started || _stopped)
		{
			base.enabled = false;
		}
		else if (Time.timeSinceLevelLoad > _damageTime + _delay)
		{
			for (int i = 0; i < _renderers.Length; i++)
			{
				_renderers[i].SetActivation(active: true);
			}
			for (int j = 0; j < _particles.Length; j++)
			{
				_particles[j].Play();
			}
		}
	}

	private void OnDamDamaged()
	{
		_started = true;
		_damageTime = Time.timeSinceLevelLoad;
		base.enabled = true;
	}

	private void OnDamBreak()
	{
		_stopped = true;
		base.enabled = false;
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].SetActivation(active: false);
		}
		for (int j = 0; j < _particles.Length; j++)
		{
			_particles[j].Stop();
		}
	}
}
