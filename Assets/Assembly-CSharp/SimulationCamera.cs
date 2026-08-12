using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class SimulationCamera : MonoBehaviour
{
	private Camera _camera;

	private RenderTexture _simulationRenderTexture;

	private Shader _simulationMaskShader;

	private Material _simulationMaskMaterial;

	private CommandBuffer _simulationMaskCommandBuffer;

	private Shader _simulationCompositeShader;

	private Material _simulationCompositeMaterial;

	private CommandBuffer _simulationCompositeCommandBuffer;

	private Material _stencilClearMaterial;

	private CommandBuffer _stencilClearCommandBuffer;

	private OWCamera _targetCamera;

	private void Awake()
	{
		_camera = GetComponent<Camera>();
		_camera.depthTextureMode = DepthTextureMode.Depth;
		_simulationRenderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.sRGB);
		_simulationRenderTexture.name = "SimulationRenderTexture";
		_simulationRenderTexture.useDynamicScale = true;
		_simulationMaskShader = Shader.Find("Hidden/BlitSimulationMask");
		_simulationMaskMaterial = new Material(_simulationMaskShader);
		_simulationMaskMaterial.name = "BlitSimulationMask";
		_simulationMaskCommandBuffer = new CommandBuffer();
		_simulationMaskCommandBuffer.name = "SimulationMask";
		_simulationMaskCommandBuffer.Blit(_simulationRenderTexture, BuiltinRenderTextureType.CameraTarget, _simulationMaskMaterial);
		_simulationCompositeShader = Shader.Find("Hidden/BlitSimulationComposite");
		_simulationCompositeMaterial = new Material(_simulationCompositeShader);
		_simulationCompositeMaterial.name = "BlitSimulationComposite";
		_simulationCompositeCommandBuffer = new CommandBuffer();
		_simulationCompositeCommandBuffer.name = "SimulationComposite";
		_simulationCompositeCommandBuffer.Blit(_simulationRenderTexture, BuiltinRenderTextureType.CameraTarget, _simulationCompositeMaterial);
		bool flag = false;
		if (SecretSettings.TryGetBool("NVidiaDriver522.25Fix", out var value))
		{
			flag = value;
		}
		if (flag)
		{
			_stencilClearMaterial = new Material(Shader.Find("Hidden/InternalClear"));
			_stencilClearMaterial.name = "Clear";
			_stencilClearCommandBuffer = new CommandBuffer();
			_stencilClearCommandBuffer.name = "ClearStencil";
			_stencilClearCommandBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, _stencilClearMaterial, 4);
			_camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _stencilClearCommandBuffer);
		}
		_camera.enabled = false;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (base.enabled && _targetCamera != null)
		{
			_targetCamera.mainCamera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, _simulationMaskCommandBuffer);
			_targetCamera.mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _simulationCompositeCommandBuffer);
		}
		Object.Destroy(_simulationMaskMaterial);
		Object.Destroy(_simulationCompositeMaterial);
		_simulationRenderTexture.Release();
		Object.Destroy(_simulationRenderTexture);
		_simulationRenderTexture = null;
	}

	private void OnEnable()
	{
		_camera.enabled = true;
		if (_targetCamera != null)
		{
			_targetCamera.mainCamera.AddCommandBuffer(CameraEvent.BeforeGBuffer, _simulationMaskCommandBuffer);
			_targetCamera.mainCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, _simulationCompositeCommandBuffer);
		}
		GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);
	}

	private void OnDisable()
	{
		_camera.enabled = false;
		if (_targetCamera != null)
		{
			_targetCamera.mainCamera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, _simulationMaskCommandBuffer);
			_targetCamera.mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _simulationCompositeCommandBuffer);
		}
		GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);
	}

	private void OnPreRender()
	{
		VerifyRenderTexResolution();
	}

	private void AllocateRenderTex()
	{
		VerifyRenderTexResolution();
		_simulationRenderTexture.Create();
		_camera.targetTexture = _simulationRenderTexture;
	}

	private void DeallocateRenderTex()
	{
		_camera.targetTexture = null;
		_simulationRenderTexture.Release();
	}

	private void VerifyRenderTexResolution()
	{
		if (!(_targetCamera == null) && (_simulationRenderTexture.width != _targetCamera.pixelWidth || _simulationRenderTexture.height != _targetCamera.pixelHeight))
		{
			if (_simulationRenderTexture.IsCreated())
			{
				_simulationRenderTexture.Release();
				_simulationRenderTexture.width = _targetCamera.pixelWidth;
				_simulationRenderTexture.height = _targetCamera.pixelHeight;
				_simulationRenderTexture.Create();
			}
			else
			{
				_simulationRenderTexture.width = _targetCamera.pixelWidth;
				_simulationRenderTexture.height = _targetCamera.pixelHeight;
			}
		}
	}

	public void OnEnterDreamWorld()
	{
		AllocateRenderTex();
	}

	public void OnExitDreamWorld()
	{
		DeallocateRenderTex();
	}

	public void SetTargetCamera(OWCamera targetCamera)
	{
		if (base.enabled)
		{
			if (_targetCamera != null)
			{
				_targetCamera.mainCamera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, _simulationMaskCommandBuffer);
				_targetCamera.mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _simulationCompositeCommandBuffer);
			}
			_targetCamera = targetCamera;
			if (_targetCamera != null)
			{
				_targetCamera.mainCamera.AddCommandBuffer(CameraEvent.BeforeGBuffer, _simulationMaskCommandBuffer);
				_targetCamera.mainCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, _simulationCompositeCommandBuffer);
			}
		}
		else
		{
			_targetCamera = targetCamera;
		}
	}

	private void OnSwitchActiveCamera(OWCamera newCamera)
	{
		SetTargetCamera(newCamera);
	}

	public void UpdateCamera()
	{
		if (!(_targetCamera == null))
		{
			_camera.fieldOfView = _targetCamera.mainCamera.fieldOfView;
			_camera.transform.position = _targetCamera.transform.position;
			_camera.transform.rotation = _targetCamera.transform.rotation;
		}
	}
}
