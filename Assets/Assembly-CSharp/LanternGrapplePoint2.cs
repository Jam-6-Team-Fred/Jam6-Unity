using UnityEngine;

[RequireComponent(typeof(LightSensor))]
public class LanternGrapplePoint2 : MonoBehaviour
{
	private enum State
	{
		Idle = 0,
		LookAt = 1,
		Grapple = 2
	}

	[SerializeField]
	private float _arrivalDistance = 5f;

	[SerializeField]
	private float _arrivalHeight;

	[SerializeField]
	private TransformAnimator _animator;

	[SerializeField]
	private AnimationCurve _speedCurve;

	[SerializeField]
	private AnimationCurve _fovCurve;

	[Space]
	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private PlayerAttachPoint _attachPoint;

	private State _state;

	private float _stateChangeTime;

	private float _grappleDuration;

	private Vector3 _startLocalPos;

	private Vector3 _endLocalPos;

	private LightSensor _lightSensor;

	private DreamLanternItem _playerLantern;

	private void Awake()
	{
		_lightSensor = GetComponent<LightSensor>();
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
	}

	private void Start()
	{
		_lightController.SetIntensity(0f);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
	}

	private void FixedUpdate()
	{
		if (base.enabled)
		{
			if (_state == State.LookAt && Time.time > _stateChangeTime + 0.4f)
			{
				ChangeState(State.Grapple);
				StartGrapple();
			}
			else if (_state == State.Grapple)
			{
				UpdateGrapple();
			}
		}
	}

	private void ChangeState(State state)
	{
		_state = state;
		_stateChangeTime = Time.time;
	}

	private void StartGrapple()
	{
		_attachPoint.transform.position = Locator.GetPlayerTransform().position;
		_attachPoint.transform.rotation = Locator.GetPlayerTransform().rotation;
		_attachPoint.AttachPlayer();
		Locator.GetPlayerController().SetColliderActivation(active: false);
		_playerLantern.ForceUnfocus();
		_startLocalPos = _attachPoint.transform.localPosition;
		Vector3 vector = Vector3.zero - _startLocalPos;
		Vector3 vector2 = vector;
		vector2.y = 0f;
		_lightController.FadeTo(0f, 0.2f);
		vector -= vector2.normalized * _arrivalDistance;
		_endLocalPos = _startLocalPos + vector;
		_endLocalPos.y = _arrivalHeight;
		float t = Mathf.InverseLerp(_arrivalDistance, _playerLantern.GetLanternController().GetMaxRange(), Vector3.Distance(_startLocalPos, _endLocalPos));
		_grappleDuration = Mathf.Lerp(0.5f, 3f, t);
	}

	private void UpdateGrapple()
	{
		float num = Mathf.InverseLerp(_stateChangeTime, _stateChangeTime + _grappleDuration, Time.time);
		_attachPoint.transform.localPosition = Vector3.Lerp(_startLocalPos, _endLocalPos, _speedCurve.Evaluate(num));
		float targetFieldOfView = Mathf.Lerp(Locator.GetPlayerCameraController().GetOrigFieldOfView(), Locator.GetPlayerCameraController().GetOrigFieldOfView() + 30f, _fovCurve.Evaluate(num));
		Locator.GetPlayerCameraController().SetTargetFieldOfView(targetFieldOfView);
		if (num >= 1f)
		{
			FinishGrapple();
		}
	}

	private void FinishGrapple()
	{
		ChangeState(State.Idle);
		base.enabled = false;
		_attachPoint.DetachPlayer();
		GlobalMessenger.FireEvent("PlayerRepositioned");
		_playerLantern = null;
		OWInput.ChangeInputMode(InputMode.Character);
		_animator.RotateToLocalEulerAngles(new Vector3(0f, 0f, 0f), 1f);
		Locator.GetPlayerCameraController().SnapToInitFieldOfView(0.5f, smoothStep: true);
		Locator.GetPlayerController().SetColliderActivation(active: true);
		Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().BreakLock();
	}

	private void OnDetectLight()
	{
		if (_state == State.Idle && !PlayerState.IsAttached() && Time.time > _stateChangeTime + 1f && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern)
		{
			_playerLantern = (DreamLanternItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
			_animator.RotateToLocalEulerAngles(new Vector3(0f, 0f, 180f), 1f);
			Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(base.transform, 5f);
			OWInput.ChangeInputMode(InputMode.None);
			_lightController.FadeTo(1f, 0.2f);
			ChangeState(State.LookAt);
			base.enabled = true;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, _arrivalDistance);
	}
}
