using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OW.Utilities.GameObjects
{
	public class GameObjectUtilities
	{
		public static List<GameObject> GetAllGameObjectsInScene(Scene scene, bool includeInactive = true)
		{
			List<GameObject> list = new List<GameObject>();
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				Transform[] componentsInChildren = rootGameObjects[i].GetComponentsInChildren<Transform>(includeInactive);
				foreach (Transform transform in componentsInChildren)
				{
					if (!list.Contains(transform.gameObject))
					{
						list.Add(transform.gameObject);
					}
				}
			}
			return list;
		}

		public static List<GameObject> GetAllGameObjectsInSceneWithMeshFilter(Scene scene)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (GameObject item in GetAllGameObjectsInScene(scene))
			{
				if (!(item.GetComponent<MeshFilter>() == null))
				{
					list.Add(item);
				}
			}
			return list;
		}

		public static List<GameObject> GetAllGameObjectsInSceneWithMeshCollider(Scene scene)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (GameObject item in GetAllGameObjectsInScene(scene))
			{
				if (!(item.GetComponent<MeshCollider>() == null))
				{
					list.Add(item);
				}
			}
			return list;
		}

		public static List<GameObject> GetAllGameObjectsInSceneWithMeshFilterOrMeshCollider(Scene scene)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (GameObject item in GetAllGameObjectsInScene(scene))
			{
				MeshCollider component = item.GetComponent<MeshCollider>();
				MeshFilter component2 = item.GetComponent<MeshFilter>();
				if (!(component == null) || !(component2 == null))
				{
					list.Add(item);
				}
			}
			return list;
		}

		public static List<GameObject> GetAllDescendants(GameObject go)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (Transform item in go.transform)
			{
				list.Add(item.gameObject);
				list.AddRange(GetAllDescendants(item.gameObject));
			}
			return list;
		}
	}
}
