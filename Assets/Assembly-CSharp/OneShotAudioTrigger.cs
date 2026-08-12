using UnityEngine;

[AddComponentMenu("Audio/One Shot Audio Trigger", 400)]
[RequireComponent(typeof(OWAudioSource))]
[RequireComponent(typeof(OWTriggerVolume))]
public class OneShotAudioTrigger : MonoBehaviour
{
	[SerializeField]
	private bool _playInShip = true;

	private OWTriggerVolume _trigger;

	private OWAudioSource _owAudioSource;

	private void OnValidate()
	{
		AudioSource component = base.gameObject.GetComponent<AudioSource>();
		if (component.loop)
		{
			component.loop = false;
		}
		if (component.playOnAwake)
		{
			component.playOnAwake = false;
		}
	}

	private void Awake()
	{
		_owAudioSource = base.gameObject.GetComponent<OWAudioSource>();
		_trigger = base.gameObject.GetComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void Update()
	{
		if (_playInShip || !PlayerState.IsInsideShip())
		{
			_owAudioSource.Play();
			base.enabled = false;
			_trigger.OnEntry -= OnEntry;
			_trigger.OnExit -= OnExit;
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
