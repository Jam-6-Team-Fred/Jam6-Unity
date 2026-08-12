using UnityEngine;

public class ProxyOrbiter : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer[] _renderers;

	[SerializeField]
	private Transform _originalBody;

	[SerializeField]
	private Transform _originalPlanetBody;

	[SerializeField]
	private ProxyBody _proxyPlanetBody;

	private bool _wasVisibleBeforeMapView;

	private bool _initialized;

	private void Awake()
	{
		_initialized = false;
	}

	private void Start()
	{
		base.transform.parent = null;
	}

	private void Update()
	{
		if (_initialized)
		{
			Transform obj = base.transform;
			obj.rotation = _originalBody.rotation;
			Vector3 vector = _originalBody.position - _originalPlanetBody.position;
			obj.position = _proxyPlanetBody.transform.position + vector * _proxyPlanetBody.currentScaleFactor;
			obj.localScale = Vector3.one * _proxyPlanetBody.currentScaleFactor;
		}
	}

	public void SetVisible(bool visible)
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].enabled = visible;
		}
		base.enabled = visible;
	}

	public void SetOriginalBodies(Transform origBody, Transform origPlanetBody)
	{
		_originalBody = origBody;
		_originalPlanetBody = origPlanetBody;
		_initialized = true;
	}
}
