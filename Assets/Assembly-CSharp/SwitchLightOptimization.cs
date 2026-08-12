using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SwitchLightOptimization : MonoBehaviour, ISwitchLightingOptimization
{
	public bool skip;

	[Header("Lighting Settings")]
	public bool disableShadows = true;

	public void Execute(List<GameObject> sceneGameObjects)
	{
		base.hideFlags = HideFlags.DontSaveInBuild;
		if (!skip)
		{
			Light component = GetComponent<Light>();
			if (component != null)
			{
				component.shadows = ((!disableShadows) ? component.shadows : LightShadows.None);
			}
		}
		Object.DestroyImmediate(this);
	}
}
