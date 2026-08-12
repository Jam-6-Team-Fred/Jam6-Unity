using System;
using System.Collections.Generic;
using UnityEngine;

public class ProxyBrittleHollow : ProxyPlanet
{
	[SerializeField]
	private MeshRenderer[] _planetRenderers;

	[SerializeField]
	private MeshRenderer _blackHoleRenderer;

	[SerializeField]
	private ProxyOrbiter _moon;

	[SerializeField]
	private ProxyBrittleHollowFragment[] _fragments;

	[SerializeField]
	private bool _collectFragments;

	[SerializeField]
	private bool _collectStaticRenderers;

	[SerializeField]
	private bool _saveFragmentNames;

	private static readonly int propID_BlackHoleRadius = Shader.PropertyToID("_Radius");

	private static readonly int propID_BlackHoleDistortRadius = Shader.PropertyToID("_MaxDistortRadius");

	private static readonly int propID_BlackHoleFadeDist = Shader.PropertyToID("_DistortFadeDist");

	private Material _blackHoleMaterial;

	private float _startBlackHoleRadius;

	private float _startBlackHoleDistortRadius;

	private float _startBlackHoleFadeDist;

	private List<DetachableFragment> realFragmentsForLookup = new List<DetachableFragment>(128);

	private bool _fragmentsResolved;

	private ProxyWhiteHole _proxyWhiteHole;

	protected override AstroObject.Name astroObjectName => AstroObject.Name.BrittleHollow;

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		_blackHoleRenderer.enabled = on;
		for (int i = 0; i < _planetRenderers.Length; i++)
		{
			_planetRenderers[i].enabled = on;
		}
		_moon.SetVisible(on);
		for (int j = 0; j < _fragments.Length; j++)
		{
			if (!_fragments[j].warped)
			{
				_fragments[j].ToggleRendering(on);
			}
		}
	}

	public override void UpdateScale(float scaleMultiplier, float viewDistance)
	{
		base.UpdateScale(scaleMultiplier, viewDistance);
	}

	private void OnValidate()
	{
		if (_collectFragments)
		{
			_fragments = GetComponentsInChildren<ProxyBrittleHollowFragment>();
			_collectFragments = false;
		}
		if (_collectStaticRenderers)
		{
			MeshRenderer[] componentsInChildren = base.transform.Find("Proxy_BH").gameObject.GetComponentsInChildren<MeshRenderer>();
			List<MeshRenderer> list = new List<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				bool num = componentsInChildren[i].GetComponent<ProxyBrittleHollowFragment>() != null;
				bool flag = componentsInChildren[i].GetComponentInParent<ProxyBrittleHollowFragment>() != null;
				if (!num && !flag)
				{
					list.Add(componentsInChildren[i]);
				}
			}
			_planetRenderers = list.ToArray();
			_collectStaticRenderers = false;
		}
		if (_saveFragmentNames)
		{
			for (int j = 0; j < _fragments.Length; j++)
			{
				_fragments[j].SaveFragmentName();
			}
			_saveFragmentNames = false;
		}
	}

	private void ResolveFragments()
	{
		realFragmentsForLookup = new List<DetachableFragment>(_realObjectTransform.GetComponentsInChildren<DetachableFragment>());
		int num = 0;
		while (num < _fragments.Length && realFragmentsForLookup.Count > 0)
		{
			for (int i = 0; i < realFragmentsForLookup.Count; i++)
			{
				if (_fragments[num].realFragmentName.Equals(realFragmentsForLookup[i].gameObject.name))
				{
					_fragments[num].SetRealFragment(realFragmentsForLookup[i]);
					if (_proxyWhiteHole != null)
					{
						_fragments[i].SetProxyWhiteHole(_proxyWhiteHole);
					}
					num++;
					realFragmentsForLookup.RemoveAt(i);
					break;
				}
			}
		}
		realFragmentsForLookup = null;
		_fragmentsResolved = true;
	}

	private void AssignBrittleHollowReference()
	{
		for (int i = 0; i < _fragments.Length; i++)
		{
			_fragments[i].SetProxyBrittleHollow(this);
		}
	}

	public void AssignWhiteHoleReference(ProxyWhiteHole proxyWhiteHole)
	{
		_proxyWhiteHole = proxyWhiteHole;
		for (int i = 0; i < _fragments.Length; i++)
		{
			_fragments[i].SetProxyWhiteHole(_proxyWhiteHole);
		}
	}

	protected override void Initialize()
	{
		try
		{
			base.Initialize();
			AstroObject astroObject = Locator.GetAstroObject(astroObjectName);
			_moon.SetOriginalBodies(astroObject.GetMoon().transform, astroObject.transform);
			if (!_fragmentsResolved)
			{
				ResolveFragments();
			}
			AssignBrittleHollowReference();
			_blackHoleMaterial = new Material(_blackHoleRenderer.sharedMaterial);
			_blackHoleRenderer.sharedMaterial = _blackHoleMaterial;
		}
		catch (NullReferenceException e)
		{
			PrintInitializeFailMessage(e);
			UnityEngine.Object.Destroy(_moon.gameObject);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
