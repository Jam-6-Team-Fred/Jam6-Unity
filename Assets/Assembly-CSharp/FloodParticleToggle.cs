using UnityEngine;

public class FloodParticleToggle : SectoredMonoBehaviour
{
	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private ParticleSystem[] _particles;

	[SerializeField]
	private bool _playAfterFlood;

	private bool _playing;

	private bool _preFlood = true;

	protected override void Awake()
	{
		base.Awake();
		_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (flag != _playing)
		{
			_playing = flag;
			UpdateParticles();
		}
	}

	private void OnFloodImpact()
	{
		_preFlood = false;
		UpdateParticles();
	}

	private void UpdateParticles()
	{
		for (int i = 0; i < _particles.Length; i++)
		{
			if (_playing && _preFlood != _playAfterFlood)
			{
				_particles[i].Play();
			}
			else
			{
				_particles[i].Stop();
			}
		}
	}
}
