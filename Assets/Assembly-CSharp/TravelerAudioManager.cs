using System.Collections.Generic;
using UnityEngine;

public class TravelerAudioManager : MonoBehaviour
{
	public bool _debugPrint;

	private List<AudioSignal> _signals = new List<AudioSignal>(16);

	private float _playAudioTime;

	private bool _playAfterDelay;

	private bool _playerInBramble;

	private void OnValidate()
	{
		if (_debugPrint)
		{
			_debugPrint = false;
			for (int i = 0; i < _signals.Count; i++)
			{
				MonoBehaviour.print(_signals[i].gameObject.name + "   " + _signals[i].GetOWAudioSource().isPlaying.ToString() + "   " + _signals[i].GetOWAudioSource().timeSamples);
			}
		}
	}

	private void Start()
	{
		if (LoadManager.GetCurrentScene() == OWScene.EyeOfTheUniverse)
		{
			MonoBehaviour.print("TRAVELER AUDIO MANAGER DESTROYED");
			Object.Destroy(this);
			return;
		}
		List<AudioSignal> audioSignals = Locator.GetAudioSignals();
		for (int i = 0; i < audioSignals.Count; i++)
		{
			if (audioSignals[i].GetFrequency() == SignalFrequency.Traveler)
			{
				_signals.Add(audioSignals[i]);
			}
		}
		GlobalMessenger.AddListener("GameUnpaused", OnUnpause);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
		GlobalMessenger<Signalscope>.AddListener("EquipSignalscope", OnEquipSignalscope);
		GlobalMessenger.AddListener("PlayerEnterBrambleDimension", OnPlayerEnterBrambleDimension);
		GlobalMessenger.AddListener("PlayerExitBrambleDimension", OnPlayerExitBrambleDimension);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("GameUnpaused", OnUnpause);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
		GlobalMessenger<Signalscope>.RemoveListener("EquipSignalscope", OnEquipSignalscope);
		GlobalMessenger.RemoveListener("PlayerEnterBrambleDimension", OnPlayerEnterBrambleDimension);
		GlobalMessenger.RemoveListener("PlayerExitBrambleDimension", OnPlayerExitBrambleDimension);
	}

	public void StopAllTravelerAudio()
	{
		_playAfterDelay = false;
		for (int i = 0; i < _signals.Count; i++)
		{
			_signals[i].GetOWAudioSource().FadeOut(0.5f);
		}
	}

	public void PlayAllTravelerAudio(float audioDelay)
	{
		_playAfterDelay = true;
		_playAudioTime = Time.time + audioDelay;
	}

	private void OnUnpause()
	{
		for (int i = 0; i < _signals.Count; i++)
		{
			bool isPlaying = _signals[i].GetOWAudioSource().isPlaying;
			_signals[i].GetOWAudioSource().Stop();
			if (isPlaying)
			{
				_signals[i].GetOWAudioSource().Play();
				_signals[i].GetOWAudioSource().timeSamples = 0;
			}
		}
	}

	private void OnEndFastForward()
	{
		OnUnpause();
	}

	private void OnEquipSignalscope(Signalscope scope)
	{
		SyncTravelers();
	}

	private void SyncTravelers()
	{
		float num = float.PositiveInfinity;
		int num2 = -1;
		for (int i = 0; i < _signals.Count; i++)
		{
			if (!_signals[i].IsOnlyAudibleToScope() && _signals[i].IsInsideDarkBramble() == _playerInBramble)
			{
				float sqrMagnitude = (_signals[i].transform.position - Locator.GetPlayerCamera().transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					num2 = i;
				}
			}
		}
		for (int j = 0; j < _signals.Count; j++)
		{
			if (j != num2 && _signals[j].IsInsideDarkBramble() == _playerInBramble)
			{
				if (_signals[j].IsOnlyAudibleToScope() && !_signals[j].GetOWAudioSource().isPlaying)
				{
					_signals[j].GetOWAudioSource().SetLocalVolume(0f);
					_signals[j].GetOWAudioSource().Play();
				}
				_signals[j].GetOWAudioSource().timeSamples = _signals[num2].GetOWAudioSource().timeSamples;
			}
		}
	}

	private void Update()
	{
		if (!_playAfterDelay || !(Time.time >= _playAudioTime))
		{
			return;
		}
		for (int i = 0; i < _signals.Count; i++)
		{
			if (!_signals[i].IsOnlyAudibleToScope() || _signals[i].GetOWAudioSource().isPlaying)
			{
				_signals[i].GetOWAudioSource().FadeIn(0.5f);
				_signals[i].GetOWAudioSource().timeSamples = 0;
			}
		}
		_playAfterDelay = false;
	}

	private void OnPlayerEnterBrambleDimension()
	{
		_playerInBramble = true;
		if (Locator.GetToolModeSwapper().IsInToolMode(ToolMode.SignalScope))
		{
			SyncTravelers();
		}
	}

	private void OnPlayerExitBrambleDimension()
	{
		_playerInBramble = false;
		if (Locator.GetToolModeSwapper().IsInToolMode(ToolMode.SignalScope))
		{
			SyncTravelers();
		}
	}
}
