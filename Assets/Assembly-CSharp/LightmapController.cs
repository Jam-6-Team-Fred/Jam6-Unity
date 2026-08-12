using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class LightmapController : MonoBehaviour
{
	public enum LightmapChannel
	{
		Red = 0,
		Green = 1,
		Blue = 2,
		Alpha = 3
	}

	private Light _light;

	[SerializeField]
	private Material[] _materials = new Material[0];

	[SerializeField]
	private LightmapChannel _channel;

	private int _propID_LightColor;

	private int _propID_LightPos;

	private void OnValidate()
	{
		CachePropertyIDs();
	}

	private void Awake()
	{
		_light = GetComponent<Light>();
		CachePropertyIDs();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(UpdateLightmapSettings));
	}

	private void OnDestroy()
	{
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(UpdateLightmapSettings));
	}

	private void CachePropertyIDs()
	{
		string text = ((_channel == LightmapChannel.Red) ? "R" : ((_channel == LightmapChannel.Green) ? "G" : ((_channel != LightmapChannel.Blue) ? "A" : "B")));
		_propID_LightColor = Shader.PropertyToID("_LightColor" + text);
		_propID_LightPos = Shader.PropertyToID("_LightPos" + text);
	}

	private void UpdateLightmapSettings(Camera camera)
	{
		if (camera.cameraType == CameraType.Preview)
		{
			return;
		}
		for (int i = 0; i < _materials.Length; i++)
		{
			if (!(_materials[i] == null))
			{
				float num = (_light.isActiveAndEnabled ? 1f : 0f);
				_materials[i].SetVector(_propID_LightColor, _light.color * _light.intensity * num);
				_materials[i].SetVector(_propID_LightPos, _light.transform.position);
			}
		}
	}
}
