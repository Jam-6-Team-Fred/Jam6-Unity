using UnityEngine;

public class ChildColliderSettings : MonoBehaviour
{
	[SerializeField]
	private DynamicOccupantMask _lodActivationMask;

	private void Awake()
	{
		if (GetComponentsInChildren<ChildColliderSettings>().Length > 1)
		{
			Debug.LogError("Found additional ChildColliderSettings scripts in children...destroying this one so they don't conflict.", base.gameObject);
			Object.Destroy(this);
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].GetComponent<OWCollider>() == null)
			{
				componentsInChildren[i].gameObject.AddComponent<OWCollider>().SetLODActivationMask(_lodActivationMask.GetMask());
			}
		}
	}
}
