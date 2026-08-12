using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class SwitchShadowCasterSceneOptimization : MonoBehaviour, ISwitchLightingOptimization
{
	public bool skip;

	[Header("Indiscriminate Shadow Disabling")]
	public bool disableAllShadowCasting;

	[Header("Disable if Dimensions are Below:")]
	public Vector3 minBoundsSize = new Vector3(0.5f, 0.5f, 0.5f);

	public float minBoundsVolume = 0.125f;

	[Header("Disable if GO Name Contains:")]
	public string[] nameSearchStrings = new string[0];

	[Header("Disable if Shader Name Contains:")]
	public string[] shaderSearchStrings = new string[0];

	[Header("Additional Options")]
	public bool disableProbeBlending = true;

	public bool disableLightProbes = true;

	public bool disableParticleShadows = true;

	[Header("Exceptions (Keep Shadows on These)")]
	public GameObject[] exceptions = new GameObject[0];

	public void Execute(List<GameObject> sceneGameObjects)
	{
		base.hideFlags = HideFlags.DontSaveInBuild;
		if (!skip)
		{
			foreach (GameObject go in sceneGameObjects)
			{
				if (!(go != null) || Enumerable.Contains(exceptions, go))
				{
					continue;
				}
				Renderer component = go.GetComponent<Renderer>();
				if (!component)
				{
					continue;
				}
				if (disableProbeBlending)
				{
					component.reflectionProbeUsage = ReflectionProbeUsage.Simple;
				}
				if (disableLightProbes)
				{
					component.lightProbeUsage = LightProbeUsage.Off;
				}
				if (disableParticleShadows && component is ParticleSystemRenderer)
				{
					component.shadowCastingMode = ShadowCastingMode.Off;
					continue;
				}
				if (disableAllShadowCasting)
				{
					component.shadowCastingMode = ShadowCastingMode.Off;
					continue;
				}
				Vector3 size = component.bounds.size;
				if (size.x < minBoundsSize.x || size.y < minBoundsSize.y || size.z < minBoundsSize.z || size.x * size.y * size.z < minBoundsVolume)
				{
					component.shadowCastingMode = ShadowCastingMode.Off;
					continue;
				}
				if (Enumerable.Any(nameSearchStrings, (string str) => StringContainsCaseInsensitive(go.name, str)))
				{
					component.shadowCastingMode = ShadowCastingMode.Off;
					continue;
				}
				Material[] sharedMaterials = component.sharedMaterials;
				foreach (Material sharedMaterial in sharedMaterials)
				{
					if ((bool)sharedMaterial && (bool)sharedMaterial.shader && Enumerable.Any(shaderSearchStrings, (string str) => StringContainsCaseInsensitive(sharedMaterial.shader.name, str)))
					{
						component.shadowCastingMode = ShadowCastingMode.Off;
						break;
					}
				}
			}
		}
		UnityEngine.Object.DestroyImmediate(this);
	}

	private bool StringContainsCaseInsensitive(string str, string searchStr)
	{
		return str.IndexOf(searchStr, StringComparison.CurrentCultureIgnoreCase) >= 0;
	}
}
