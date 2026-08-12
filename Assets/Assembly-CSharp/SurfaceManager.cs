using System.Collections.Generic;
using UnityEngine;

public class SurfaceManager : MonoBehaviour
{
	[SerializeField]
	private SurfaceLookupTable _surfaceLookupAsset;

	private Dictionary<Material, SurfaceType> _lookupTable;

	private void Awake()
	{
		_lookupTable = (_surfaceLookupAsset ? _surfaceLookupAsset.BuildDictionary() : new Dictionary<Material, SurfaceType>(0));
	}

	public int GetHitSubmesh(RaycastHit hitInfo)
	{
		MeshCollider meshCollider = hitInfo.collider as MeshCollider;
		if (meshCollider == null)
		{
			return 0;
		}
		Mesh sharedMesh = meshCollider.sharedMesh;
		int subMeshCount = sharedMesh.subMeshCount;
		if (subMeshCount > 1)
		{
			int num = hitInfo.triangleIndex * 3;
			for (int i = 0; i < subMeshCount - 1; i++)
			{
				if (num < sharedMesh.GetIndexStart(i + 1))
				{
					return i;
				}
			}
			return subMeshCount - 1;
		}
		return 0;
	}

	public Material GetHitMaterial(RaycastHit hitInfo)
	{
		Renderer component = hitInfo.collider.GetComponent<Renderer>();
		BatchedMaterialLookup component2 = hitInfo.collider.GetComponent<BatchedMaterialLookup>();
		if (component == null && component2 == null)
		{
			return null;
		}
		int hitSubmesh = GetHitSubmesh(hitInfo);
		if ((bool)component2)
		{
			return component2.materials[hitSubmesh];
		}
		return component.sharedMaterials[hitSubmesh];
	}

	public SurfaceType GetHitSurfaceType(RaycastHit hitInfo)
	{
		return GetSurfaceType(GetHitMaterial(hitInfo));
	}

	public SurfaceType GetSurfaceType(Material material)
	{
		if (material == null)
		{
			return SurfaceType.None;
		}
		if (_lookupTable.TryGetValue(material, out var value))
		{
			return value;
		}
		return SurfaceType.None;
	}
}
