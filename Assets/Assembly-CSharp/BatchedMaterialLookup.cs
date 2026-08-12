using UnityEngine;

public class BatchedMaterialLookup : MonoBehaviour
{
	public Material[] materials;

	private void Reset()
	{
		MeshCollider component = GetComponent<MeshCollider>();
		if (component != null && component.sharedMesh != null)
		{
			materials = new Material[component.sharedMesh.subMeshCount];
		}
	}

	[ContextMenu("Copy Materials from Renderer", true)]
	private bool ValidateCopyMaterialsFromRenderer()
	{
		return GetComponent<Renderer>() != null;
	}

	[ContextMenu("Copy Materials from Renderer", false)]
	private void CopyMaterialsFromRenderer()
	{
		materials = GetComponent<Renderer>().sharedMaterials;
	}
}
