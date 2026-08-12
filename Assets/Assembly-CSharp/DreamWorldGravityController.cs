using UnityEngine;

public class DreamWorldGravityController : MonoBehaviour
{
	[SerializeField]
	private DirectionalForceVolume _forceVolume;

	[SerializeField]
	private AnimationCurve _stageOneCurve;

	[SerializeField]
	private AnimationCurve _stageTwoCurve;

	[SerializeField]
	private Vector3 _localTiltAxis = Vector3.forward;

	private Vector3 _origLocalForceDirection;

	private bool _playerSleepingInLighthouse;

	private bool _updateStageOne;

	private bool _updateStageTwo;

	private float _startTime;

	private float _stageOneDuration;

	private float _stageTwoDuration;

	private void Start()
	{
		base.enabled = false;
		_origLocalForceDirection = _forceVolume.GetLocalForceDirection();
		_stageOneDuration = _stageOneCurve[_stageOneCurve.length - 1].time;
		_stageTwoDuration = _stageTwoCurve[_stageTwoCurve.length - 1].time;
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.AddListener("LighthouseCollapseStageOne", OnLighthouseCollapseStageOne);
		GlobalMessenger.AddListener("LighthouseCollapseStageTwo", OnLighthouseCollapseStageTwo);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.RemoveListener("LighthouseCollapseStageOne", OnLighthouseCollapseStageOne);
		GlobalMessenger.RemoveListener("LighthouseCollapseStageTwo", OnLighthouseCollapseStageTwo);
	}

	private void FixedUpdate()
	{
		if (_playerSleepingInLighthouse)
		{
			UpdateGravityDirection();
		}
		if (_updateStageOne && Time.time > _startTime + _stageOneDuration)
		{
			_updateStageOne = false;
		}
		else if (_updateStageTwo && Time.time > _startTime + _stageTwoDuration)
		{
			_updateStageTwo = false;
		}
		if (!_updateStageOne && !_updateStageTwo)
		{
			base.enabled = false;
		}
	}

	private void UpdateGravityDirection()
	{
		float num = 0f;
		Vector3 localForceDirection = _origLocalForceDirection;
		if (_updateStageOne)
		{
			num = _stageOneCurve.Evaluate(Time.time - _startTime);
		}
		else if (_updateStageTwo)
		{
			num = _stageTwoCurve.Evaluate(Time.time - _startTime);
		}
		if (num > 0f)
		{
			localForceDirection = Quaternion.AngleAxis(num, _localTiltAxis) * _origLocalForceDirection;
		}
		_forceVolume.SetLocalForceDirection(localForceDirection);
	}

	private void OnLighthouseCollapseStageOne()
	{
		base.enabled = true;
		_updateStageOne = true;
		_startTime = Time.time;
	}

	private void OnLighthouseCollapseStageTwo()
	{
		base.enabled = true;
		_updateStageTwo = true;
		_startTime = Time.time;
	}

	private void OnEnterDreamWorld()
	{
		if (!PlayerState.IsResurrected() && Locator.GetDreamWorldController().GetDreamCampfire() != null && Locator.GetDreamWorldController().GetDreamCampfire().GetLocation() == DreamArrivalPoint.Location.Zone2)
		{
			_playerSleepingInLighthouse = true;
			UpdateGravityDirection();
		}
	}

	private void OnExitDreamWorld()
	{
		_playerSleepingInLighthouse = false;
		_forceVolume.SetLocalForceDirection(_origLocalForceDirection);
	}
}
