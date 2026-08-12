using System;
using UnityEngine;

public class SlidePlayTimeModule : SlideFunctionModule
{
	[SerializeField]
	public float _duration;

	private float _startTime;

	public override int DataSize()
	{
		return 4;
	}

	public SlidePlayTimeModule()
	{
		_duration = 0f;
	}

	public SlidePlayTimeModule(byte[] data, int offset)
	{
		_duration = BitConverter.ToSingle(data, offset);
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
		return BitConverter.GetBytes(_duration);
	}
}
