using UnityEngine;

public class NomaiWarpReceiver : NomaiWarpPlatform
{
	public delegate void NomaiWarpEvent();

	[SerializeField]
	private Transform _alignmentTarget;

	[SerializeField]
	private OWRendererFadeController _returnGlowFadeController;

	[SerializeField]
	private OWTriggerVolume _returnTrigger;

	private float _exitPlatformTime;

	private bool _waitToActivateReturnWarp;

	private NomaiWarpPlatform _returnPlatform;

	private bool _returnOnEntry;

	public event NomaiWarpEvent OnReceivePlayerBody;

	protected override void Awake()
	{
		_returnGlowFadeController.SetFade(0f);
		base.Awake();
		if (_alignmentTarget == null)
		{
			_alignmentTarget = this.GetAttachedOWRigidbody().transform;
		}
		Locator.RegisterWarpReceiver(this);
		if (_returnTrigger != null)
		{
			_returnTrigger.OnExit += OnExitReturnTrigger;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Locator.UnregisterWarpReceiver(this);
		if (_returnTrigger != null)
		{
			_returnTrigger.OnExit -= OnExitReturnTrigger;
		}
	}

	public Transform GetAlignmentTarget()
	{
		return _alignmentTarget;
	}

	public bool IsReturnWarpEnabled()
	{
		return _returnPlatform != null;
	}

	protected override void ReceiveWarpedBody(OWRigidbody body, NomaiWarpPlatform startPlatform)
	{
		base.ReceiveWarpedBody(body, startPlatform);
		_returnPlatform = startPlatform;
		_returnOnEntry = false;
		_returnGlowFadeController.SetFade(0f);
		if (body.CompareTag("Player") && this.OnReceivePlayerBody != null)
		{
			this.OnReceivePlayerBody();
		}
		if (GetFrequency() == Frequency.TimeLoop)
		{
			MonoBehaviour.print("ENTER TIME LOOP: " + body.gameObject.name);
			GlobalMessenger<OWRigidbody>.FireEvent("EnterTimeLoopCentral", body);
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_waitToActivateReturnWarp && Time.time > _exitPlatformTime + 1f)
		{
			_waitToActivateReturnWarp = false;
			_returnOnEntry = true;
			_returnGlowFadeController.FadeTo(0.5f, 5f);
		}
	}

	protected override void UpdateEnabled()
	{
		if (_objectsOnPlatform.Count == 0 && !IsBlackHoleOpen() && !_waitToActivateReturnWarp)
		{
			base.enabled = false;
		}
	}

	protected override void OnEntry(GameObject hitObj)
	{
		base.OnEntry(hitObj);
		_waitToActivateReturnWarp = false;
		if (_returnOnEntry && _returnPlatform != null)
		{
			OpenBlackHole(_returnPlatform);
			_returnOnEntry = false;
			_returnPlatform = null;
			_returnGlowFadeController.FadeTo(0f, 1f);
		}
	}

	protected override void OnExit(GameObject hitObj)
	{
		base.OnExit(hitObj);
		if (_returnTrigger == null && _objectsOnPlatform.Count == 0 && _returnPlatform != null)
		{
			_waitToActivateReturnWarp = true;
			_exitPlatformTime = Time.time;
		}
	}

	private void OnExitReturnTrigger(GameObject hitObj)
	{
		if (_returnTrigger.getTrackedObjects().Count == 0 && _returnPlatform != null)
		{
			_waitToActivateReturnWarp = true;
			_exitPlatformTime = Time.time;
			base.enabled = true;
		}
	}
}
