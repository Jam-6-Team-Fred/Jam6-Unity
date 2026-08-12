using System;
using UnityEngine;

public class SlideAmbientLightModule : SlideFunctionModule
{
	private const float fadeTime = 1f;

	[SerializeField]
	public float _intensity;

	[SerializeField]
	public float _range;

	[SerializeField]
	public Color _color;

	[SerializeField]
	public float _spotIntensityMod;

	public override int DataSize()
	{
		return 28;
	}

	public SlideAmbientLightModule()
	{
		_intensity = 1f;
		_range = 20f;
		_color = Color.white;
		_spotIntensityMod = 0f;
	}

	public SlideAmbientLightModule(byte[] data, int offset)
	{
		_intensity = BitConverter.ToSingle(data, offset);
		_range = BitConverter.ToSingle(data, offset + 4);
		_color = default(Color);
		_color.r = BitConverter.ToSingle(data, offset + 8);
		_color.g = BitConverter.ToSingle(data, offset + 12);
		_color.b = BitConverter.ToSingle(data, offset + 16);
		_color.a = BitConverter.ToSingle(data, offset + 20);
		_spotIntensityMod = BitConverter.ToSingle(data, offset + 24);
	}

	public override void EnterSlide(Slide slide, bool forward)
	{
		slide.InvokeBounceLightUpdate(GetLightParameters());
	}

	public override void Update(Slide slide)
	{
	}

	public override void ExitSlide(Slide slide, bool forward)
	{
	}

	public void CopyLightSettings(Light light)
	{
		_intensity = light.intensity;
		_range = light.range;
		_color = light.color;
	}

	public LightParameters GetLightParameters()
	{
		LightParameters result = default(LightParameters);
		result.intensity = _intensity;
		result.range = _range;
		result.color = _color;
		result.spotIntensityMod = _spotIntensityMod;
		return result;
	}

	public override byte[] ToBytes()
	{
		byte[] array = new byte[DataSize()];
		byte[] bytes = BitConverter.GetBytes(_intensity);
		int num = 0;
		Buffer.BlockCopy(bytes, 0, array, num, bytes.Length);
		num += bytes.Length;
		bytes = BitConverter.GetBytes(_range);
		Buffer.BlockCopy(bytes, 0, array, num, bytes.Length);
		num += bytes.Length;
		bytes = BitConverter.GetBytes(_color.r);
		Buffer.BlockCopy(bytes, 0, array, num, bytes.Length);
		num += bytes.Length;
		bytes = BitConverter.GetBytes(_color.g);
		Buffer.BlockCopy(bytes, 0, array, num, bytes.Length);
		num += bytes.Length;
		bytes = BitConverter.GetBytes(_color.b);
		Buffer.BlockCopy(bytes, 0, array, num, bytes.Length);
		num += bytes.Length;
		bytes = BitConverter.GetBytes(_color.a);
		Buffer.BlockCopy(bytes, 0, array, num, bytes.Length);
		num += bytes.Length;
		bytes = BitConverter.GetBytes(_spotIntensityMod);
		Buffer.BlockCopy(bytes, 0, array, num, bytes.Length);
		return array;
	}
}
