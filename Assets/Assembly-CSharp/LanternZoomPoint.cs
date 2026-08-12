using System;
using UnityEngine;

[RequireComponent(typeof(LightSensor))]
public class LanternZoomPoint : SectoredMonoBehaviour
{
	private enum State
	{
		Idle = 0,
		LookAt = 1,
		ZoomIn = 2,
		RetroZoom = 3
	}

	[Space]
	[SerializeField]
	private float _arrivalDistance = 5f;

	[SerializeField]
	private float _minActivationDistance = 10f;

	[SerializeField]
	private AnimationCurve _zoomInCurve;

	[SerializeField]
	private AnimationCurve _retroZoomCurve;

	[Space]
	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private PlayerAttachPoint _attachPoint;

	[SerializeField]
	private Animator _totemAnimator;

	public const float ZOOM_IN_DURATION = 0.5f;

	public const float RETRO_ZOOM_DURATION = 1.2f;

	private State _state;

	private float _stateChangeTime;

	private float _imageHalfWidth;

	private float _startFOV;

	private Vector3 _startLocalPos;

	private Vector3 _endLocalPos;

	private LightSensor _lightSensor;

	private DreamLanternItem _playerLantern;

	protected override void Awake()
	{
		base.Awake();
		_lightSensor = GetComponent<LightSensor>();
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
	}

	private void Start()
	{
		_lightController.SetIntensity(0f);
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
	}

	public void CancelZoom()
	{
		FinishRetroZoom();
	}

	protected override void OnSectorOccupantsUpdated()
	{
		_totemAnimator.enabled = _sector.ContainsOccupant(DynamicOccupant.Player);
	}

	private void FixedUpdate()
	{
		_ = base.enabled;
	}

	private void Update()
	{
		if (base.enabled)
		{
			if (_state != State.RetroZoom)
			{
				_playerLantern.GetLanternController().MoveTowardFocus(1f, 2f);
			}
			if (_state == State.LookAt && Time.time > _stateChangeTime + 0.4f)
			{
				ChangeState(State.ZoomIn);
				StartZoomIn();
			}
			else if (_state == State.ZoomIn)
			{
				UpdateZoomIn();
			}
			if (_state == State.RetroZoom)
			{
				UpdateRetroZoom();
			}
		}
	}

	private void ChangeState(State state)
	{
		_state = state;
		_stateChangeTime = Time.time;
	}

	private void StartZoomIn()
	{
		float origFieldOfView = Locator.GetPlayerCameraController().GetOrigFieldOfView();
		_imageHalfWidth = _arrivalDistance * Mathf.Tan(origFieldOfView * ((float)Math.PI / 180f) * 0.4f);
		_lightController.FadeTo(1f, 0.5f);
		Vector3 vector = base.transform.position - Locator.GetPlayerCamera().transform.position;
		_startFOV = Mathf.Atan2(_imageHalfWidth, vector.magnitude) * 2f * 57.29578f;
		Locator.GetPlayerAudioController().OnGrappleTotemZoom();
		if (_totemAnimator != null)
		{
			_totemAnimator.SetTrigger("Zoom");
		}
	}

	private void UpdateZoomIn()
	{
		float time = Mathf.InverseLerp(_stateChangeTime, _stateChangeTime + 0.5f, Time.time);
		float t = _zoomInCurve.Evaluate(time);
		float targetFieldOfView = Mathf.Lerp(Locator.GetPlayerCameraController().GetOrigFieldOfView(), _startFOV, t);
		Locator.GetPlayerCameraController().SetTargetFieldOfView(targetFieldOfView);
		if (Time.time > _stateChangeTime + 0.5f)
		{
			ChangeState(State.RetroZoom);
			StartRetroZoom();
		}
	}

	private void StartRetroZoom()
	{
		Locator.GetPlayerAudioController().OnGrappleTotemRetroZoom();
		Locator.GetPlayerController().SetColliderActivation(active: false);
		_startLocalPos = _attachPoint.transform.localPosition;
		Vector3 vector = Locator.GetPlayerCamera().transform.position - Locator.GetPlayerTransform().position;
		float num = _imageHalfWidth / Mathf.Tan(Locator.GetPlayerCamera().fieldOfView * ((float)Math.PI / 180f) * 0.5f);
		Vector3 position = Locator.GetPlayerCamera().transform.position + Locator.GetPlayerCamera().transform.forward * num - vector;
		_endLocalPos = base.transform.InverseTransformPoint(position);
	}

	private void UpdateRetroZoom()
	{
		float num = Mathf.InverseLerp(_stateChangeTime, _stateChangeTime + 1.2f, Time.time);
		float focus = Mathf.Pow(Mathf.SmoothStep(0f, 1f, 1f - num), 0.2f);
		_playerLantern.GetLanternController().SetFocus(focus);
		float t = _retroZoomCurve.Evaluate(num);
		float targetFieldOfView = Mathf.Lerp(_startFOV, Locator.GetPlayerCameraController().GetOrigFieldOfView(), t);
		Locator.GetPlayerCameraController().SetTargetFieldOfView(targetFieldOfView);
		float num2 = _imageHalfWidth / Mathf.Tan(Locator.GetPlayerCamera().fieldOfView * ((float)Math.PI / 180f) * 0.5f);
		Vector3 vector = _startLocalPos - _endLocalPos;
		_attachPoint.transform.localPosition = _endLocalPos + vector.normalized * num2;
		if (num >= 1f)
		{
			FinishRetroZoom();
		}
	}

	private void FinishRetroZoom()
	{
		ChangeState(State.Idle);
		base.enabled = false;
		_attachPoint.DetachPlayer();
		GlobalMessenger.FireEvent("PlayerRepositioned");
		_playerLantern.ForceUnfocus();
		_playerLantern.enabled = true;
		_playerLantern = null;
		OWInput.ChangeInputMode(InputMode.Character);
		_lightController.FadeTo(0f, 1f);
		Locator.GetPlayerController().SetColliderActivation(active: true);
		Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().BreakLock();
		Locator.GetDreamWorldController().SetActiveZoomPoint(null);
	}

	private void OnDetectLight()
	{
		if (_state == State.Idle && !PlayerState.IsAttached() && Time.time > _stateChangeTime + 1f && Vector3.Distance(base.transform.position, Locator.GetPlayerCamera().transform.position) > _minActivationDistance && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern)
		{
			_playerLantern = (DreamLanternItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
			Locator.GetDreamWorldController().SetActiveZoomPoint(this);
			_attachPoint.transform.position = Locator.GetPlayerTransform().position;
			_attachPoint.transform.rotation = Locator.GetPlayerTransform().rotation;
			_attachPoint.AttachPlayer();
			Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(base.transform, 5f);
			OWInput.ChangeInputMode(InputMode.None);
			_playerLantern.enabled = false;
			ChangeState(State.LookAt);
			base.enabled = true;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, _arrivalDistance);
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(base.transform.position, _minActivationDistance);
	}
}
