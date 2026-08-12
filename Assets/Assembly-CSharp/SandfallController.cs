using System;
using UnityEngine;

public class SandfallController : SectoredMonoBehaviour
{
	[Serializable]
	private struct SandfallParticleSystem
	{
		public ParticleSystem particle;

		public float lifetimeOffset;
	}

	[SerializeField]
	private float _height = 20f;

	[SerializeField]
	private SandfallParticleSystem[] _particles;

	[SerializeField]
	private Transform _sandPile;

	[SerializeField]
	private OWAudioSource _audioSource;

	private Transform _planetTransform;

	private SandLevelController _sandLevelController;

	private bool _playerInSector;

	private void Start()
	{
		AstroObject componentInParent = GetComponentInParent<AstroObject>();
		_planetTransform = componentInParent.transform;
		_sandLevelController = componentInParent.GetSandLevelController();
		if (_audioSource != null)
		{
			_audioSource.SetLocalVolume(0f);
		}
		base.enabled = false;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool playerInSector = _playerInSector;
		_playerInSector = _sector.ContainsAnyOccupants(DynamicOccupant.Player);
		if (_audioSource != null)
		{
			base.enabled = _playerInSector;
			if (!playerInSector && _playerInSector)
			{
				UpdateAudioPosition();
				_audioSource.FadeIn(5f, fadeFromNothing: false, randomizePlayhead: true);
			}
			else if (playerInSector && !_playerInSector)
			{
				_audioSource.FadeOut(5f);
			}
		}
	}

	private void Update()
	{
		UpdateAudioPosition();
	}

	private void UpdateAudioPosition()
	{
		Vector3 vector = base.transform.position - _planetTransform.position;
		float magnitude = vector.magnitude;
		float num = Mathf.Max(0f, Mathf.Max(magnitude - _height, _sandLevelController.GetRadius()));
		Vector3 segmentStart = _planetTransform.position + vector.normalized * num;
		Vector3 position = OWMath.ClosestPointOnSegment(Locator.GetPlayerTransform().position, segmentStart, base.transform.position);
		_audioSource.transform.position = position;
	}
}
