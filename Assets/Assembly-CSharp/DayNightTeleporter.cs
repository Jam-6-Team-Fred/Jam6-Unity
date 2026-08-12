using UnityEngine;

public class DayNightTeleporter : MonoBehaviour
{
	[SerializeField]
	private Transform _rootTransform;

	[SerializeField]
	private bool _noctural;

	[SerializeField]
	private Transform _headStartingPoint;

	[SerializeField]
	private Transform _tailStartingPoint;

	private DayNightPlanetController _dayNightController;

	private void Start()
	{
		_dayNightController = GetComponentInParent<DayNightPlanetController>();
		_dayNightController.OnDayHeads += OnDayHeads;
		_dayNightController.OnDayTails += OnDayTails;
		if ((_dayNightController.IsDay(heads: true) && _noctural) || (_dayNightController.IsDay(heads: false) && !_noctural))
		{
			_rootTransform.SetPositionAndRotation(_tailStartingPoint.position, _tailStartingPoint.rotation);
		}
		else
		{
			_rootTransform.SetPositionAndRotation(_headStartingPoint.position, _headStartingPoint.rotation);
		}
	}

	private void OnDayHeads()
	{
		if (_noctural)
		{
			_rootTransform.SetPositionAndRotation(_tailStartingPoint.position, _tailStartingPoint.rotation);
		}
		else
		{
			_rootTransform.SetPositionAndRotation(_headStartingPoint.position, _headStartingPoint.rotation);
		}
	}

	private void OnDayTails()
	{
		if (_noctural)
		{
			_rootTransform.SetPositionAndRotation(_headStartingPoint.position, _headStartingPoint.rotation);
		}
		else
		{
			_rootTransform.SetPositionAndRotation(_tailStartingPoint.position, _tailStartingPoint.rotation);
		}
	}
}
