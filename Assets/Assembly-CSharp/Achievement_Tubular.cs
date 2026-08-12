using UnityEngine;

public class Achievement_Tubular : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _triggerVolume;

	[SerializeField]
	private OWRingRiverCollider _riverCollider;

	private float _surfingStartTime;

	private bool _inSurfVolume;

	private bool _hasStartedSurfing;

	private float _startingRotation;

	private void Awake()
	{
		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;
		GlobalMessenger.AddListener("DamBroken", OnDamBroken);
	}

	private void Start()
	{
		base.enabled = false;
		_startingRotation = base.transform.localEulerAngles.y;
		_triggerVolume.SetTriggerActivation(active: false);
	}

	protected virtual void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEntry;
		_triggerVolume.OnExit -= OnExit;
		GlobalMessenger.RemoveListener("DamBroken", OnDamBroken);
	}

	private void FixedUpdate()
	{
		bool flag = _inSurfVolume && PlayerState.IsRidingRaft(raftMustBeInWater: false);
		if (!_hasStartedSurfing && flag)
		{
			_hasStartedSurfing = true;
			_surfingStartTime = Time.time;
		}
		else if (_hasStartedSurfing && !flag)
		{
			MonoBehaviour.print("Stop Surfing Time: " + (Time.time - _surfingStartTime) + "   In surf volume: " + _inSurfVolume.ToString() + "   Riding raft: " + PlayerState.IsRidingRaft(raftMustBeInWater: false).ToString());
			_triggerVolume.SetTriggerActivation(active: false);
			base.enabled = false;
			return;
		}
		base.transform.Rotate(Vector3.up, _startingRotation + _riverCollider.GetFloodWaveDegree() - base.transform.localEulerAngles.y);
		if (flag && Time.time - _surfingStartTime > 15f)
		{
			Achievements.Earn(Achievements.Type.TUBULAR);
			_triggerVolume.SetTriggerActivation(active: false);
			base.enabled = false;
		}
		if (_riverCollider.GetFloodLerp() > 0.95f)
		{
			_triggerVolume.SetTriggerActivation(active: false);
			base.enabled = false;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_inSurfVolume = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_inSurfVolume = false;
		}
	}

	private void OnDamBroken()
	{
		base.enabled = true;
		_triggerVolume.SetTriggerActivation(active: true);
	}
}
