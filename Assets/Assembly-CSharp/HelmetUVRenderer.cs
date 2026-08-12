using UnityEngine;
using UnityEngine.Rendering;

public class HelmetUVRenderer : MonoBehaviour
{
	[SerializeField]
	private OWCamera _targetCamera;

	[SerializeField]
	private Renderer[] _renderers;

	[SerializeField]
	private float _removeHelmetDelay = 1f;

	private RenderTexture _uvRenderTex;

	private CommandBuffer _commandBuffer;

	private bool _helmetOn;

	private float _helmetRemoveTime;

	private void Awake()
	{
		Vector2 vector = new Vector2(480f, 270f);
		float num = (float)Screen.width / (float)Screen.height;
		if (!OWMath.ApproxEquals(num, 1.777f, 0.01f))
		{
			vector.x *= num / 1.7777778f;
		}
		_uvRenderTex = new RenderTexture(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		_uvRenderTex.name = "HelmetVisorUV";
		_uvRenderTex.Create();
		_commandBuffer = new CommandBuffer();
		_commandBuffer.name = "Helmet Visor UVs";
		_commandBuffer.EnableShaderKeyword("OW_PASS_HELMETUV");
		if (_targetCamera.useViewmodels)
		{
			_commandBuffer.EnableShaderKeyword("_VIEWMODEL_OVERRIDE");
		}
		_commandBuffer.SetRenderTarget(_uvRenderTex);
		_commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
		for (int i = 0; i < _renderers.Length; i++)
		{
			_commandBuffer.DrawRenderer(_renderers[i], _renderers[i].sharedMaterial);
		}
		_commandBuffer.DisableShaderKeyword("OW_PASS_HELMETUV");
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _uvRenderTex;
		GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
		RenderTexture.active = active;
		GlobalMessenger.AddListener("PutOnHelmet", OnPutOnHelmet);
		GlobalMessenger.AddListener("RemoveHelmet", OnRemoveHelmet);
		base.enabled = false;
	}

	private void Start()
	{
		_targetCamera.postProcessingSettings.lensDirt.uvTexture = _uvRenderTex;
	}

	private void OnDestroy()
	{
		_uvRenderTex.Release();
		Object.Destroy(_uvRenderTex);
		_uvRenderTex = null;
		_commandBuffer.Dispose();
		GlobalMessenger.RemoveListener("PutOnHelmet", OnPutOnHelmet);
		GlobalMessenger.RemoveListener("RemoveHelmet", OnRemoveHelmet);
	}

	private void OnEnable()
	{
		_targetCamera.mainCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, _commandBuffer);
	}

	private void OnDisable()
	{
		_targetCamera.mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _commandBuffer);
	}

	private void Update()
	{
		if (!_helmetOn && Time.time > _helmetRemoveTime + _removeHelmetDelay)
		{
			base.enabled = false;
		}
	}

	private void OnPutOnHelmet()
	{
		_helmetOn = true;
		base.enabled = true;
	}

	private void OnRemoveHelmet()
	{
		_helmetOn = false;
		_helmetRemoveTime = Time.time;
	}
}
