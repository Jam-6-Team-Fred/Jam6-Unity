using UnityEngine;

public class FaceActiveCamera : MonoBehaviour
{
	private bool _isMapCamActive;

	[SerializeField]
	private Vector3 _localFacingVector = Vector3.forward;

	[SerializeField]
	private Vector3 _localRotationAxis = Vector3.zero;

	[SerializeField]
	private bool _useLookAt;

	private OWCamera _activeCam;

	private void Awake()
	{
		GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
	}

	private void Start()
	{
		_activeCam = Locator.GetActiveCamera();
		UpdateRotation();
	}

	private void OnDestroy()
	{
		GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
	}

	private void Update()
	{
		UpdateRotation();
	}

	private void UpdateRotation()
	{
		if (_isMapCamActive)
		{
			Vector3 vector = new Vector3(0f, 150000f, 0f);
			if (_useLookAt)
			{
				base.transform.LookAt(vector);
			}
			else
			{
				base.transform.rotation = Quaternion.FromToRotation(base.transform.TransformDirection(_localFacingVector), vector) * base.transform.rotation;
			}
		}
		else if (_useLookAt)
		{
			base.transform.LookAt(_activeCam.transform.position);
		}
		else
		{
			Vector3 vector2 = _activeCam.transform.position - base.transform.position;
			Vector3 toDirection = vector2 - Vector3.Project(vector2, base.transform.TransformDirection(_localRotationAxis));
			base.transform.rotation = Quaternion.FromToRotation(base.transform.TransformDirection(_localFacingVector), toDirection) * base.transform.rotation;
		}
	}

	private void OnSwitchActiveCamera(OWCamera activeCamera)
	{
		_activeCam = activeCamera;
		UpdateRotation();
	}

	private void OnEnterMapView()
	{
		if (_localRotationAxis.sqrMagnitude == 0f)
		{
			_isMapCamActive = true;
		}
	}

	private void OnExitMapView()
	{
		_isMapCamActive = false;
	}
}
