using System.Collections.Generic;
using OW.Utilities.GameObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchLightingOptimizationUtilities
{
	public static void ExecuteAllLightingOptimizationOptionsInScene(Scene scene)
	{
		List<GameObject> allGameObjectsInScene = GameObjectUtilities.GetAllGameObjectsInScene(scene);
		foreach (GameObject item in allGameObjectsInScene)
		{
			item.GetComponent<ISwitchLightingOptimization>()?.Execute(allGameObjectsInScene);
			item.GetComponent<ISwitchTessellationOptimization>()?.Execute();
		}
	}
}
