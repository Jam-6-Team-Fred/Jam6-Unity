using System;
using UnityEngine;

public class SlideBlackFrameModule : SlideFunctionModule
{
	[SerializeField]
	public float _duration;

	private float _startTime;

	public override int DataSize()
	{
		return 4;
	}

	public SlideBlackFrameModule()
	{
		_duration = 0f;
	}

	public SlideBlackFrameModule(byte[] data, int offset)
	{
		_duration = BitConverter.ToSingle(data, offset);
	}

	public override void EnterSlide(Slide slide, bool forward)
	{
		if (!(_duration <= 0f))
		{
			_startTime = Time.unscaledTime;
			slide.textureOverride = (slide.invertBlackFrames ? Texture2D.whiteTexture : Texture2D.blackTexture);
			slide.SetChangeSlidesAllowed(allowed: false);
		}
	}

	public override void Update(Slide slide)
	{
		if (!(_duration <= 0f) && Time.unscaledTime > _startTime + _duration && slide.textureOverride != null)
		{
			slide.textureOverride = null;
			slide.SetChangeSlidesAllowed(allowed: true);
		}
	}

	public override void ExitSlide(Slide slide, bool forward)
	{
		if (slide.textureOverride != null)
		{
			slide.textureOverride = null;
			slide.InvokeTextureUpdate();
			slide.SetChangeSlidesAllowed(allowed: true);
		}
	}

	public override byte[] ToBytes()
	{
		return BitConverter.GetBytes(_duration);
	}
}
