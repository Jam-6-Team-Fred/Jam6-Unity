using UnityEngine;

public class PlayerAttachPoint : MonoBehaviour
{
	[SerializeField]
	private bool _lockPlayerTurning = true;

	[SerializeField]
	private bool _matchRotation = true;

	[SerializeField]
	private bool _centerCamera = true;

	private bool _savedMatchRotation;

	private float _translationRate = 2f;

	private float _rotationRate = 100f;

	private Transform _playerTransform;

	private OWRigidbody _playerOWRigidbody;

	private PlayerCameraController _fpsCamController;

	private PlayerCharacterController _playerController;

	private float _initAttachTime;

	private float _turnDuration;

	private Vector3 _attachOffset = Vector3.zero;

	private Quaternion _initLocalRotation;

	private void Start()
	{
		base.enabled = false;
		_playerTransform = Locator.GetPlayerTransform();
		_playerOWRigidbody = _playerTransform.GetRequiredComponent<OWRigidbody>();
		_playerController = _playerTransform.GetRequiredComponent<PlayerCharacterController>();
		_fpsCamController = _playerTransform.GetRequiredComponentInChildren<PlayerCameraController>();
	}

	private void OnDestroy()
	{
		GlobalMessenger<Signalscope>.RemoveListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
	}

	public void AttachPlayer()
	{
		InitAttachment();
		GlobalMessenger<OWRigidbody>.FireEvent("AttachPlayerToPoint", this.GetAttachedOWRigidbody());
		GlobalMessenger<Signalscope>.AddListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.AddListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
	}

	public void DetachPlayer()
	{
		if (!base.enabled)
		{
			Debug.LogWarning("Attempting to detach player but player is not attached.", this);
			return;
		}
		base.enabled = false;
		_playerController.SetColliderActivation(active: true);
		_playerTransform.parent = null;
		_playerOWRigidbody.MakeNonKinematic();
		_playerOWRigidbody.SetVelocity(this.GetAttachedOWRigidbody().GetPointVelocity(base.transform.position));
		_playerController.UnlockMovement();
		GlobalMessenger.FireEvent("DetachPlayerFromPoint");
		GlobalMessenger<Signalscope>.RemoveListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
	}

	public void SetAttachOffset(Vector3 attachOffset)
	{
		_attachOffset = attachOffset;
	}

	private void InitAttachment()
	{
		base.enabled = true;
		_initAttachTime = Time.unscaledTime;
		_playerController.SetColliderActivation(active: false);
		_playerOWRigidbody.MakeKinematic();
		_playerTransform.parent = base.transform;
		_initLocalRotation = _playerTransform.localRotation;
		_turnDuration = Quaternion.Angle(_playerTransform.rotation, base.transform.rotation) / _rotationRate;
		if (_centerCamera)
		{
			_fpsCamController.CenterCamera(_rotationRate);
		}
		_playerController.LockMovement(_lockPlayerTurning);
	}

	private void Update()
	{
		if (OWTime.IsPaused(OWTime.PauseType.Reading))
		{
			UpdatePlayerAttach();
		}
	}

	private void FixedUpdate()
	{
		if (!OWTime.IsPaused())
		{
			UpdatePlayerAttach();
		}
	}

	private void UpdatePlayerAttach()
	{
		float t = Time.unscaledDeltaTime * _translationRate;
		_playerTransform.localPosition = Vector3.Lerp(_playerTransform.localPosition, _attachOffset, t);
		if (_matchRotation)
		{
			float t2 = 1f;
			if (_turnDuration > 0f)
			{
				t2 = Mathf.InverseLerp(_initAttachTime, _initAttachTime + _turnDuration, Time.unscaledTime);
				t2 = Mathf.SmoothStep(0f, 1f, t2);
			}
			_playerTransform.localRotation = Quaternion.Slerp(_initLocalRotation, Quaternion.identity, t2);
		}
	}

	private void OnEnterSignalscopeZoom(Signalscope telescope)
	{
		_savedMatchRotation = _matchRotation;
		_matchRotation = false;
		_playerController.UnlockMovement();
	}

	private void OnExitSignalscopeZoom()
	{
		_matchRotation = _savedMatchRotation;
		InitAttachment();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		OWGizmos.DrawWireCapsule(base.transform.position, base.transform.rotation, 1f, 0.5f);
	}
}
