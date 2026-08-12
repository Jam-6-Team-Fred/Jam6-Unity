public class SlideRotationModule : SlideFunctionModule
{
	public const float FORWARD_ROTATE_ANGLE = -45f;

	public override int DataSize()
	{
		return 0;
	}

	public SlideRotationModule()
	{
	}

	public SlideRotationModule(byte[] data, int offset)
	{
	}

	public override void EnterSlide(Slide slide, bool forward)
	{
		slide.RotateToMySection();
	}

	public override void Update(Slide slide)
	{
	}

	public override void ExitSlide(Slide slide, bool forward)
	{
		if (!forward)
		{
			slide.RotateToPrevSection();
		}
	}

	public override byte[] ToBytes()
	{
		return new byte[0];
	}
}
