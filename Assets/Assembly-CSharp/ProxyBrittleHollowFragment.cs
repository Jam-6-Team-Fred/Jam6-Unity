using UnityEngine;

public class ProxyBrittleHollowFragment : ProxyBody
{
	[SerializeField]
	private MeshRenderer[] _renderers;

	[SerializeField]
	private string _realFragmentName;

	private DetachableFragment _originalFragment;

	private ProxyBrittleHollow _proxyBrittleHollow;

	private ProxyWhiteHole _proxyWhiteHole;

	private bool _detached;

	private bool _warped;

	private bool _settled;

	public string realFragmentName => _realFragmentName;

	public bool detached => _detached;

	public bool warped => _warped;

	public bool settled => _settled;

	protected override void Awake()
	{
		LateInitializerManager.RegisterLateInitializer(this);
	}

	protected override void Start()
	{
		base.Start();
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		if (!_initialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		if (_originalFragment != null)
		{
			_originalFragment.OnDetachFragment -= OnFragmentDetached;
			_originalFragment.OnChangeSector -= OnFragmentWarped;
			_originalFragment.OnComeToRest -= OnFragmentCameToRest;
		}
	}

	public void SaveFragmentName()
	{
		_realFragmentName = _realObjectTransform.name;
	}

	public void CollectRenderers()
	{
		_renderers = GetComponentsInChildren<MeshRenderer>();
	}

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].enabled = on;
		}
	}

	public void SetRealFragment(DetachableFragment body)
	{
		_originalFragment = body;
		_originalFragment.OnDetachFragment += OnFragmentDetached;
		_originalFragment.OnChangeSector += OnFragmentWarped;
		_originalFragment.OnComeToRest += OnFragmentCameToRest;
		_realObjectTransform = body.transform;
	}

	public void SetProxyBrittleHollow(ProxyBrittleHollow proxyBrittleHollow)
	{
		_proxyBrittleHollow = proxyBrittleHollow;
	}

	public void SetProxyWhiteHole(ProxyWhiteHole proxyWhiteHole)
	{
		_proxyWhiteHole = proxyWhiteHole;
	}

	protected override void Update()
	{
		if (_initialized)
		{
			if (_realObjectTransform == null)
			{
				ToggleRendering(on: false);
				base.enabled = false;
			}
			else if (_detached && !_warped)
			{
				base.transform.localPosition = _proxyBrittleHollow.realObjectTransform.InverseTransformPoint(_realObjectTransform.position);
				base.transform.rotation = _realObjectTransform.rotation;
				base.transform.localScale = _realObjectTransform.lossyScale;
			}
			else if (_detached && _warped && !_settled)
			{
				base.transform.localPosition = _proxyWhiteHole.realObjectTransform.InverseTransformPoint(_realObjectTransform.position);
				base.transform.rotation = _realObjectTransform.rotation;
				base.transform.localScale = _realObjectTransform.lossyScale;
			}
			else
			{
				base.enabled = false;
			}
		}
	}

	private void OnFragmentDetached(OWRigidbody fragmentBody, OWRigidbody attachedBody)
	{
		_detached = true;
		base.transform.parent = _proxyBrittleHollow.transform;
		base.enabled = true;
	}

	private void OnFragmentWarped(Sector newParentSector)
	{
		if (!TryAssignProxyWhiteholeRuntime())
		{
			ToggleRendering(on: false);
			base.enabled = false;
			return;
		}
		_warped = true;
		base.transform.parent = _proxyWhiteHole.transform;
		base.transform.localPosition = Vector3.zero;
		base.transform.rotation = _realObjectTransform.rotation;
		base.transform.localScale = Vector3.one * 0.1f;
		_proxyWhiteHole.AddFragment(this);
	}

	private void OnFragmentCameToRest(OWRigidbody anchorBody)
	{
		_settled = true;
		base.transform.localPosition = _proxyWhiteHole.realObjectTransform.InverseTransformPoint(_realObjectTransform.position);
		base.transform.rotation = _realObjectTransform.rotation;
		base.transform.localScale = Vector3.one;
		base.enabled = false;
	}

	private bool TryAssignProxyWhiteholeRuntime()
	{
		if (_proxyWhiteHole != null)
		{
			return true;
		}
		if (DistantProxyManager.instance != null)
		{
			_proxyWhiteHole = DistantProxyManager.instance.proxyWhiteHole;
		}
		return _proxyWhiteHole != null;
	}
}
