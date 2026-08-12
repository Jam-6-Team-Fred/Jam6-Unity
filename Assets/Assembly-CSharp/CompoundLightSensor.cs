using UnityEngine;

public class CompoundLightSensor : LightSensor
{
	[SerializeField]
	private SingleLightSensor[] _childSensors;

	private int _illuminatedCount;

	private void Awake()
	{
		for (int i = 0; i < _childSensors.Length; i++)
		{
			_childSensors[i].OnDetectLight += new OWEvent.OWCallback(OnChildDetectLight);
			_childSensors[i].OnDetectDarkness += new OWEvent.OWCallback(OnChildDetectDarkness);
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _childSensors.Length; i++)
		{
			_childSensors[i].OnDetectLight -= new OWEvent.OWCallback(OnChildDetectLight);
			_childSensors[i].OnDetectDarkness -= new OWEvent.OWCallback(OnChildDetectDarkness);
		}
	}

	public override bool IsIlluminated()
	{
		return _illuminatedCount > 0;
	}

	public override bool IsIlluminatedByGhostLantern()
	{
		if (_illuminatedCount == 0)
		{
			return false;
		}
		for (int i = 0; i < _childSensors.Length; i++)
		{
			if (_childSensors[i].IsIlluminatedByGhostLantern())
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsIlluminatedByLantern(DreamLanternController lantern)
	{
		if (_illuminatedCount == 0)
		{
			return false;
		}
		for (int i = 0; i < _childSensors.Length; i++)
		{
			if (_childSensors[i].IsIlluminatedByLantern(lantern))
			{
				return true;
			}
		}
		return false;
	}

	private void OnChildDetectLight()
	{
		_illuminatedCount++;
		if (_illuminatedCount == 1)
		{
			OnDetectLight.Invoke();
		}
	}

	private void OnChildDetectDarkness()
	{
		_illuminatedCount--;
		if (_illuminatedCount == 0)
		{
			OnDetectDarkness.Invoke();
		}
	}
}
