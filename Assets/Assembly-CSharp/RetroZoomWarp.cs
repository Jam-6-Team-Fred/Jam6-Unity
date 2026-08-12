using System;
using UnityEngine;

public class RetroZoomWarp : MonoBehaviour
{
	[SerializeField]
	private float _arrivalDistance = 5f;

	[SerializeField]
	private PlayerAttachPoint _attachPoint;

	private Collider _collider;

	private Signalscope _scope;

	private PlayerCameraController _playerCamera;

	private bool _warpOnExit;

	private bool _warping;

	private bool _lookingAtSymbol;

	private float _startWarpTime;

	private float _imageHalfWidth;

	private Vector3 _startLocalPos;

	private Vector3 _endLocalPos;

	private void Awake()
	{
		_collider = GetComponent<Collider>();
		GlobalMessenger<Signalscope>.AddListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.AddListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
	}

	private void Start()
	{
		_playerCamera = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger<Signalscope>.RemoveListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
	}

	private void StartWarp()
	{
		base.enabled = true;
		_warping = true;
		_attachPoint.transform.position = Locator.GetPlayerTransform().position;
		_attachPoint.transform.rotation = Locator.GetPlayerTransform().rotation;
		_attachPoint.AttachPlayer();
		_startWarpTime = Time.time;
		_startLocalPos = _attachPoint.transform.localPosition;
		Vector3 vector = Locator.GetPlayerCamera().transform.position - Locator.GetPlayerTransform().position;
		float num = _imageHalfWidth / Mathf.Tan(Locator.GetPlayerCamera().fieldOfView * ((float)Math.PI / 180f) * 0.5f);
		Vector3 position = _playerCamera.transform.position + _playerCamera.transform.forward * num - vector;
		_endLocalPos = base.transform.InverseTransformPoint(position);
	}

	private void UpdateWarp()
	{
		float num = Mathf.InverseLerp(_startWarpTime, _startWarpTime + 0.5f, Time.time);
		if (_scope != null)
		{
			float num2 = _imageHalfWidth / Mathf.Tan(Locator.GetPlayerCamera().fieldOfView * ((float)Math.PI / 180f) * 0.5f);
			Vector3 vector = _startLocalPos - _endLocalPos;
			_attachPoint.transform.localPosition = _endLocalPos + vector.normalized * num2;
		}
		else
		{
			float num3 = _imageHalfWidth / Mathf.Tan(Locator.GetPlayerCamera().fieldOfView * ((float)Math.PI / 180f) * 0.5f);
			Vector3 vector2 = _startLocalPos - _endLocalPos;
			_attachPoint.transform.localPosition = _endLocalPos + vector2.normalized * num3;
		}
		if (num >= 1f)
		{
			_attachPoint.DetachPlayer();
			GlobalMessenger.FireEvent("PlayerRepositioned");
			_warping = false;
		}
	}

	private void FixedUpdate()
	{
		_warpOnExit = false;
		if (_warping)
		{
			UpdateWarp();
		}
		else if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern)
		{
			DreamLanternItem obj = (DreamLanternItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
			PlayerCameraController component = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
			bool lookingAtSymbol = _lookingAtSymbol;
			if (obj.GetLanternController().IsFocused())
			{
				if (CheckPlayerLookingAtSymbol())
				{
					_lookingAtSymbol = true;
					_warpOnExit = true;
					float origFieldOfView = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().GetOrigFieldOfView();
					_imageHalfWidth = _arrivalDistance * Mathf.Tan(origFieldOfView * ((float)Math.PI / 180f) * 0.5f);
					Vector3 vector = base.transform.position - _playerCamera.transform.position;
					float targetFOV = Mathf.Atan2(_imageHalfWidth, vector.magnitude) * 2f * 57.29578f;
					component.SetTargetFieldOfView(targetFOV, 4f);
				}
				else
				{
					_lookingAtSymbol = false;
				}
			}
			if (!_lookingAtSymbol && lookingAtSymbol)
			{
				component.SetTargetFieldOfView(component.GetOrigFieldOfView(), 4f);
			}
		}
		else if (_scope != null && _scope.InZoomMode())
		{
			_scope.ClearTargetWarpFov();
			if (CheckPlayerLookingAtSymbol())
			{
				_warpOnExit = true;
				float origFieldOfView2 = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().GetOrigFieldOfView();
				_imageHalfWidth = _arrivalDistance * Mathf.Tan(origFieldOfView2 * ((float)Math.PI / 180f) * 0.5f);
				Vector3 vector2 = base.transform.position - _playerCamera.transform.position;
				float targetWarpFov = Mathf.Atan2(_imageHalfWidth, vector2.magnitude) * 2f * 57.29578f;
				_scope.SetTargetWarpFov(targetWarpFov);
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	private bool CheckPlayerLookingAtSymbol(float maxAngle = 10f)
	{
		Vector3 to = base.transform.position - _playerCamera.transform.position;
		float magnitude = to.magnitude;
		if (Vector3.Angle(_playerCamera.transform.forward, to) < maxAngle && magnitude > _arrivalDistance && Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out var hitInfo, 800f, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore) && hitInfo.collider.Equals(_collider))
		{
			return true;
		}
		return false;
	}

	private void OnEnterSignalscopeZoom(Signalscope scope)
	{
		base.enabled = true;
		_scope = scope;
	}

	private void OnExitSignalscopeZoom()
	{
		if (_warpOnExit)
		{
			_warpOnExit = false;
			StartWarp();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, _arrivalDistance);
	}
}
