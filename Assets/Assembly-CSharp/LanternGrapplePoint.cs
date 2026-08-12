using UnityEngine;

[RequireComponent(typeof(LightSensor))]
public class LanternGrapplePoint : MonoBehaviour
{
	[SerializeField]
	private OWLightController _lightController;

	private LightSensor _lightSensor;

	private float _startGrappleTime;

	private bool _grappleAfterDelay;

	private bool _hasResetFOV;

	private bool _updateGrapple;

	private bool _hasUnfocusedLantern;

	private PlayerCameraController _cameraController;

	private void Awake()
	{
		_lightSensor = GetComponent<LightSensor>();
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void Start()
	{
		_cameraController = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
		_lightController.SetIntensity(0f);
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void FixedUpdate()
	{
		if (!_updateGrapple)
		{
			return;
		}
		Vector3 vector = base.transform.position - base.transform.up * 1f - Locator.GetPlayerTransform().position;
		Vector3 target = vector.normalized * 20f;
		Vector3 velocity = Locator.GetPlayerBody().GetVelocity();
		float num = 50f;
		if (vector.magnitude < 8f)
		{
			if (!_hasUnfocusedLantern && velocity.magnitude > 19f)
			{
				_hasUnfocusedLantern = true;
				if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern)
				{
					((DreamLanternItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem()).ForceUnfocus();
				}
			}
			target = Vector3.zero;
			num = 50f;
			if (!_hasResetFOV)
			{
				_cameraController.SnapToInitFieldOfView(0.5f, smoothStep: true);
				_hasResetFOV = true;
			}
			if (velocity.magnitude < 1f)
			{
				_hasResetFOV = false;
				_updateGrapple = false;
				_hasUnfocusedLantern = false;
				Locator.GetPlayerController().SetColliderActivation(active: true);
				Locator.GetPlayerController().UnlockMovement();
				Locator.GetPlayerDetector().GetComponent<ForceApplier>().SetApplyForces(applyForces: true);
				Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().BreakLock();
				OWInput.ChangeInputMode(InputMode.Character);
				_lightController.FadeTo(0f, 1f);
			}
		}
		Vector3 vector2 = Vector3.MoveTowards(velocity, target, num * Time.deltaTime);
		Locator.GetPlayerBody().AddVelocityChange(vector2 - velocity);
	}

	private void Update()
	{
		if (_grappleAfterDelay && Time.time > _startGrappleTime)
		{
			StartGrapple();
		}
	}

	private void StartGrapple()
	{
		Locator.GetPlayerController().SetColliderActivation(active: false);
		Locator.GetPlayerController().LockMovement(lockTurning: false);
		Locator.GetPlayerDetector().GetComponent<ForceApplier>().SetApplyForces(applyForces: false);
		_cameraController.SetTargetFieldOfView(120f, 2f, overrideSnapZoom: true);
		_updateGrapple = true;
		_grappleAfterDelay = false;
		_lightController.FadeTo(0f, 0.2f);
		if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern)
		{
			((DreamLanternItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem()).ForceUnfocus();
		}
	}

	private void OnDetectLight()
	{
		if (!_updateGrapple && !_grappleAfterDelay && !PlayerState.IsAttached())
		{
			Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(base.transform, 5f);
			OWInput.ChangeInputMode(InputMode.None);
			_lightController.FadeTo(1f, 0.2f);
			_grappleAfterDelay = true;
			_startGrappleTime = Time.time + 0.4f;
		}
	}

	private void OnDetectDarkness()
	{
	}
}
