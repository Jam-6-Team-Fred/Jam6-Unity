using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(OWCamera))]
public class LandingCamera : MonoBehaviour
{
	public enum Mode
	{
		Double = 0,
		Swap = 1
	}

	private OWCamera _owCamera;

	[SerializeField]
	private float _aspectRatio = 1.3333334f;

	[SerializeField]
	private RenderTexture _targetTexture;

	[Space]
	[SerializeField]
	private Shader _landingCamShader;

	[SerializeField]
	private Texture2D _landingCameraLUT;

	[SerializeField]
	[Range(0f, 1f)]
	private float _landingCameraNoise = 0.005f;

	private OWCamera _playerCamera;

	private CommandBuffer _dashboardCommandBuffer;

	private Material _landingCamBlitMaterial;

	private bool _isPowered = true;

	private bool _isDamaged;

	private int _origLayerMask;

	public OWCamera owCamera => _owCamera;

	public Mode mode => Mode.Double;

	private void Awake()
	{
		_owCamera = GetComponent<OWCamera>();
		_owCamera.aspect = _aspectRatio;
		_origLayerMask = _owCamera.cullingMask;
		_owCamera.targetTexture = _targetTexture;
		_owCamera.postProcessing.enabled = false;
		ClearTargetTexture();
		_landingCamBlitMaterial = new Material(_landingCamShader);
		_landingCamBlitMaterial.SetTexture("_UserLut", _landingCameraLUT);
		_landingCamBlitMaterial.SetVector("_UserLut_Params", new Vector4(1f / (float)_landingCameraLUT.width, 1f / (float)_landingCameraLUT.height, (float)_landingCameraLUT.height - 1f, 0f));
		_landingCamBlitMaterial.SetVector("_NoiseRes", new Vector4(_targetTexture.width, _targetTexture.height, 0f, 0f));
		_landingCamBlitMaterial.SetFloat("_NoiseStrength", _landingCameraNoise);
	}

	private void OnEnable()
	{
		_owCamera.enabled = _isPowered;
	}

	private void OnDisable()
	{
		_owCamera.enabled = false;
		ClearTargetTexture();
	}

	private void OnDestroy()
	{
		Object.Destroy(_landingCamBlitMaterial);
		_landingCamBlitMaterial = null;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, _landingCamBlitMaterial);
	}

	private void ClearTargetTexture()
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _targetTexture;
		GL.Clear(clearDepth: true, clearColor: true, Color.black);
		RenderTexture.active = active;
	}

	private void UpdateState()
	{
		if (_isPowered)
		{
			_owCamera.enabled = base.enabled;
			if (_isDamaged)
			{
				_owCamera.cullingMask = 0;
				_owCamera.planetaryFog.enabled = false;
				_landingCamBlitMaterial.SetFloat("_NoiseStrength", 1f);
			}
			else
			{
				_owCamera.cullingMask = _origLayerMask;
				_owCamera.planetaryFog.enabled = true;
				_landingCamBlitMaterial.SetFloat("_NoiseStrength", _landingCameraNoise);
			}
		}
		else
		{
			_owCamera.enabled = false;
			ClearTargetTexture();
		}
	}

	public void SetDamaged(bool isDamaged)
	{
		if (_isDamaged != isDamaged)
		{
			_isDamaged = isDamaged;
			UpdateState();
		}
	}

	public void SetPowered(bool isPowered)
	{
		if (_isPowered != isPowered)
		{
			_isPowered = isPowered;
			UpdateState();
		}
	}
}
