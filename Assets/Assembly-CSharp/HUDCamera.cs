using UnityEngine;

[RequireComponent(typeof(Camera))]
public class HUDCamera : MonoBehaviour
{
	private Camera _camera;

	[SerializeField]
	private Material _hudMaterial;

	private float _cameraInitFieldOfView;

	private RenderTexture _hudRenderTex;

	private bool _activated;

	private bool _suspended;

	private bool _keepDeallocated;

	private void Awake()
	{
		_camera = GetComponent<Camera>();
		_cameraInitFieldOfView = _camera.fieldOfView;
		_camera.enabled = false;
		Vector2 vector = new Vector2(1920f, 1080f);
		float num = (float)Screen.width / (float)Screen.height;
		if (!OWMath.ApproxEquals(num, 1.777f, 0.01f))
		{
			vector.x *= num / 1.7777778f;
			_camera.fieldOfView = _cameraInitFieldOfView * (1.7777778f / num);
		}
		_hudRenderTex = new RenderTexture(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), 16, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		_hudRenderTex.name = "HelmetHUD";
		_hudRenderTex.Create();
		_camera.targetTexture = _hudRenderTex;
		_hudMaterial.SetTexture("_MainTex", _hudRenderTex);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _camera.targetTexture;
		GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
		RenderTexture.active = active;
		GlobalMessenger.AddListener("HelmetHUDActivated", ActivateHUD);
		GlobalMessenger.AddListener("RemoveHelmet", DeactivateHUD);
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);
		GlobalMessenger<GraphicSettings>.AddListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
	}

	private void OnDestroy()
	{
		_hudRenderTex.Release();
		Object.Destroy(_hudRenderTex);
		_hudRenderTex = null;
		GlobalMessenger.RemoveListener("HelmetHUDActivated", ActivateHUD);
		GlobalMessenger.RemoveListener("RemoveHelmet", DeactivateHUD);
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);
		GlobalMessenger<GraphicSettings>.RemoveListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
	}

	private void OnPreCull()
	{
		QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
	}

	private void OnPostRender()
	{
		QualitySettings.shadows = UnityEngine.ShadowQuality.All;
	}

	private void OnSwitchActiveCamera(OWCamera camera)
	{
		if (camera.CompareTag("MainCamera"))
		{
			ResumeHUDRendering();
		}
		else
		{
			SuspendHUDRendering();
		}
	}

	private void OnGraphicSettingsUpdated(GraphicSettings graphicSettings)
	{
		Vector2 vector = new Vector2(1920f, 1080f);
		float num = (float)graphicSettings.displayResWidth / (float)graphicSettings.displayResHeight;
		if (!OWMath.ApproxEquals(num, 1.777f, 0.01f))
		{
			vector.x *= num / 1.7777778f;
			_camera.fieldOfView = _cameraInitFieldOfView * (1.7777778f / num);
		}
		else
		{
			_camera.fieldOfView = _cameraInitFieldOfView;
		}
		int num2 = Mathf.RoundToInt(vector.x);
		int num3 = Mathf.RoundToInt(vector.y);
		if (_hudRenderTex.width != num2 || _hudRenderTex.height != num3)
		{
			_hudRenderTex.Release();
			_hudRenderTex.width = num2;
			_hudRenderTex.height = num3;
			if (!_keepDeallocated)
			{
				_hudRenderTex.Create();
			}
		}
		_camera.ResetAspect();
	}

	private void ActivateHUD()
	{
		_activated = true;
		_camera.enabled = _activated && !_suspended;
	}

	private void DeactivateHUD()
	{
		_activated = false;
		_camera.enabled = _activated && !_suspended;
	}

	private void SuspendHUDRendering()
	{
		_suspended = true;
		_camera.enabled = _activated && !_suspended;
	}

	private void ResumeHUDRendering()
	{
		_suspended = false;
		_camera.enabled = _activated && !_suspended;
	}

	private void OnEnterDreamWorld()
	{
		_keepDeallocated = true;
		_hudRenderTex.Release();
	}

	private void OnExitDreamWorld()
	{
		_keepDeallocated = false;
		if (!_hudRenderTex.IsCreated())
		{
			_hudRenderTex.Create();
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = _camera.targetTexture;
			GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
			RenderTexture.active = active;
		}
	}
}
