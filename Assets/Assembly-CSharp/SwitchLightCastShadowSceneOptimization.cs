using System.Collections.Generic;
using UnityEngine;

public class SwitchLightCastShadowSceneOptimization : MonoBehaviour, ISwitchLightingOptimization
{
	public bool skip;

	[Header("Explicit Light Components")]
	public Light[] sceneLightComponents = new Light[0];

	public Light[] prefabLightComponents = new Light[0];

	[Header("Switch Light Component Override Settings")]
	public bool operateOnPrefabLights;

	public bool disableShadows = true;

	public void Execute(List<GameObject> sceneGameObjects)
	{
		base.hideFlags = HideFlags.DontSaveInBuild;
		if (!skip)
		{
			Light[] array = sceneLightComponents;
			foreach (Light lightComponent in array)
			{
				ProcessLightComponent(lightComponent);
			}
			if (operateOnPrefabLights)
			{
				array = prefabLightComponents;
				foreach (Light lightComponent2 in array)
				{
					ProcessLightComponent(lightComponent2);
				}
			}
		}
		Object.DestroyImmediate(this);
	}

	private void ProcessLightComponent(Light lightComponent)
	{
		if (!(lightComponent == null) && (lightComponent.type == LightType.Point || lightComponent.type == LightType.Spot))
		{
			lightComponent.shadows = ((!disableShadows) ? lightComponent.shadows : LightShadows.None);
		}
	}
}
