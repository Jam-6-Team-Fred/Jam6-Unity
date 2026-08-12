using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class PerCameraRendererState : MonoBehaviour
{
	private Renderer _renderer;

	[SerializeField]
	private OWCamera _owCamera;

	[SerializeField]
	private bool _enabled = true;

	[SerializeField]
	private ShadowCastingMode _shadowCastingMode = ShadowCastingMode.On;

	private bool _prevEnabled;

	private ShadowCastingMode _prevShadowCastingMode;

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
	}

	private void OnEnable()
	{
		_owCamera.onThisPreCull += new OWEvent<OWCamera>.OWCallback(OnOWCameraPreCull);
		_owCamera.onThisPostRender += new OWEvent<OWCamera>.OWCallback(OnOWCameraPostRender);
	}

	private void OnDisable()
	{
		_owCamera.onThisPreCull -= new OWEvent<OWCamera>.OWCallback(OnOWCameraPreCull);
		_owCamera.onThisPostRender -= new OWEvent<OWCamera>.OWCallback(OnOWCameraPostRender);
	}

	private void OnOWCameraPreCull(OWCamera owCamera)
	{
		_prevEnabled = _renderer.enabled;
		_prevShadowCastingMode = _renderer.shadowCastingMode;
		_renderer.enabled = _enabled;
		_renderer.shadowCastingMode = _shadowCastingMode;
	}

	private void OnOWCameraPostRender(OWCamera owCamera)
	{
		_renderer.enabled = _prevEnabled;
		_renderer.shadowCastingMode = _prevShadowCastingMode;
	}
}
