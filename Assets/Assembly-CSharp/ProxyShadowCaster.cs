using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ProxyShadowCaster : MonoBehaviour
{
	private MeshFilter _meshFilter;

	private MeshRenderer _meshRenderer;

	private ProxyShadowCasterSuperGroup _superGroup;

	private StreamingRenderMeshHandle _streamingMeshHandle;

	[EnumFlags]
	[SerializeField]
	private ProxyShadowCascade.Flags _cascadeFlags = (ProxyShadowCascade.Flags)(-1);

	[SerializeField]
	private bool _earlyDraw;

	[SerializeField]
	private bool _dynamic;

	public ProxyShadowCasterSuperGroup superGroup => _superGroup;

	public ProxyShadowCascade.Flags cascadeFlags => _cascadeFlags;

	public bool near => (_cascadeFlags & ProxyShadowCascade.Flags.Near) > (ProxyShadowCascade.Flags)0;

	public bool far => (_cascadeFlags & ProxyShadowCascade.Flags.Final) > (ProxyShadowCascade.Flags)0;

	public bool earlyDraw => _earlyDraw;

	public bool dynamic => _dynamic;

	public Mesh mesh => _meshFilter.sharedMesh;

	public int subMeshCount => _meshFilter.sharedMesh.subMeshCount;

	public Matrix4x4 localToWorldMatrix => _meshRenderer.localToWorldMatrix;

	public Matrix4x4 localToGroupMatrix => _superGroup.transform.worldToLocalMatrix * _meshRenderer.localToWorldMatrix;

	public MeshRenderer meshRenderer => _meshRenderer;

	private void Awake()
	{
		_meshFilter = GetComponent<MeshFilter>();
		_meshRenderer = GetComponent<MeshRenderer>();
		Transform transform = base.transform;
		while (transform != null)
		{
			_superGroup = transform.GetComponent<ProxyShadowCasterSuperGroup>();
			if (_superGroup != null)
			{
				break;
			}
			OWRigidbody component = transform.GetComponent<OWRigidbody>();
			if (component != null)
			{
				if (!(component.GetOrigParent() != null))
				{
					break;
				}
				transform = component.GetOrigParent();
			}
			else
			{
				transform = transform.parent;
			}
		}
		if (_superGroup == null)
		{
			Debug.LogError("ProxyShadowCaster found with no parent SuperGroup!", this);
		}
		_streamingMeshHandle = GetComponent<StreamingRenderMeshHandle>();
		if (_streamingMeshHandle != null)
		{
			_streamingMeshHandle.OnMeshLoaded += OnMeshLoaded;
			_streamingMeshHandle.OnMeshUnloaded += OnMeshUnloaded;
		}
		base.enabled = _meshFilter != null && _meshFilter.sharedMesh != null;
	}

	private void OnDestroy()
	{
		if (_streamingMeshHandle != null)
		{
			_streamingMeshHandle.OnMeshLoaded -= OnMeshLoaded;
			_streamingMeshHandle.OnMeshUnloaded -= OnMeshUnloaded;
		}
	}

	private void OnEnable()
	{
		if (_meshFilter != null && _meshRenderer != null && _superGroup != null)
		{
			_superGroup.AddShadowCaster(this);
		}
	}

	private void OnDisable()
	{
		if (_superGroup != null)
		{
			_superGroup.RemoveShadowCaster(this);
		}
	}

	private void OnMeshLoaded()
	{
		base.enabled = true;
	}

	private void OnMeshUnloaded()
	{
		base.enabled = false;
	}

	public void SetSuperGroup(ProxyShadowCasterSuperGroup superGroup)
	{
		if (!(_superGroup == superGroup))
		{
			if (!base.enabled || !base.gameObject.activeInHierarchy)
			{
				_superGroup = superGroup;
				return;
			}
			OnDisable();
			_superGroup = superGroup;
			OnEnable();
		}
	}

	public void SetCascadeFlags(ProxyShadowCascade.Flags cascadeFlags)
	{
		if (_cascadeFlags != cascadeFlags)
		{
			if (!base.enabled || !base.gameObject.activeInHierarchy)
			{
				_cascadeFlags = cascadeFlags;
				return;
			}
			OnDisable();
			_cascadeFlags = cascadeFlags;
			OnEnable();
		}
	}

	public void SetEarlyDraw(bool earlyDraw)
	{
		if (_earlyDraw != earlyDraw)
		{
			if (!base.enabled || !base.gameObject.activeInHierarchy)
			{
				_earlyDraw = earlyDraw;
				return;
			}
			OnDisable();
			_earlyDraw = earlyDraw;
			OnEnable();
		}
	}

	public void SetDynamic(bool dynamic)
	{
		_dynamic = dynamic;
		if (_superGroup != null)
		{
			_superGroup.UpdateDynamicFlag(this);
		}
	}
}
