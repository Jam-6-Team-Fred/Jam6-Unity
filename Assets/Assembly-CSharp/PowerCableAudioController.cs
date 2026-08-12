using UnityEngine;

[RequireComponent(typeof(OWAudioSource))]
public class PowerCableAudioController : SectoredMonoBehaviour
{
	private bool _activated;

	private OWAudioSource _audioSource;

	protected override void Awake()
	{
		base.Awake();
		_audioSource = GetComponent<OWAudioSource>();
		base.enabled = false;
	}

	public void OnFloodImpact()
	{
		_activated = true;
		UpdateEnabled();
	}

	protected override void OnSectorOccupantsUpdated()
	{
		UpdateEnabled();
	}

	private void UpdateEnabled()
	{
		bool flag = _activated && _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		base.enabled = flag;
	}

	private void Update()
	{
		if (!_audioSource.isPlaying)
		{
			_audioSource.transform.localPosition = Vector3.right * Random.Range(-25f, 25f);
			_audioSource.Play();
		}
	}
}
