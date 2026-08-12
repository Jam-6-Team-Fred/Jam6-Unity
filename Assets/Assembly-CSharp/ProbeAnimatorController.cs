using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ProbeAnimatorController : MonoBehaviour
{
	private Animator _animator;

	private SurveyorProbe _probe;

	[SerializeField]
	private Transform _centerBone;

	private Quaternion _startCenterBoneRotation = Quaternion.identity;

	private Quaternion _targetCenterBoneRotation = Quaternion.identity;

	private Quaternion _currentCenterBoneRotation = Quaternion.identity;

	private float _rotationT;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_probe = this.GetAttachedOWRigidbody().GetRequiredComponent<SurveyorProbe>();
		_probe.OnLaunchProbe += OnProbeFire;
		_probe.OnAnchorProbe += OnProbeAnchor;
		_probe.OnRetrieveProbe += OnProbeReset;
	}

	private void Start()
	{
		_probe.GetRotatingCamera().OnRotateCamera += OnRotateCamera;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_probe.OnLaunchProbe -= OnProbeFire;
		_probe.OnAnchorProbe -= OnProbeAnchor;
		_probe.OnRetrieveProbe -= OnProbeReset;
	}

	public Quaternion GetCenterBoneLocalRotation()
	{
		return _centerBone.localRotation;
	}

	private void OnProbeFire()
	{
		_animator.SetTrigger("Fire");
	}

	private void OnProbeAnchor()
	{
		_animator.SetTrigger("Impact");
	}

	private void OnProbeReset()
	{
		_animator.SetTrigger("Reset");
		_centerBone.localRotation = Quaternion.identity;
		_startCenterBoneRotation = Quaternion.identity;
		_targetCenterBoneRotation = Quaternion.identity;
		_currentCenterBoneRotation = Quaternion.identity;
		_rotationT = 0f;
		base.enabled = false;
	}

	private void OnRotateCamera(Vector2 rotation)
	{
		_startCenterBoneRotation = _currentCenterBoneRotation;
		_targetCenterBoneRotation = Quaternion.AngleAxis(rotation.x, Vector3.up);
		_rotationT = 0f;
		base.enabled = true;
	}

	private void LateUpdate()
	{
		_rotationT += 10f * Time.deltaTime;
		if (_rotationT >= 1f)
		{
			_rotationT = 1f;
			_currentCenterBoneRotation = _targetCenterBoneRotation;
			base.enabled = false;
		}
		else
		{
			float t = Mathf.Sqrt(Mathf.SmoothStep(0f, 1f, _rotationT));
			_currentCenterBoneRotation = Quaternion.Lerp(_startCenterBoneRotation, _targetCenterBoneRotation, t);
		}
		_centerBone.localRotation = _currentCenterBoneRotation;
	}
}
