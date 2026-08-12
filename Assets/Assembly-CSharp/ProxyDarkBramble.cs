using UnityEngine;

public class ProxyDarkBramble : ProxyPlanet
{
	[SerializeField]
	private MeshRenderer _mainBody;

	[SerializeField]
	private MeshRenderer _volumetricFogRenderer;

	protected override AstroObject.Name astroObjectName => AstroObject.Name.DarkBramble;

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		_mainBody.enabled = on;
		_volumetricFogRenderer.enabled = on;
	}
}
