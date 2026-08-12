using UnityEngine;

[RequireComponent(typeof(OWCamera))]
[RequireComponent(typeof(AudioListener))]
public class NomaiRemoteCamera : MonoBehaviour
{
	private OWCamera _camera;

	private AudioListener _audioListener;

	private NomaiViewerImageEffect _viewerImageEffect;

	private NomaiRemoteCameraPlatform _owningPlatform;

	private NomaiRemoteCameraPlatform _controllingPlatform;

	private OWCamera _controllingCamera;

	private void Awake()
	{
		_camera = GetComponent<OWCamera>();
		_audioListener = GetComponent<AudioListener>();
		_viewerImageEffect = _camera.GetComponent<NomaiViewerImageEffect>();
		_owningPlatform = GetComponentInParent<NomaiRemoteCameraPlatform>();
		base.enabled = false;
	}

	private void OnEnable()
	{
		_camera.enabled = true;
		_audioListener.enabled = true;
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _camera);
	}

	private void OnDisable()
	{
		_camera.enabled = false;
		_audioListener.enabled = false;
	}

	private void LateUpdate()
	{
		if ((bool)_owningPlatform && (bool)_controllingPlatform)
		{
			base.transform.position = NomaiRemoteCameraPlatform.TransformPoint(_controllingCamera.transform.position, _controllingPlatform, _owningPlatform);
			base.transform.rotation = NomaiRemoteCameraPlatform.TransformRotation(_controllingCamera.transform.rotation, _controllingPlatform, _owningPlatform);
			_camera.fieldOfView = _controllingCamera.fieldOfView;
		}
		else
		{
			base.enabled = false;
		}
	}

	public void Activate(NomaiRemoteCameraPlatform controllingPlatform, OWCamera viewer)
	{
		_controllingPlatform = controllingPlatform;
		_controllingCamera = viewer;
		base.enabled = true;
	}

	public void Deactivate()
	{
		_controllingPlatform = null;
		base.enabled = false;
	}

	public bool IsActive()
	{
		return base.enabled;
	}

	public void SetImageEffectFade(float fade)
	{
		if (_viewerImageEffect != null)
		{
			_viewerImageEffect.SetFade(fade);
		}
	}
}
