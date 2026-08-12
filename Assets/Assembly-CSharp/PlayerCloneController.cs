using UnityEngine;

public class PlayerCloneController : MonoBehaviour
{
	[SerializeField]
	private Transform _playerWarpTarget;

	[SerializeField]
	private EndlessCylinder _endlessCylinder;

	[SerializeField]
	private GameObject _playerVisuals;

	[SerializeField]
	private AudioSignal _signal;

	[SerializeField]
	private OWAudioSource _desolateAmbience;

	private Vector3 _localMirrorPos;

	private bool _warpFlickerActivated;

	private float _warpTime;

	private bool _warpPlayerNextFrame;

	private NotificationData _signalMessage;

	private void Awake()
	{
		_playerVisuals.SetActive(value: false);
		_signalMessage = new NotificationData(NotificationTarget.All, UITextLibrary.GetString(UITextType.UnidentifiedSignal), 0f);
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
	}

	public void ActivateClone()
	{
		base.enabled = true;
		NotificationManager.SharedInstance.PostNotification(_signalMessage, pin: true);
		_playerVisuals.SetActive(value: true);
		_signal.SetSignalActivation(active: true);
		float num = 120f;
		Vector3 vector = base.transform.parent.position - Locator.GetPlayerTransform().position;
		Vector3 vector2 = Locator.GetPlayerTransform().position + vector.normalized * num * 0.5f;
		_localMirrorPos = base.transform.parent.InverseTransformPoint(vector2);
		base.transform.position = vector2 + vector.normalized * num * 0.5f;
	}

	private void FixedUpdate()
	{
		Transform playerTransform = Locator.GetPlayerTransform();
		Vector3 vector = base.transform.parent.InverseTransformPoint(playerTransform.position);
		Vector3 vector2 = _localMirrorPos - vector;
		Vector3 position = _localMirrorPos + vector2;
		position.y = vector.y;
		base.transform.position = base.transform.parent.TransformPoint(position);
		Vector3 normalized = (base.transform.position - playerTransform.position).normalized;
		Vector3 forward = Vector3.Reflect(playerTransform.forward, normalized);
		Vector3 upwards = Vector3.Reflect(playerTransform.up, normalized);
		base.transform.rotation = Quaternion.LookRotation(forward, upwards);
		float num = Vector3.Distance(base.transform.position, playerTransform.position);
		if (!_warpFlickerActivated && num < 10f)
		{
			_warpFlickerActivated = true;
			_warpTime = Time.time + 0.5f;
			GlobalMessenger<float, float>.FireEvent("FlickerOffAndOn", 0.5f, 0.5f);
		}
		if (_warpPlayerNextFrame)
		{
			WarpPlayerToCampfire();
		}
	}

	private void Update()
	{
		if (_warpFlickerActivated && Time.time > _warpTime)
		{
			_warpPlayerNextFrame = true;
		}
	}

	private void WarpPlayerToCampfire()
	{
		Locator.GetEyeStateManager().SetState(EyeState.InstrumentHunt);
		_endlessCylinder.SetActivation(active: false);
		_desolateAmbience.FadeOut(5f);
		Vector3 from = base.transform.position - Locator.GetPlayerTransform().position;
		from.y = 0f;
		float angle = OWMath.Angle(from, Locator.GetPlayerTransform().forward, Vector3.up);
		Locator.GetPlayerTransform().rotation = _playerWarpTarget.rotation * Quaternion.AngleAxis(angle, Vector3.up);
		Locator.GetPlayerBody().SetPosition(_playerWarpTarget.position);
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
		NotificationManager.SharedInstance.UnpinNotification(_signalMessage);
		base.gameObject.SetActive(value: false);
		if (Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe() != null)
		{
			Object.Destroy(Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe()
				.gameObject);
				Debug.Log("PROBE DESTROYED (LEFT BEHIND)");
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(base.transform.parent.TransformPoint(_localMirrorPos), 1f);
		}
	}
