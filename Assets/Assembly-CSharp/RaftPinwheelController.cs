using UnityEngine;

public class RaftPinwheelController : MonoBehaviour
{
	[SerializeField]
	private Transform _rudderPivot;

	[SerializeField]
	private DreamTorch _torch;

	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private ForceDetector _forceDetector;

	[SerializeField]
	private MeshRenderer[] _panelRenderers;

	[SerializeField]
	private Material _glowMaterial;

	private Material _origMaterial;

	private float _rudderDegreesPerSecond;

	private OWRigidbody _raftBody;

	private void Awake()
	{
		_raftBody = GetComponent<OWRigidbody>();
		_origMaterial = _panelRenderers[0].sharedMaterial;
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void FixedUpdate()
	{
		float target = 0f;
		float num = 200f;
		if (_lightSensor.IsIlluminated())
		{
			Vector3 vector = _rudderPivot.position - Locator.GetPlayerCamera().transform.position;
			Vector3 forward = Locator.GetPlayerCamera().transform.forward;
			vector = Vector3.ProjectOnPlane(vector, _rudderPivot.up);
			forward = Vector3.ProjectOnPlane(forward, _rudderPivot.up);
			float f = OWMath.Angle(vector, forward, _rudderPivot.up);
			if (Mathf.Abs(f) > 5f)
			{
				target = (0f - Mathf.Sign(f)) * 120f;
			}
			for (int i = 0; i < _panelRenderers.Length; i++)
			{
				float num2 = (0f - Mathf.Sign(f)) * OWMath.Angle(vector, _panelRenderers[i].transform.forward, _rudderPivot.up);
				bool flag = Mathf.Abs(f) > 5f && num2 > 10f && num2 < 90f;
				_panelRenderers[i].sharedMaterial = (flag ? _glowMaterial : _origMaterial);
			}
		}
		_rudderDegreesPerSecond = Mathf.MoveTowards(_rudderDegreesPerSecond, target, num * Time.deltaTime);
		_rudderPivot.Rotate(Vector3.up * _rudderDegreesPerSecond * Time.deltaTime, Space.Self);
		if (_torch.IsLit())
		{
			_raftBody.AddAcceleration(_rudderPivot.forward * 2f);
		}
		Vector3 toDirection = -_forceDetector.GetForceAcceleration();
		Vector3 vector2 = OWPhysics.FromToAngularVelocity(base.transform.up, toDirection);
		_raftBody.AddAngularAcceleration(vector2 * 0.5f);
	}

	private void OnDetectLight()
	{
	}

	private void OnDetectDarkness()
	{
		for (int i = 0; i < _panelRenderers.Length; i++)
		{
			_panelRenderers[i].sharedMaterial = _origMaterial;
		}
	}
}
