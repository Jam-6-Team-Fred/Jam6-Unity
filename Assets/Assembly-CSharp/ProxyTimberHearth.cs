using System;
using UnityEngine;

public class ProxyTimberHearth : ProxyPlanet
{
	[SerializeField]
	private MeshRenderer[] _planetRenderers;

	[SerializeField]
	private ProxyOrbiter _moon;

	protected override AstroObject.Name astroObjectName => AstroObject.Name.TimberHearth;

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		for (int i = 0; i < _planetRenderers.Length; i++)
		{
			_planetRenderers[i].enabled = on;
		}
		_moon.SetVisible(on);
	}

	protected override void Initialize()
	{
		try
		{
			base.Initialize();
			AstroObject astroObject = Locator.GetAstroObject(astroObjectName);
			_moon.SetOriginalBodies(astroObject.GetMoon().transform, astroObject.transform);
		}
		catch (NullReferenceException e)
		{
			PrintInitializeFailMessage(e);
			UnityEngine.Object.Destroy(_moon.gameObject);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
