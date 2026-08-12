using UnityEngine;
using UnityEngine.Rendering;

public class ProxyShadowCasterGroup : MonoBehaviour
{
	[EnumFlags]
	[SerializeField]
	private ProxyShadowCascade.Flags _cascadeFlags = (ProxyShadowCascade.Flags)(-1);

	[SerializeField]
	private bool _earlyDraw;

	[SerializeField]
	private bool _dynamic;

	private void Awake()
	{
		AddProxyShadowCastersToChildren();
	}

	private void AddProxyShadowCastersToChildren()
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].shadowCastingMode == ShadowCastingMode.Off)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < componentsInChildren[i].sharedMaterials.Length; j++)
			{
				if (componentsInChildren[i].sharedMaterials[j].GetTag("RenderType", searchFallbacks: true, "Opaque") != "Transparent" && componentsInChildren[i].sharedMaterials[j].GetTag("ForceNoShadowCasting", searchFallbacks: true, "False") != "True" && componentsInChildren[i].sharedMaterials[j].GetTag("FORCENOSHADOWCASTING", searchFallbacks: true, "False") != "true")
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				ProxyShadowCaster proxyShadowCaster = componentsInChildren[i].gameObject.AddComponent<ProxyShadowCaster>();
				proxyShadowCaster.enabled = false;
				proxyShadowCaster.SetCascadeFlags(_cascadeFlags);
				proxyShadowCaster.SetEarlyDraw(_earlyDraw);
				proxyShadowCaster.SetDynamic(_dynamic);
				proxyShadowCaster.enabled = true;
			}
		}
	}
}
