using System;
using UnityEngine;

public class DistantProxyManager : MonoBehaviour
{
	[Serializable]
	public struct AstroProxyTuple
	{
		public AstroObject.Name astroName;

		public GameObject proxyPrefab;
	}

	private static DistantProxyManager _instance;

	[SerializeField]
	private GameObject _sunProxyPrefab;

	[SerializeField]
	private AstroProxyTuple[] _proxies;

	private ProxyWhiteHole _proxyWhiteHole;

	private ProxyBrittleHollow _proxyBrittleHollow;

	public static DistantProxyManager instance => _instance;

	public ProxyWhiteHole proxyWhiteHole => _proxyWhiteHole;

	private void Start()
	{
		_instance = this;
		SunProxyEffectController componentInChildren = UnityEngine.Object.Instantiate(_sunProxyPrefab).GetComponentInChildren<SunProxyEffectController>();
		Locator.GetSunController().SetProxyEffectController(componentInChildren);
		for (int i = 0; i < _proxies.Length; i++)
		{
			if (Locator.GetAstroObject(_proxies[i].astroName) != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_proxies[i].proxyPrefab);
				switch (_proxies[i].astroName)
				{
				case AstroObject.Name.WhiteHole:
					_proxyWhiteHole = gameObject.GetComponent<ProxyWhiteHole>();
					break;
				case AstroObject.Name.BrittleHollow:
					_proxyBrittleHollow = gameObject.GetComponent<ProxyBrittleHollow>();
					break;
				}
			}
		}
		if (_proxyBrittleHollow != null && _proxyWhiteHole != null)
		{
			_proxyBrittleHollow.AssignWhiteHoleReference(_proxyWhiteHole);
		}
	}

	private void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}
}
