using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

// CHANGED
[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class OWCamera : MonoBehaviour
{
	protected Camera _mainCamera;

	protected Camera _farCamera;

	protected PlanetaryFogImageEffect _planetaryFog;

	protected PostProcessingBehaviour _postProcessing;

	protected PostProcessingGameplaySettings _postProcessingSettings;

	[SerializeField]
	protected bool _useFarCamera;

	[SerializeField]
	protected float _farCameraDist = 100000f;

	[FormerlySerializedAs("_useSkyboxCamera")]
	[SerializeField]
	protected bool _renderSkybox;

	[SerializeField]
	protected bool _useViewmodels;

	[SerializeField]
	protected float _viewmodelFOV = 70f;

	private bool _enabled = true;

	private CameraClearFlags _mainCameraOriginalClearFlags;

	private CommandBuffer _skyboxCommandBuffer;

	private CommandBuffer _grabPassCommandBuffer;

	private int _propID_OpaqueGrabTex = Shader.PropertyToID("_OpaqueGrabTex");

	private int _propID_ViewmodelMatrixVP = Shader.PropertyToID("_ViewmodelMatrixVP");

	private static Material s_stencilClearMat;

	private static CommandBuffer _stencilClearCommandBuffer;

	private bool _frustumDirty = true;

	private Plane[] _frustumPlanes = new Plane[6];

	public OWEvent<OWCamera> onThisPreCull;

	public static OWEvent<OWCamera> onAnyPreCull;

	public OWEvent<OWCamera> onThisPreRender;

	public static OWEvent<OWCamera> onAnyPreRender;

	public OWEvent<OWCamera> onThisPostRender;

	public static OWEvent<OWCamera> onAnyPostRender;

	public PlanetaryFogImageEffect planetaryFog
	{
		get
		{
			if (_planetaryFog == null)
			{
				_planetaryFog = GetComponent<PlanetaryFogImageEffect>();
			}
			return _planetaryFog;
		}
	}

	public PostProcessingBehaviour postProcessing
	{
		get
		{
			if (_postProcessing == null)
			{
				_postProcessing = GetComponent<PostProcessingBehaviour>();
			}
			return _postProcessing;
		}
	}

	public PostProcessingGameplaySettings postProcessingSettings => _postProcessingSettings;

	public new bool enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			SetEnabled(value);
		}
	}

	public Camera mainCamera
	{
		get
		{
			if (_mainCamera == null)
			{
				_mainCamera = this.GetRequiredComponent<Camera>();
			}
			return _mainCamera;
		}
	}

	public Camera farCamera => _farCamera;

	public bool useFarCamera
	{
		get
		{
			return _useFarCamera;
		}
		set
		{
			_useFarCamera = value;
		}
	}

	public float farCameraDistance
	{
		get
		{
			return _farCameraDist;
		}
		set
		{
			_farCameraDist = Mathf.Max(0.01f, value);
		}
	}

	public bool renderSkybox
	{
		get
		{
			return _renderSkybox;
		}
		set
		{
			_renderSkybox = value;
		}
	}

	public bool useViewmodels
	{
		get
		{
			return _useViewmodels;
		}
		set
		{
			_useViewmodels = value;
		}
	}

	public float viewmodelFOV
	{
		get
		{
			return _viewmodelFOV;
		}
		set
		{
			_viewmodelFOV = Mathf.Clamp(value, 1f, 179f);
		}
	}

	public float aspect
	{
		get
		{
			return mainCamera.aspect;
		}
		set
		{
			mainCamera.aspect = value;
			_frustumDirty = true;
		}
	}

	public Color backgroundColor
	{
		get
		{
			return mainCamera.backgroundColor;
		}
		set
		{
			mainCamera.backgroundColor = value;
		}
	}

	public CameraClearFlags clearFlags
	{
		get
		{
			return mainCamera.clearFlags;
		}
		set
		{
			mainCamera.clearFlags = value;
		}
	}

	public int cullingMask
	{
		get
		{
			return mainCamera.cullingMask;
		}
		set
		{
			mainCamera.cullingMask = value;
		}
	}

	public float nearClipPlane
	{
		get
		{
			return mainCamera.nearClipPlane;
		}
		set
		{
			mainCamera.nearClipPlane = value;
			_frustumDirty = true;
		}
	}

	public float farClipPlane
	{
		get
		{
			return mainCamera.farClipPlane;
		}
		set
		{
			mainCamera.farClipPlane = value;
			_frustumDirty = true;
		}
	}

	public float fieldOfView
	{
		get
		{
			return mainCamera.fieldOfView;
		}
		set
		{
			mainCamera.fieldOfView = value;
			_frustumDirty = true;
		}
	}

	public int pixelWidth => mainCamera.pixelWidth;

	public int pixelHeight => mainCamera.pixelHeight;

	public RenderTexture targetTexture
	{
		get
		{
			return mainCamera.targetTexture;
		}
		set
		{
			mainCamera.targetTexture = value;
		}
	}

	public void Render()
	{
		mainCamera.Render();
	}

	public void ResetAspect()
	{
		mainCamera.ResetAspect();
		_frustumDirty = true;
	}

	public Ray ScreenPointToRay(Vector3 position)
	{
		return mainCamera.ScreenPointToRay(position);
	}

	public Vector3 ScreenToViewportPoint(Vector3 position)
	{
		return mainCamera.ScreenToViewportPoint(position);
	}

	public Vector3 ScreenToWorldPoint(Vector3 position)
	{
		return mainCamera.ScreenToWorldPoint(position);
	}

	public Ray ViewportPointToRay(Vector3 position)
	{
		return mainCamera.ViewportPointToRay(position);
	}

	public Vector3 ViewportToScreenPoint(Vector3 position)
	{
		return mainCamera.ViewportToScreenPoint(position);
	}

	public Vector3 ViewportToWorldPoint(Vector3 position)
	{
		return mainCamera.ViewportToWorldPoint(position);
	}

	public Vector3 WorldToScreenPoint(Vector3 position)
	{
		return mainCamera.WorldToScreenPoint(position);
	}

	public Vector3 WorldToViewportPoint(Vector3 position)
	{
		return mainCamera.WorldToViewportPoint(position);
	}

	public virtual bool UsesCamera(Camera camera)
	{
		if (camera == null)
		{
			return false;
		}
		if (!(camera == mainCamera))
		{
			return camera == farCamera;
		}
		return true;
	}

	protected virtual void OnValidate()
	{
		if (_farCameraDist < 0.01f)
		{
			_farCameraDist = 0.01f;
		}
	}

	protected virtual void Awake()
	{
		SetupStencilClear();
		if (_useFarCamera)
		{
			if (SystemInfo.usesReversedZBuffer)
			{
				mainCamera.farClipPlane = _farCameraDist;
			}
			else
			{
				CreateFarCamera();
			}
		}
		InitializeGrabPass();
		if (_renderSkybox)
		{
			InitializeSkybox();
		}
		if (postProcessing != null && Application.isPlaying) // CHANGED
		{
			_postProcessingSettings = new PostProcessingGameplaySettings(postProcessing);
		}
		_enabled = mainCamera.enabled;
	}

	// CHANGED
	private void OnDisable()
	{
		// for some reason patch 14 has owcamera get disabled somehow, which breaks camera events LOL
		base.enabled = true;
	}

	protected virtual void OnDestroy()
	{
		if (_postProcessingSettings != null)
		{
			_postProcessingSettings.Destroy();
		}
		if (_farCamera != null)
		{
			Object.Destroy(_farCamera.gameObject);
		}
	}

	public virtual void SetEnabled(bool isEnabled)
	{
		_enabled = isEnabled;
		mainCamera.enabled = _enabled;
	}

	public Plane[] GetFrustumPlanes()
	{
		if (base.transform.hasChanged || _frustumDirty)
		{
			GeometryUtility.CalculateFrustumPlanes(_mainCamera, _frustumPlanes);
			base.transform.hasChanged = false;
			_frustumDirty = false;
		}
		return _frustumPlanes;
	}

	protected virtual void OnPreCull()
	{
		onThisPreCull.Invoke(this);
		onAnyPreCull.Invoke(this);
	}

	protected virtual void OnPreRender()
	{
		onThisPreRender.Invoke(this);
		onAnyPreRender.Invoke(this);
		RebuildGrabPass();
		if (_renderSkybox)
		{
			RebuildSkybox();
		}
		if (_useFarCamera && !SystemInfo.usesReversedZBuffer)
		{
			UpdateFarCamera();
			RenderFarCamera();
		}
		if (_useViewmodels)
		{
			Matrix4x4 value = GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(_viewmodelFOV, _mainCamera.aspect, _mainCamera.nearClipPlane, _mainCamera.farClipPlane), renderIntoTexture: true) * _mainCamera.worldToCameraMatrix;
			Shader.EnableKeyword("_VIEWMODEL_OVERRIDE");
			Shader.SetGlobalMatrix(_propID_ViewmodelMatrixVP, value);
		}
		if (_postProcessingSettings != null)
		{
			_postProcessingSettings.ApplySettings();
		}
	}

	protected virtual void OnPostRender()
	{
		onThisPostRender.Invoke(this);
		onAnyPostRender.Invoke(this);
		_grabPassCommandBuffer.Clear();
		if (_skyboxCommandBuffer != null)
		{
			_skyboxCommandBuffer.Clear();
		}
		Shader.DisableKeyword("_VIEWMODEL_OVERRIDE");
	}

	protected void CreateFarCamera()
	{
		GameObject gameObject = new GameObject(base.name + "_Far");
		gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.NotEditable | HideFlags.DontSaveInBuild;
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		gameObject.tag = "FarCamera";
		_farCamera = gameObject.AddComponent<Camera>();
		_farCamera.enabled = false;
		_mainCameraOriginalClearFlags = _mainCamera.clearFlags;
		_mainCamera.clearFlags = CameraClearFlags.Depth;
	}

	protected void UpdateFarCamera()
	{
		_farCamera.CopyFrom(_mainCamera);
		_farCamera.farClipPlane = _farCameraDist;
		_farCamera.nearClipPlane = _mainCamera.farClipPlane;
		_farCamera.clearFlags = _mainCameraOriginalClearFlags;
		_farCamera.depth = _mainCamera.depth - 1f;
	}

	protected void RenderFarCamera()
	{
		QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
		_farCamera.Render();
		QualitySettings.shadows = UnityEngine.ShadowQuality.All;
	}

	protected void InitializeSkybox()
	{
		_skyboxCommandBuffer = new CommandBuffer();
		_skyboxCommandBuffer.name = "Skybox";
		Camera obj = ((_useFarCamera && !SystemInfo.usesReversedZBuffer) ? _farCamera : _mainCamera);
		CameraEvent evt = ((obj.actualRenderingPath == RenderingPath.DeferredShading) ? CameraEvent.AfterLighting : CameraEvent.BeforeForwardOpaque);
		obj.AddCommandBuffer(evt, _skyboxCommandBuffer);
	}

	protected void RebuildSkybox()
	{
		Matrix4x4 worldToCameraMatrix = _mainCamera.worldToCameraMatrix;
		worldToCameraMatrix.m03 = 0f;
		worldToCameraMatrix.m13 = 0f;
		worldToCameraMatrix.m23 = 0f;
		_skyboxCommandBuffer.SetViewProjectionMatrices(worldToCameraMatrix, _mainCamera.projectionMatrix);
		_skyboxCommandBuffer.SetGlobalDepthBias(1E+09f, 0f);
		List<SkyboxRenderer> activeSkyboxRenderers = SkyboxRenderer.activeSkyboxRenderers;
		for (int i = 0; i < activeSkyboxRenderers.Count; i++)
		{
			if (activeSkyboxRenderers[i].shouldRender)
			{
				_skyboxCommandBuffer.DrawRenderer(activeSkyboxRenderers[i].renderer, activeSkyboxRenderers[i].material);
			}
		}
	}

	protected void InitializeGrabPass()
	{
		_grabPassCommandBuffer = new CommandBuffer();
		_grabPassCommandBuffer.name = "OpaqueGrabPass";
		mainCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, _grabPassCommandBuffer);
	}

	protected void RebuildGrabPass()
	{
		if (_grabPassCommandBuffer == null)
		{
			InitializeGrabPass();
		}
		_grabPassCommandBuffer.GetTemporaryRT(_propID_OpaqueGrabTex, -1, -1, 0, FilterMode.Bilinear, _mainCamera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1, enableRandomWrite: false, RenderTextureMemoryless.None, _mainCamera.allowDynamicResolution);
		_grabPassCommandBuffer.CopyTexture(BuiltinRenderTextureType.CurrentActive, _propID_OpaqueGrabTex);
	}

	protected void SetupStencilClear()
	{
		bool flag = false;
		if (SecretSettings.TryGetBool("NVidiaDriver522.25Fix", out var value))
		{
			flag = value;
		}
		mainCamera.clearStencilAfterLightingPass = !flag;
		if (flag)
		{
			if (s_stencilClearMat == null)
			{
				s_stencilClearMat = new Material(Shader.Find("Hidden/InternalClear"));
				s_stencilClearMat.name = "Clear";
			}
			_stencilClearCommandBuffer = new CommandBuffer();
			_stencilClearCommandBuffer.name = "ClearStencil";
			_stencilClearCommandBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, s_stencilClearMat, 4);
			_mainCamera.AddCommandBuffer(CameraEvent.AfterLighting, _stencilClearCommandBuffer);
		}
	}
}
