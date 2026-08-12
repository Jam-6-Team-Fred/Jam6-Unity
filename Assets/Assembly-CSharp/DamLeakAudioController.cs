using System;
using UnityEngine;

public class DamLeakAudioController : MonoBehaviour
{
	[Serializable]
	public struct DamLeakAudio
	{
		[SerializeField]
		private float _startLeakProgression;

		[SerializeField]
		private OWAudioSource _audioSource;

		public bool isPlaying => _audioSource.isPlaying;

		public void Initialize()
		{
			_audioSource.SetLocalVolume(0f);
		}

		public void UpdateAudio(bool playerInSector, float leakProgression, float deltaTime)
		{
			float num = (playerInSector ? Mathf.Lerp(_startLeakProgression, 1f, leakProgression) : 0f);
			if (!_audioSource.isPlaying)
			{
				if (!(num > 0f))
				{
					return;
				}
				_audioSource.Play();
				_audioSource.RandomizePlayhead();
			}
			float localVolume = _audioSource.GetLocalVolume();
			localVolume = Mathf.MoveTowards(localVolume, num, deltaTime);
			_audioSource.SetLocalVolume(localVolume);
			if (localVolume <= 0f)
			{
				_audioSource.Stop();
			}
		}

		public void OnDamBroken()
		{
			_audioSource.FadeOut(0.5f);
		}
	}

	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private DamDestructionController _damDestructionController;

	[SerializeField]
	private DamLeakAudio[] _leakAudio;

	private void Awake()
	{
		_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnAudioSectorEntry);
		GlobalMessenger.AddListener("DamBroken", OnDamBroken);
	}

	private void Start()
	{
		base.enabled = false;
		for (int i = 0; i < _leakAudio.Length; i++)
		{
			_leakAudio[i].Initialize();
		}
	}

	private void OnDestroy()
	{
		RemoveListeners();
	}

	private void RemoveListeners()
	{
		_sector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnAudioSectorEntry);
		GlobalMessenger.RemoveListener("DamBroken", OnDamBroken);
	}

	private void Update()
	{
		if (!base.enabled)
		{
			return;
		}
		float leakProgression = _damDestructionController.GetLeakProgression();
		bool flag = _sector.ContainsOccupant(DynamicOccupant.Player);
		bool flag2 = false;
		for (int i = 0; i < _leakAudio.Length; i++)
		{
			_leakAudio[i].UpdateAudio(flag, leakProgression, Time.deltaTime);
			if (_leakAudio[i].isPlaying)
			{
				flag2 = true;
			}
		}
		if (!flag && !flag2)
		{
			base.enabled = false;
		}
	}

	private void OnDamBroken()
	{
		RemoveListeners();
		base.enabled = false;
		for (int i = 0; i < _leakAudio.Length; i++)
		{
			_leakAudio[i].OnDamBroken();
		}
	}

	private void OnAudioSectorEntry(SectorDetector detector)
	{
		if (detector.GetOccupantType() == DynamicOccupant.Player)
		{
			base.enabled = true;
		}
	}
}
