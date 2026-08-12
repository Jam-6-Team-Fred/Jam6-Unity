using Tessellation;
using UnityEngine;

public class SwitchTessellationSceneOptimization : MonoBehaviour, ISwitchTessellationOptimization
{
	public bool skip;

	[Header("Switch-Specific Settings")]
	[SerializeField]
	protected MeshGroup _tessellationMeshGroup;

	[SerializeField]
	protected int _maxLOD = 8;

	[SerializeField]
	protected int _LODBias;

	[SerializeField]
	protected float _LODRadius = 1f;

	public void Execute()
	{
		base.hideFlags = HideFlags.DontSaveInBuild;
		if (!skip)
		{
			TessellatedRenderer component = GetComponent<TessellatedRenderer>();
			if ((bool)component)
			{
				component.tessellationMeshGroup = _tessellationMeshGroup;
				component.maxLOD = _maxLOD;
				component.LODBias = _LODBias;
				component.LODRadius = _LODRadius;
			}
		}
		Object.DestroyImmediate(this);
	}
}
