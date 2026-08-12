using System.Collections.Generic;
using UnityEngine;

public static class VolumeOcclusionManager
{
	private static List<VolumeOcclusionRenderer> s_activeOcclusionVolumes = new List<VolumeOcclusionRenderer>(32);

	private static List<VolumeOcclusionLight> s_activeLights = new List<VolumeOcclusionLight>(32);

	private static List<VolumeOcclusionRenderer> s_culledOcclusionVolumes = new List<VolumeOcclusionRenderer>(32);

	private static List<VolumeOcclusionLight> s_culledLights = new List<VolumeOcclusionLight>(32);

	public static bool HasActiveOcclusionVolumes()
	{
		return s_activeOcclusionVolumes.Count > 0;
	}

	public static List<VolumeOcclusionRenderer> GetActiveOcclusionVolumeList()
	{
		return s_activeOcclusionVolumes;
	}

	public static List<VolumeOcclusionLight> GetActiveLightList()
	{
		return s_activeLights;
	}

	public static List<VolumeOcclusionRenderer> GetCulledOcclusionVolumeList(Plane[] frustumPlanes)
	{
		s_culledOcclusionVolumes.Clear();
		List<VolumeOcclusionRenderer> activeOcclusionVolumeList = GetActiveOcclusionVolumeList();
		for (int i = 0; i < activeOcclusionVolumeList.Count; i++)
		{
			VolumeOcclusionRenderer volumeOcclusionRenderer = activeOcclusionVolumeList[i];
			if (volumeOcclusionRenderer.mesh == null || volumeOcclusionRenderer.occlusionStrength <= 0f)
			{
				continue;
			}
			bool flag = false;
			Vector4 vector = volumeOcclusionRenderer.CalcWorldBounds();
			for (int j = 0; j < frustumPlanes.Length; j++)
			{
				if (Vector3.Dot(vector, frustumPlanes[j].normal) + frustumPlanes[j].distance < 0f - vector.w)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				s_culledOcclusionVolumes.Add(volumeOcclusionRenderer);
			}
		}
		return s_culledOcclusionVolumes;
	}

	public static List<VolumeOcclusionLight> GetCulledLightList(Plane[] frustumPlanes)
	{
		s_culledLights.Clear();
		List<VolumeOcclusionLight> activeLightList = GetActiveLightList();
		for (int i = 0; i < activeLightList.Count; i++)
		{
			VolumeOcclusionLight volumeOcclusionLight = activeLightList[i];
			if (volumeOcclusionLight.intensity <= 0f || volumeOcclusionLight.range <= 0f)
			{
				continue;
			}
			bool flag = false;
			Vector4 vector = volumeOcclusionLight.CalcWorldBounds();
			for (int j = 0; j < frustumPlanes.Length; j++)
			{
				if (Vector3.Dot(vector, frustumPlanes[j].normal) + frustumPlanes[j].distance < 0f - vector.w)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				s_culledLights.Add(volumeOcclusionLight);
			}
		}
		return s_culledLights;
	}

	public static void RegisterVolumeOcclusionRenderer(VolumeOcclusionRenderer volumeOcclusionRenderer)
	{
		s_activeOcclusionVolumes.Add(volumeOcclusionRenderer);
	}

	public static void UnregisterVolumeOcclusionRenderer(VolumeOcclusionRenderer volumeOcclusionRenderer)
	{
		s_activeOcclusionVolumes.Remove(volumeOcclusionRenderer);
	}

	public static void RegisterVolumeOcclusionLight(VolumeOcclusionLight volumeOcclusionLight)
	{
		s_activeLights.Add(volumeOcclusionLight);
	}

	public static void UnregisterVolumeOcclusionLight(VolumeOcclusionLight volumeOcclusionLight)
	{
		s_activeLights.Remove(volumeOcclusionLight);
	}
}
