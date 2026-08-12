using UnityEngine;

public class ProxyQuantumMoon : ProxyPlanet
{
	[SerializeField]
	private MeshRenderer _mainRenderer;

	protected override AstroObject.Name astroObjectName => AstroObject.Name.QuantumMoon;

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		_mainRenderer.enabled = on;
	}
}
