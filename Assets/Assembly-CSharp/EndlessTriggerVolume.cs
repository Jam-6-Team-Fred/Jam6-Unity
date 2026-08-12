using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public abstract class EndlessTriggerVolume : MonoBehaviour
{
	private OWTriggerVolume _trigger;

	private bool _active = true;

	private bool _warpPlayer;

	private bool _warpProbe;

	private float _flickerOutTime;

	protected virtual void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		base.enabled = false;
	}

	protected virtual void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	public void SetActivation(bool active)
	{
		if (!active)
		{
			_warpPlayer = false;
			_warpProbe = false;
			base.enabled = false;
		}
		_active = active;
	}

	protected abstract void WarpBody(OWRigidbody body);

	private void OnEntry(GameObject hitObj)
	{
		if (_active)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				_warpPlayer = false;
			}
			else if (hitObj.CompareTag("ProbeDetector"))
			{
				_warpProbe = false;
			}
			if (!_warpPlayer && !_warpProbe)
			{
				base.enabled = false;
			}
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (_active)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				_warpPlayer = true;
			}
			else if (hitObj.CompareTag("ProbeDetector") && Locator.GetProbe().IsLaunched() && !Locator.GetProbe().IsRetrieving() && !DialogueConditionManager.SharedInstance.GetConditionState("PROBE_ENTERED_EYE"))
			{
				_warpProbe = true;
			}
			if (!base.enabled && (_warpPlayer || _warpProbe))
			{
				float num = 0.5f;
				GlobalMessenger<float, float>.FireEvent("FlickerOffAndOn", num, 2f);
				_flickerOutTime = Time.time + num;
				base.enabled = true;
			}
		}
	}

	private void FixedUpdate()
	{
		if (Time.time >= _flickerOutTime)
		{
			if (_warpPlayer)
			{
				WarpBody(Locator.GetPlayerBody());
			}
			if (_warpProbe && Locator.GetProbe() != null && Locator.GetProbe().IsLaunched())
			{
				WarpBody(Locator.GetProbe().GetOWRigidbody());
			}
			base.enabled = (_warpProbe = (_warpPlayer = false));
		}
	}
}
