using UnityEngine;

[AddComponentMenu("Audio/Audio Spread Controller", 300)]
[RequireComponent(typeof(OWAudioSource))]
public class AudioSpreadController : SectoredMonoBehaviour
{
	[SerializeField]
	private float _transitionDistance = 5f;

	private OWAudioSource _audioSource;

	private float _origSpread;

	private void Start()
	{
		base.enabled = false;
		_audioSource = GetComponent<OWAudioSource>();
		_origSpread = _audioSource.spread;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsOccupant(DynamicOccupant.Player);
	}

	private void Update()
	{
		float value = Vector3.Distance(base.transform.position, Locator.GetPlayerTransform().position);
		float t = Mathf.InverseLerp(_audioSource.minDistance + _transitionDistance, _audioSource.minDistance, value);
		_audioSource.spread = Mathf.Lerp(_origSpread, 180f, t);
	}

	private void OnDrawGizmosSelected()
	{
		AudioSource component = GetComponent<AudioSource>();
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, component.minDistance);
		Gizmos.DrawWireSphere(base.transform.position, component.minDistance + _transitionDistance);
	}
}
