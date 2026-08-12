using UnityEngine;

public class PlayerLockOnTargeting : MonoBehaviour
{
	private PlayerCameraController _playerCamController;

	private Transform _playerTransform;

	private Transform _targetTransform;

	private Vector3 _localOffset;

	private float _followRate;

	private bool _useZoom;

	private float _zoomSpeed = 1f;

	private void Awake()
	{
		base.enabled = false;
		if (!base.gameObject.CompareTag("Player"))
		{
			Debug.LogError("PlayerLockOnTargeting should only be attached to the Player's transform");
			Debug.Break();
		}
		_playerTransform = base.transform;
		_playerCamController = _playerTransform.GetRequiredComponentInChildren<PlayerCameraController>();
	}

	public void LockOn(Transform targetTransform, float followEasing = 1f, bool useZoom = false, float zoomSpeed = 1f)
	{
		LockOn(targetTransform, Vector3.zero, followEasing, useZoom, zoomSpeed);
	}

	public void LockOn(Transform targetTransform, Vector3 localOffset, float followEasing = 1f, bool useZoom = false, float zoomSpeed = 1f)
	{
		base.enabled = true;
		_targetTransform = targetTransform;
		_localOffset = localOffset;
		_playerCamController.LockOn(_targetTransform, localOffset, followEasing);
		_followRate = followEasing;
		_useZoom = useZoom;
		_zoomSpeed = zoomSpeed;
		GlobalMessenger.FireEvent("PlayerCameraLockOn");
	}

	public void BreakLock()
	{
		BreakLock(_zoomSpeed);
	}

	public void BreakLock(float zoomSpeed)
	{
		base.enabled = false;
		_playerCamController.BreakLock();
		_playerCamController.ResetTargetFieldOfView(zoomSpeed);
		GlobalMessenger.FireEvent("PlayerCameraBreakLock");
	}

	private void Update()
	{
		if (OWTime.IsPaused(OWTime.PauseType.Reading))
		{
			UpdateLockOnTargeting(Time.unscaledDeltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (!OWTime.IsPaused())
		{
			UpdateLockOnTargeting(Time.fixedDeltaTime);
		}
	}

	private void UpdateLockOnTargeting(float deltaTime)
	{
		Vector3 vector = _targetTransform.TransformPoint(_localOffset);
		Vector3 vector2 = vector - _playerTransform.position;
		Vector3 vector3 = vector2 - Vector3.Project(vector2, _playerTransform.up);
		Quaternion quaternion = Quaternion.AngleAxis(Vector3.Angle(_playerTransform.forward, vector3) * Mathf.Sign(Vector3.Dot(vector3, _playerTransform.right)) * _followRate * deltaTime, _playerTransform.up);
		_playerTransform.rotation = quaternion * _playerTransform.rotation;
		float num = Vector3.Distance(_playerCamController.transform.position, vector);
		if (_useZoom)
		{
			if (num > 10f)
			{
				float targetFOV = Mathf.Max(1f / num * 500f, 20f);
				_playerCamController.SetTargetFieldOfView(targetFOV, _zoomSpeed, overrideSnapZoom: true);
			}
			else
			{
				_playerCamController.ResetTargetFieldOfView(2f);
			}
		}
	}
}
