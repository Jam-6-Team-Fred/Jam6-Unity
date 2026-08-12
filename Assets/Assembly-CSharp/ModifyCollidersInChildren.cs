using UnityEngine;

public class ModifyCollidersInChildren : MonoBehaviour
{
	private enum ColliderOperation
	{
		disableAll = 0,
		meshToSphereColliders = 1,
		makeTriggers = 2
	}

	[SerializeField]
	private ColliderOperation _colliderOperation;

	private void Awake()
	{
		switch (_colliderOperation)
		{
		case ColliderOperation.disableAll:
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			break;
		}
		case ColliderOperation.meshToSphereColliders:
		{
			MeshCollider[] componentsInChildren2 = GetComponentsInChildren<MeshCollider>();
			foreach (MeshCollider obj in componentsInChildren2)
			{
				obj.gameObject.AddComponent<SphereCollider>();
				Object.Destroy(obj);
			}
			break;
		}
		case ColliderOperation.makeTriggers:
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].isTrigger = true;
			}
			break;
		}
		}
	}
}
