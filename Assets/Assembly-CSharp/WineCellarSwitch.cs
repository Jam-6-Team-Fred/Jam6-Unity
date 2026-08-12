using UnityEngine;

public class WineCellarSwitch : MonoBehaviour
{
	[SerializeField]
	private float _bottleAnimDuration = 0.5f;

	[SerializeField]
	private TransformAnimator _bottleAnimator;

	[SerializeField]
	private SlidingDoor _slidingDoor;

	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private OWAudioSource _audioSource;

	private float _openDoorTime;

	private void Awake()
	{
		_interactReceiver.OnPressInteract += OnPressInteract;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_interactReceiver.OnPressInteract -= OnPressInteract;
	}

	private void OnPressInteract()
	{
		_interactReceiver.DisableInteraction();
		_bottleAnimator.RotateToLocalEulerAngles(Vector3.up * 45f, _bottleAnimDuration);
		_audioSource.PlayOneShot(AudioType.GearRotate_Light);
		base.enabled = true;
		_openDoorTime = Time.time + _bottleAnimDuration;
	}

	private void Update()
	{
		if (Time.time >= _openDoorTime)
		{
			_slidingDoor.Open();
			base.enabled = false;
		}
	}
}
