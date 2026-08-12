using UnityEngine;
using UnityEngine.Rendering;

public class PerSectorShadowCastingStateGroup : MonoBehaviour
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private ShadowCastingMode _shadowCastingMode = ShadowCastingMode.On;

	private void Awake()
	{
		AddToRenderersInHierarchy_Recursive(base.transform);
	}

	private void AddToRenderersInHierarchy_Recursive(Transform parent)
	{
		if (parent.GetComponent<Renderer>() != null && parent.GetComponent<PerSectorShadowCastingState>() == null)
		{
			PerSectorShadowCastingState perSectorShadowCastingState = parent.gameObject.AddComponent<PerSectorShadowCastingState>();
			perSectorShadowCastingState.SetShadowCastingMode(_shadowCastingMode);
			perSectorShadowCastingState.SetSector(_sector);
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			AddToRenderersInHierarchy_Recursive(parent.GetChild(i));
		}
	}
}
