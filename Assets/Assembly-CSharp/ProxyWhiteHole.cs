using System;
using System.Collections.Generic;
using UnityEngine;

public class ProxyWhiteHole : ProxyBody
{
	[SerializeField]
	private MeshRenderer _singularityRenderer;

	private Material _singularityMaterial;

	private float _startSingularityRadius;

	private float _startSingularityDistortRadius;

	private float _startSingularityFadeDist;

	private List<ProxyBrittleHollowFragment> _parkedFragments = new List<ProxyBrittleHollowFragment>(128);

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		_singularityRenderer.enabled = on;
		for (int i = 0; i < _parkedFragments.Count; i++)
		{
			if (_parkedFragments[i] != null)
			{
				_parkedFragments[i].ToggleRendering(on);
			}
		}
	}

	public void AddFragment(ProxyBrittleHollowFragment fragment)
	{
		_parkedFragments.Add(fragment);
		fragment.ToggleRendering(_singularityRenderer.enabled);
	}

	protected override void Initialize()
	{
		try
		{
			base.Initialize();
			AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.WhiteHole);
			_realObjectTransform = astroObject.transform;
			_singularityMaterial = new Material(_singularityRenderer.sharedMaterial);
			_singularityRenderer.sharedMaterial = _singularityMaterial;
		}
		catch (NullReferenceException e)
		{
			PrintInitializeFailMessage(e);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
