using System;
using UnityEngine;

public class ProxyAshTwin : ProxyPlanet
{
	[SerializeField]
	private MeshRenderer[] _planetRenderers;

	[SerializeField]
	private TessellatedSphereRenderer _sandRenderer;

	[SerializeField]
	private Transform _sandTransform;

	[SerializeField]
	private Transform _realSandTransform;

	[SerializeField]
	private Transform _sandColumnRoot;

	[SerializeField]
	private Transform _realSandColumnRoot;

	[SerializeField]
	private MeshRenderer _sandColumnRenderer;

	[SerializeField]
	private GameObject _realSandColumnRenderObject;

	protected override AstroObject.Name astroObjectName => AstroObject.Name.TowerTwin;

	public override void UpdateScale(float scaleMultiplier, float viewDistance)
	{
		base.UpdateScale(scaleMultiplier, viewDistance);
		if (_sandColumnRenderer.enabled != _realSandColumnRenderObject.activeSelf)
		{
			_sandColumnRenderer.enabled = _realSandColumnRenderObject.activeSelf;
		}
		if (_sandColumnRenderer.enabled)
		{
			_sandTransform.localScale = _realSandTransform.localScale;
			_sandColumnRoot.position = _realSandColumnRoot.position;
			_sandColumnRoot.rotation = _realSandColumnRoot.rotation;
			_sandColumnRoot.localScale = _realSandColumnRoot.localScale;
		}
	}

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		for (int i = 0; i < _planetRenderers.Length; i++)
		{
			_planetRenderers[i].enabled = on;
		}
		_sandRenderer.enabled = on;
		_sandColumnRenderer.enabled = on;
	}

	protected override void Initialize()
	{
		try
		{
			base.Initialize();
			AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.TowerTwin);
			_realSandTransform = astroObject.GetSandLevelController().transform;
			SandFunnelController componentInParent = UnityEngine.Object.FindObjectOfType<SandfallHazardVolume>().GetComponentInParent<SandFunnelController>();
			_realSandColumnRoot = componentInParent.scaleRoot;
			_realSandColumnRenderObject = componentInParent.sandGeoObjects[0];
		}
		catch (NullReferenceException e)
		{
			PrintInitializeFailMessage(e);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
