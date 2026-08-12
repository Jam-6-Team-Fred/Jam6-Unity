using UnityEngine;

public class OWEmissiveRenderer : OWRenderer
{
	[SerializeField]
	private int _emissiveMaterialIndex;

	[Header("HDR Color Space Hack")]
	[SerializeField]
	private bool _convertToGamma;

	private float _scale = 1f;

	private float _flickerScale = 1f;

	public float GetEmissiveScale()
	{
		Initialize();
		return _scale;
	}

	public float GetFlickerScale()
	{
		Initialize();
		return _flickerScale;
	}

	public void SetEmissiveScale(float scale)
	{
		Initialize();
		_scale = scale;
		UpdateEmissiveColor();
	}

	public void SetFlickerScale(float scale)
	{
		Initialize();
		_flickerScale = scale;
		UpdateEmissiveColor();
	}

	private void UpdateEmissiveColor()
	{
		Color color = ((_emissiveMaterialIndex == 0) ? GetOriginalEmissionColor() : GetOriginalEmissionColor(_emissiveMaterialIndex));
		if (_convertToGamma)
		{
			color = color.gamma;
		}
		SetEmissionColor(color * _scale * _flickerScale);
	}
}
