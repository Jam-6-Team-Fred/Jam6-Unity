using UnityEngine;

public class ProbeTrajectoryHologram : Hologram
{
	[SerializeField]
	private AnimationCurve _launchRateCurve;

	[SerializeField]
	private GameObject _probePathPrefab;

	[SerializeField]
	private Transform _probeLauncherTransform;

	[SerializeField]
	private Transform _solarSystemTransform;

	[SerializeField]
	private Transform _eyeSocket;

	[SerializeField]
	private bool _latestLaunchOnly;

	private float _startSpawnTime;

	private float _startZoomTime;

	private float _frameSpawnCount;

	private int _pathCount;

	private float _curveScalar;

	private const int EYE_PATH_COUNT = 1000;

	private const float TOTAL_DURATION = 10f;

	private int _totalPathCount;

	private void OnDestroy()
	{
	}

	protected override void OnFinishActivation()
	{
		_pathCount = 0;
		_totalPathCount = 1000 + TimeLoop.GetLoopCount();
		_startZoomTime = Time.time + 1f;
		_frameSpawnCount = 0f;
		int num = (int)(10f / Time.fixedDeltaTime);
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			float time = (float)i / (float)num;
			num2 += _launchRateCurve.Evaluate(time) / (float)num;
		}
		_curveScalar = (float)_totalPathCount / num2;
	}

	protected override void OnDeactivation()
	{
	}

	protected override void UpdateHologram()
	{
		float num = 3f;
		float num2 = (Time.time - _startZoomTime) / num;
		float num3 = 0.05f;
		_solarSystemTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * num3, Mathf.SmoothStep(0f, 1f, num2));
		float num4 = _startZoomTime + num - 0.5f;
		float time = (Time.time - num4) / 10f;
		if (_latestLaunchOnly)
		{
			if (num2 >= 1f)
			{
				MonoBehaviour.print("HOLOGRAM: spawn probe path and call CompleteHologram()");
				SpawnPath();
				CompleteHologram();
			}
		}
		else if (Time.time > num4 + 10f + 3f)
		{
			CompleteHologram();
		}
		else
		{
			float num5 = _curveScalar * _launchRateCurve.Evaluate(time) * (Time.fixedDeltaTime / 10f);
			_frameSpawnCount += num5;
			while (_frameSpawnCount > 1f)
			{
				_frameSpawnCount -= 1f;
				SpawnPath();
			}
		}
	}

	private void SpawnPath()
	{
		_pathCount++;
		GameObject obj = Object.Instantiate(_probePathPrefab);
		obj.transform.parent = _solarSystemTransform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		ProbePathAnimator component = obj.GetComponent<ProbePathAnimator>();
		Vector3 vector = Random.insideUnitSphere.normalized;
		if (_pathCount == 1000)
		{
			vector = base.transform.InverseTransformDirection(_eyeSocket.position - _probeLauncherTransform.position).normalized;
		}
		else if (_latestLaunchOnly)
		{
			vector = base.transform.InverseTransformDirection(Locator.GetAstroObject(AstroObject.Name.ProbeCannon).transform.forward);
		}
		component.Init(_solarSystemTransform.InverseTransformPoint(_probeLauncherTransform.position), vector.normalized, 200f, 2f, _pathCount == 1000, _latestLaunchOnly);
	}
}
