using OW.Utilities.GameObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MeshOptimizationOptionsUtilities
{
	public static void RemoveAllMeshOptimizationOptionsInScene(Scene scene)
	{
		foreach (GameObject item in GameObjectUtilities.GetAllGameObjectsInScene(scene))
		{
			MeshOptimizationOptions component = item.GetComponent<MeshOptimizationOptions>();
			if (component != null)
			{
				Object.DestroyImmediate(component);
			}
		}
	}
}
