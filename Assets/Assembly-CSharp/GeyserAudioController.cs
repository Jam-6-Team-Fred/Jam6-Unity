using UnityEngine;

[RequireComponent(typeof(GeyserController))]
public class GeyserAudioController : SectoredMonoBehaviour
{
	[SerializeField]
	private Sector _exclusionSector;

	[Space(10f)]
	[SerializeField]
	private OWAudioSource _audioSourceLoop;

	[SerializeField]
	private OWAudioSource _audioSourceOneShot;

	[Space(10f)]
	[SerializeField]
	private float _geyserOpeningHeight;

	[SerializeField]
	private ParticleSystem _geyserSpoutParticles;

	private GeyserController _geyserController;

	private GeyserFluidVolume _fluidVolume;

	private bool _geyserStartOneShotPlayed;

	private bool _geyserActive;

	private float _geyserDeactivateTime;

	private bool _playerInSector;

	protected override void Awake()
	{
		base.Awake();
		_fluidVolume = this.GetRequiredComponentInChildren<GeyserFluidVolume>();
		_geyserController = this.GetRequiredComponentInChildren<GeyserController>();
		_geyserController.OnGeyserActivateEvent += OnActivateGeyser;
		_geyserController.OnGeyserDeactivateEvent += OnDeactivateGeyser;
		if (_exclusionSector != null)
		{
			_exclusionSector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
	}

	private void Start()
	{
		_audioSourceLoop.SetLocalVolume(0f);
		_geyserOpeningHeight = Mathf.Max(0f, _geyserOpeningHeight - 10f);
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_geyserController != null)
		{
			_geyserController.OnGeyserActivateEvent -= OnActivateGeyser;
			_geyserController.OnGeyserDeactivateEvent -= OnDeactivateGeyser;
		}
		if (_exclusionSector != null)
		{
			_exclusionSector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
	}

	private void Update()
	{
		if (_geyserActive)
		{
			UpdateLoopingAudioPosition();
		}
		if (!(_audioSourceOneShot != null))
		{
			return;
		}
		if (!_geyserStartOneShotPlayed && _fluidVolume.GetHeight() >= _geyserOpeningHeight)
		{
			if (_playerInSector)
			{
				_audioSourceOneShot.PlayOneShot(AudioType.TH_GeyserStart);
			}
			_geyserStartOneShotPlayed = true;
		}
		if (_geyserDeactivateTime > 0.001f && _geyserDeactivateTime + _geyserSpoutParticles.main.startLifetime.Evaluate(1f) > Time.time)
		{
			if (_playerInSector)
			{
				_audioSourceOneShot.PlayOneShot(AudioType.TH_GeyserEnd);
			}
			_geyserDeactivateTime = 0f;
			base.enabled = false;
		}
	}

	private void UpdateLoopingAudioPosition()
	{
		float max = (_geyserActive ? _fluidVolume.GetHeight() : _fluidVolume.GetMaximumHeight());
		float y = Mathf.Clamp(base.transform.InverseTransformPoint(Locator.GetPlayerTransform().position).y, 0f, max);
		_audioSourceLoop.transform.localPosition = new Vector3(0f, y, 0f);
	}

	private void OnActivateGeyser()
	{
		if (_playerInSector)
		{
			UpdateLoopingAudioPosition();
			_audioSourceLoop.FadeIn(0.5f, fadeFromNothing: false, randomizePlayhead: true);
			_geyserStartOneShotPlayed = false;
			base.enabled = true;
		}
		_geyserActive = true;
	}

	private void OnDeactivateGeyser()
	{
		_audioSourceLoop.FadeOut(_geyserSpoutParticles.main.startLifetime.Evaluate(1f) + 1f);
		_geyserDeactivateTime = Time.time;
		_geyserActive = false;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = _exclusionSector != null && _exclusionSector.ContainsOccupant(DynamicOccupant.Player);
		_playerInSector = !flag && _sector.ContainsOccupant(DynamicOccupant.Player);
		if (!_geyserActive)
		{
			return;
		}
		if (_playerInSector)
		{
			if (!_audioSourceLoop.isPlaying || _audioSourceLoop.IsFadingOut())
			{
				_audioSourceLoop.FadeIn(2f, fadeFromNothing: false, randomizePlayhead: true);
			}
			base.enabled = true;
		}
		else
		{
			_audioSourceLoop.FadeOut(2f);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_fluidVolume == null)
		{
			_fluidVolume = this.GetRequiredComponentInChildren<GeyserFluidVolume>();
		}
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(_fluidVolume.transform.position + _fluidVolume.transform.up * _geyserOpeningHeight, 0.5f);
	}
}
