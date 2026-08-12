using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
[RequireComponent(typeof(SphereShape))]
public class ExplosiveGasVolume : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _particles;

	private OWTriggerVolume _trigger;

	private ThrusterModel _jetpack;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
	}

	private void Start()
	{
		_jetpack = Locator.GetPlayerTransform().GetComponent<ThrusterModel>();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void FixedUpdate()
	{
		if (_jetpack.GetLocalAcceleration().magnitude > 1f)
		{
			if (_particles != null)
			{
				_particles.Play();
			}
			Vector3 vector = Locator.GetPlayerTransform().position - base.transform.position;
			Locator.GetPlayerBody().AddVelocityChange(vector.normalized * 20f);
			base.enabled = false;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = false;
		}
	}
}
