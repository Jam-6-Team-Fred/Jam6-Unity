using UnityEngine;

public class EffectRuleset : RulesetVolume
{
	public enum BubbleType
	{
		None = 0,
		Underwater = 1,
		FogWarp = 2
	}

	[SerializeField]
	private BubbleType _type;

	[SerializeField]
	private Material _material;

	[Header("Underwater Effect Bubble Params")]
	[SerializeField]
	private float _underwaterDistortScale = 0.001f;

	[SerializeField]
	private float _underwaterMinDistort = 0.005f;

	[SerializeField]
	private float _underwaterMaxDistort = 0.1f;

	[Header("Cloud Bubble Material (Optional)")]
	[SerializeField]
	private Material _cloudMaterial;

	[Header("Sand Bubble Material (Optional)")]
	[SerializeField]
	private Material _sandMaterial;

	public bool UsesEffectBubble()
	{
		return _type != BubbleType.None;
	}

	public BubbleType GetEffectBubbleType()
	{
		return _type;
	}

	public Material GetEffectBubbleMaterial()
	{
		return _material;
	}

	public float GetEffectBubbleUnderwaterDistortScale()
	{
		return _underwaterDistortScale;
	}

	public float GetEffectBubbleUnderwaterMinDistort()
	{
		return _underwaterMinDistort;
	}

	public float GetEffectBubbleUnderwaterMaxDistort()
	{
		return _underwaterMaxDistort;
	}

	public Material GetCloudEffectBubbleMaterial()
	{
		return _cloudMaterial;
	}

	public Material GetSandEffectBubbleMaterial()
	{
		return _sandMaterial;
	}
}
