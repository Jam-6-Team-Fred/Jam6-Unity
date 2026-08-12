using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InterferenceDetector : Detector
{
	private float _netInterference;

	private OWRigidbody _owRigidbody;

	protected override void Awake()
	{
		_owRigidbody = this.GetAttachedOWRigidbody();
		base.Awake();
	}

	public float GetInterference()
	{
		return _netInterference;
	}

	private void Update()
	{
		_netInterference = 0f;
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			InterferenceVolume interferenceVolume = _activeVolumes[i] as InterferenceVolume;
			_netInterference += interferenceVolume.GetInterferenceAtPoint(_owRigidbody.GetPosition());
		}
	}

	public override void AddVolume(EffectVolume eVol)
	{
		if (eVol as InterferenceVolume != null)
		{
			base.AddVolume(eVol);
		}
	}

	public override void RemoveVolume(EffectVolume eVol)
	{
		if (eVol as InterferenceVolume != null)
		{
			base.RemoveVolume(eVol);
		}
	}
}
