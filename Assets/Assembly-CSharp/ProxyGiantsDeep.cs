using System;
using UnityEngine;

public class ProxyGiantsDeep : ProxyPlanet
{
	[SerializeField]
	private MeshRenderer _mainBody;

	[SerializeField]
	private CloudLightningGenerator _lightningGenerator;

	[SerializeField]
	private ProxyOrbiter[] _cannonChunks;

	protected override AstroObject.Name astroObjectName => AstroObject.Name.GiantsDeep;

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		_mainBody.enabled = on;
		_lightningGenerator.enabled = on;
		for (int i = 0; i < _cannonChunks.Length; i++)
		{
			_cannonChunks[i].SetVisible(on);
		}
	}

	protected override void Initialize()
	{
		try
		{
			base.Initialize();
			AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.ProbeCannon);
			_cannonChunks[0].SetOriginalBodies(astroObject.transform, _realObjectTransform);
			OrbitalProbeLaunchController component = astroObject.GetComponent<OrbitalProbeLaunchController>();
			_cannonChunks[1].SetOriginalBodies(component.realDebrisSectorProxies[0].GetComponentInParent<InitialMotion>().transform, _realObjectTransform);
			_cannonChunks[2].SetOriginalBodies(component.realDebrisSectorProxies[1].GetComponentInParent<InitialMotion>().transform, _realObjectTransform);
		}
		catch (NullReferenceException e)
		{
			PrintInitializeFailMessage(e);
			UnityEngine.Object.Destroy(_cannonChunks[0].gameObject);
			UnityEngine.Object.Destroy(_cannonChunks[1].gameObject);
			UnityEngine.Object.Destroy(_cannonChunks[2].gameObject);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
