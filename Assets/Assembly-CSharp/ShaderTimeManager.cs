using System;
using UnityEngine;

public class ShaderTimeManager : MonoBehaviour
{
	private int _propID_FixedTime;

	private void Awake()
	{
		_propID_FixedTime = Shader.PropertyToID("_FixedTime");
		Shader.EnableKeyword("_FIXEDTIME_AVAILABLE");
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(UpdateFixedTime));
	}

	private void OnDestroy()
	{
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(UpdateFixedTime));
		Shader.DisableKeyword("_FIXEDTIME_AVAILABLE");
	}

	private void UpdateFixedTime(Camera camera)
	{
		Shader.SetGlobalFloat(_propID_FixedTime, Time.fixedTime);
	}
}
