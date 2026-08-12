using System;
using UnityEngine;

public class ProxyEmberTwin : ProxyPlanet
{
	[SerializeField]
	private MeshRenderer[] _planetRenderers;

	[SerializeField]
	private TessellatedSphereRenderer _sandRenderer;

	[SerializeField]
	private Transform _sandTransform;

	[SerializeField]
	private Transform _realSandTransform;

	protected override AstroObject.Name astroObjectName => AstroObject.Name.CaveTwin;

	public override void UpdateScale(float scaleMultiplier, float viewDistance)
	{
		base.UpdateScale(scaleMultiplier, viewDistance);
		_sandTransform.localScale = _realSandTransform.localScale;
	}

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		for (int i = 0; i < _planetRenderers.Length; i++)
		{
			_planetRenderers[i].enabled = on;
		}
		_sandRenderer.enabled = on;
	}

	protected override void Initialize()
	{
		try
		{
			base.Initialize();
			_realSandTransform = Locator.GetAstroObject(AstroObject.Name.CaveTwin).GetSandLevelController().transform;
		}
		catch (NullReferenceException e)
		{
			PrintInitializeFailMessage(e);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
