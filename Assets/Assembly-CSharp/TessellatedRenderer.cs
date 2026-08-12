using Tessellation;
using UnityEngine;

public abstract class TessellatedRenderer : MonoBehaviour
{
	[SerializeField]
	protected MeshGroup _tessellationMeshGroup;

	[SerializeField]
	protected Material[] _materials = new Material[1];

	[SerializeField]
	protected Patch.CullingMode _cullingMode = Patch.CullingMode.Normal;

	[Space]
	[SerializeField]
	protected int _maxLOD = 8;

	[SerializeField]
	protected int _LODBias;

	[SerializeField]
	protected float _LODRadius = 1f;

	public MeshGroup tessellationMeshGroup
	{
		get
		{
			return _tessellationMeshGroup;
		}
		set
		{
			_tessellationMeshGroup = value;
		}
	}

	public Material sharedMaterial
	{
		get
		{
			if (_materials.Length == 0)
			{
				return null;
			}
			return _materials[0];
		}
		set
		{
			if (_materials.Length == 0)
			{
				_materials = new Material[1];
			}
			_materials[0] = value;
		}
	}

	public Material[] sharedMaterials
	{
		get
		{
			return _materials;
		}
		set
		{
			_materials = ((value != null) ? value : new Material[0]);
		}
	}

	public Patch.CullingMode cullingMode
	{
		get
		{
			return _cullingMode;
		}
		set
		{
			_cullingMode = value;
		}
	}

	public int maxLOD
	{
		get
		{
			return _maxLOD;
		}
		set
		{
			_maxLOD = Mathf.Max(value, 0);
		}
	}

	public int LODBias
	{
		get
		{
			return _LODBias;
		}
		set
		{
			_LODBias = Mathf.Max(value, 0);
		}
	}

	public float LODRadius
	{
		get
		{
			return _LODRadius;
		}
		set
		{
			_LODRadius = Mathf.Max(value, 0f);
		}
	}

	protected virtual void OnValidate()
	{
		if ((bool)_tessellationMeshGroup)
		{
			ReconfigureMeshBounds();
		}
		if (_maxLOD < 0)
		{
			_maxLOD = 0;
		}
		if (_LODBias < 0)
		{
			_LODBias = 0;
		}
		if (_LODRadius < 0f)
		{
			_LODRadius = 0f;
		}
	}

	protected virtual void OnEnable()
	{
		if ((bool)_tessellationMeshGroup)
		{
			ReconfigureMeshBounds();
		}
		OWCamera.onAnyPreCull += new OWEvent<OWCamera>.OWCallback(Rebuild);
		OWCamera.onAnyPostRender += new OWEvent<OWCamera>.OWCallback(Clear);
	}

	protected virtual void OnDisable()
	{
		OWCamera.onAnyPreCull -= new OWEvent<OWCamera>.OWCallback(Rebuild);
		OWCamera.onAnyPostRender -= new OWEvent<OWCamera>.OWCallback(Clear);
	}

	protected abstract void ReconfigureMeshBounds();

	protected abstract void Rebuild(OWCamera owCamera);

	protected abstract void Clear(OWCamera owCamera);

	protected bool ShouldRenderInCamera(OWCamera owCamera)
	{
		if ((owCamera.cullingMask & (1 << base.gameObject.layer)) == 0)
		{
			return false;
		}
		return true;
	}
}
