using System;
using UnityEngine;

public class SlideBeatAudioModule : SlideFunctionModule
{
	[SerializeField]
	public AudioType _audioType;

	[SerializeField]
	public float _delay;

	private float _startTime;

	private bool _played;

	public override int DataSize()
	{
		return 8;
	}

	public SlideBeatAudioModule()
	{
		_audioType = AudioType.Reel_1_Beat_A;
		_delay = 0f;
	}

	public SlideBeatAudioModule(byte[] data, int offset)
	{
		_audioType = (AudioType)BitConverter.ToInt32(data, offset);
		_delay = BitConverter.ToSingle(data, offset + 4);
	}

	public override void EnterSlide(Slide slide, bool forward)
	{
		if (!forward)
		{
			_startTime = float.PositiveInfinity;
			return;
		}
		_played = false;
		_startTime = Time.unscaledTime;
		if (_delay <= 0f)
		{
			slide.InvokePlayBeatAudio(_audioType);
			_played = true;
		}
	}

	public override void Update(Slide slide)
	{
		if (!_played && _delay > 0f && Time.unscaledTime > _startTime + _delay)
		{
			slide.InvokePlayBeatAudio(_audioType);
			_played = true;
		}
	}

	public override void ExitSlide(Slide slide, bool forward)
	{
	}

	public override byte[] ToBytes()
	{
		byte[] array = new byte[DataSize()];
		int num = 0;
		byte[] bytes = BitConverter.GetBytes((int)_audioType);
		Buffer.BlockCopy(bytes, 0, array, num, bytes.Length);
		num += bytes.Length;
		bytes = BitConverter.GetBytes(_delay);
		Buffer.BlockCopy(bytes, 0, array, num, bytes.Length);
		return array;
	}
}
