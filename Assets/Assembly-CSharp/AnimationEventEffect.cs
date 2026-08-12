using System;
using UnityEngine;

public class AnimationEventEffect : SectoredMonoBehaviour
{
	[Serializable]
	public struct PrefabTransform
	{
		public GameObject prefab;

		[Tooltip("target position/rotation to spawn prefab on, if null will use current gameObject's position/rotation")]
		public Transform targetTransform;
	}

	[Serializable]
	public struct EventActivations
	{
		[SerializeField]
		public PrefabTransform[] prefabs;

		public ParticleSystem[] ParticleSystems;

		[Space]
		public OWAudioSource audioSource;

		public float audioDelay;
	}

	[SerializeField]
	private EventActivations[] _eventActivations;

	private Transform _fluidVolumeTransform;

	private void Start()
	{
		_fluidVolumeTransform = Locator.GetRingRiverFluidVolume().transform;
	}

	public void OnAnimationEvent(int index)
	{
		if (!_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			return;
		}
		if (index >= 0 && index < _eventActivations.Length)
		{
			for (int i = 0; i < _eventActivations[index].prefabs.Length; i++)
			{
				Transform transform = ((_eventActivations[index].prefabs[i].targetTransform != null) ? _eventActivations[index].prefabs[i].targetTransform : base.transform);
				UnityEngine.Object.Instantiate(_eventActivations[index].prefabs[i].prefab, transform.position, transform.rotation, _fluidVolumeTransform);
			}
			for (int j = 0; j < _eventActivations[index].ParticleSystems.Length; j++)
			{
				_eventActivations[index].ParticleSystems[j].Play();
			}
			if (_eventActivations[index].audioSource != null)
			{
				_eventActivations[index].audioSource.PlayDelayed(_eventActivations[index].audioDelay);
			}
		}
		else
		{
			Debug.LogError($"[AnimationEventEffect] An animation event index ({index}) on {base.gameObject.name} is out of the effects list range!", this);
		}
	}
}
