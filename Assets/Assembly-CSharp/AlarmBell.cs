using UnityEngine;

public class AlarmBell : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private Animation _animation;

	[SerializeField]
	private OWTriggerVolume _bellTrigger;

	[SerializeField]
	private OWLightController _lightController;

	private void Awake()
	{
		_bellTrigger.OnEntry += OnEntry;
	}

	private void Start()
	{
		if (_lightController != null)
		{
			_lightController.SetIntensity(0f);
		}
	}

	private void OnDestroy()
	{
		_bellTrigger.OnEntry -= OnEntry;
	}

	public void PlaySingleChime(int index)
	{
		_oneShotSource.PlayOneShot(AudioType.AlarmChime_RW, index);
	}

	public void PlayAnimation()
	{
		_animation.Play("AlarmBell_Chime");
		_lightController.SetIntensity(1f);
	}

	public void StopAnimation()
	{
		_animation.Stop();
		_lightController.FadeTo(0f, 5f);
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("ProbeDetector"))
		{
			_oneShotSource.PlayOneShot(AudioType.AlarmChime_RW);
		}
		else if (hitObj.CompareTag("PlayerDetector"))
		{
			Vector3 vector = base.gameObject.GetAttachedOWRigidbody().GetPointVelocity(_bellTrigger.transform.position) - Locator.GetPlayerBody().GetVelocity();
			float magnitude = Vector3.ProjectOnPlane(vector, _bellTrigger.transform.up).magnitude;
			if (magnitude > 4f)
			{
				float volume = Mathf.Lerp(0.2f, 1f, Mathf.InverseLerp(4f, 12f, magnitude));
				_oneShotSource.PlayOneShot(AudioType.AlarmChime_RW, volume);
			}
		}
	}
}
