using System;
using UnityEngine;

public class SlideBackdropAudioModule : SlideFunctionModule
{
	[SerializeField]
	public AudioType _audioType;

	[SerializeField]
	public float _fadeTime;

	private float _startTime;

	private bool _played;

	public override int DataSize()
	{
		return 8;
	}

	public SlideBackdropAudioModule()
	{
		_audioType = AudioType.Reel_1_Backdrop_A;
		_fadeTime = 2f;
	}

	public SlideBackdropAudioModule(byte[] data, int offset)
	{
		_audioType = (AudioType)BitConverter.ToInt32(data, offset);
		_fadeTime = BitConverter.ToSingle(data, offset + 4);
	}

	public override void EnterSlide(Slide slide, bool forward)
	{
	}

	public override void Update(Slide slide)
	{
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
		bytes = BitConverter.GetBytes(_fadeTime);
		Buffer.BlockCopy(bytes, 0, array, num, bytes.Length);
		return array;
	}
}
