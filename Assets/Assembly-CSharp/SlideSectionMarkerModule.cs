using System;
using UnityEngine;

public class SlideSectionMarkerModule : SlideFunctionModule
{
	[SerializeField]
	public bool _expanded;

	public override int DataSize()
	{
		return 1;
	}

	public SlideSectionMarkerModule()
	{
		_expanded = false;
	}

	public SlideSectionMarkerModule(byte[] data, int offset)
	{
		_expanded = BitConverter.ToBoolean(data, offset);
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
		return BitConverter.GetBytes(_expanded);
	}
}
