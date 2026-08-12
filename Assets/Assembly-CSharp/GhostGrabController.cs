using UnityEngine;

public class GhostGrabController : MonoBehaviour
{
	[SerializeField]
	private Transform _holdPoint;

	[SerializeField]
	private Transform _liftPoint;

	private GhostEffects _effects;

	private PlayerAttachPoint _attachPoint;

	private Transform _origParent;

	private Vector3 _startLocalPos;

	private Quaternion _startLocalRot;

	private float _grabStartTime;

	private float _grabMoveDuration;

	private float _extinguishTime;

	private bool _playerAttached;

	private bool _snappingNeck;

	private bool _holdingInPlace;

	private bool _grabMoveComplete;

	private bool _extinguishStarted;

	public void Initialize(GhostEffects ghostEffects)
	{
		_effects = ghostEffects;
		_effects.OnLiftPlayer += new OWEvent.OWCallback(OnStartLiftPlayer);
		_effects.OnExtinguishPlayerLantern += new OWEvent.OWCallback(OnExtinguishPlayerLantern);
		_effects.OnSnapPlayerNeck += new OWEvent.OWCallback(OnSnapPlayerNeck);
		_attachPoint = GetComponent<PlayerAttachPoint>();
		_origParent = _attachPoint.transform.parent;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_effects != null)
		{
			_effects.OnLiftPlayer -= new OWEvent.OWCallback(OnStartLiftPlayer);
			_effects.OnExtinguishPlayerLantern -= new OWEvent.OWCallback(OnExtinguishPlayerLantern);
			_effects.OnSnapPlayerNeck -= new OWEvent.OWCallback(OnSnapPlayerNeck);
		}
	}

	public void GrabPlayer(float speed)
	{
		if (!PlayerState.IsAttached())
		{
			base.enabled = true;
			_snappingNeck = !Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController()
				.IsHeldByPlayer();
			_holdingInPlace = true;
			_grabMoveComplete = false;
			_extinguishStarted = false;
			_attachPoint.transform.parent = _origParent;
			_attachPoint.transform.position = Locator.GetPlayerTransform().position;
			_attachPoint.transform.rotation = Locator.GetPlayerTransform().rotation;
			_startLocalPos = _attachPoint.transform.localPosition;
			_startLocalRot = _attachPoint.transform.localRotation;
			_playerAttached = true;
			_attachPoint.AttachPlayer();
			GlobalMessenger.FireEvent("PlayerGrabbedByGhost");
			OWInput.ChangeInputMode(InputMode.None);
			ReticleController.Hide();
			Locator.GetDreamWorldController().SetActiveGhostGrabController(this);
			_grabStartTime = Time.time;
			_grabMoveDuration = Mathf.Min(Vector3.Distance(_startLocalPos, _holdPoint.localPosition) / speed, 2f);
			if (_snappingNeck)
			{
				_effects.PlaySnapNeckAnimation();
			}
			else
			{
				_effects.PlayBlowOutLanternAnimation(PlayerState.HasPlayerHadLanternBlownOut());
				Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController()
					.grabbedByGhost = true;
			}
			_effects.PlayGrabAudio(AudioType.Ghost_Grab_Contact);
			RumbleManager.PlayGhostGrab();
			Achievement_Ghost.GotCaughtByGhost();
		}
	}

	public void ReleasePlayer()
	{
		if (_playerAttached)
		{
			_playerAttached = false;
			_attachPoint.DetachPlayer();
			GlobalMessenger.FireEvent("PlayerReleasedByGhost");
			_attachPoint.transform.parent = _origParent;
			Locator.GetDreamWorldController().SetActiveGhostGrabController(null);
			OWInput.ChangeInputMode(InputMode.Character);
			base.enabled = false;
		}
	}

	private void OnStartLiftPlayer()
	{
		_holdingInPlace = false;
		_attachPoint.transform.parent = _liftPoint;
		_startLocalPos = _attachPoint.transform.localPosition;
		_startLocalRot = _attachPoint.transform.localRotation;
		_grabStartTime = Time.time;
		_grabMoveDuration = Mathf.Min(Vector3.Distance(_startLocalPos, Vector3.zero) / 0.5f, 1f);
	}

	private void OnExtinguishPlayerLantern()
	{
		_extinguishStarted = true;
		_extinguishTime = Time.time + 0.4f;
	}

	private void OnSnapPlayerNeck()
	{
		if (!Locator.GetDeathManager().IsPlayerDying() && !Locator.GetDeathManager().IsPlayerDead())
		{
			Locator.GetDreamWorldController().ExitDreamWorld(DreamWakeType.NeckSnapped);
		}
		base.enabled = false;
	}

	private void CompleteExtinguish()
	{
		if (!Locator.GetDeathManager().IsPlayerDying() && !Locator.GetDeathManager().IsPlayerDead())
		{
			Locator.GetDreamWorldController().ExitDreamWorld(DreamWakeType.LanternBlownOut);
		}
		base.enabled = false;
	}

	private void FixedUpdate()
	{
		if (_extinguishStarted && Time.time >= _extinguishTime)
		{
			CompleteExtinguish();
			return;
		}
		float t = Mathf.InverseLerp(_grabStartTime, _grabStartTime + _grabMoveDuration, Time.time);
		t = Mathf.SmoothStep(0f, 1f, t);
		if (_holdingInPlace)
		{
			_attachPoint.transform.localPosition = Vector3.Lerp(_startLocalPos, _holdPoint.localPosition, t);
			_attachPoint.transform.localRotation = Quaternion.Slerp(_startLocalRot, _holdPoint.localRotation, t);
		}
		else if (!_grabMoveComplete)
		{
			_attachPoint.transform.localPosition = Vector3.Lerp(_startLocalPos, Vector3.zero, t);
			_attachPoint.transform.localRotation = Quaternion.Slerp(_startLocalRot, Quaternion.identity, t);
			if (t >= 1f)
			{
				_grabMoveComplete = true;
			}
		}
	}
}
