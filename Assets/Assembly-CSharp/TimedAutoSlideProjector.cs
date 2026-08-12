using UnityEngine;

public class TimedAutoSlideProjector : AutoSlideProjector
{
	[SerializeField]
	private TimedSlideCollection[] _slideCollections;

	private int _activeIndex = -1;

	private void OnValidate()
	{
		if (_slideCollections == null || _slideCollections.Length <= 1)
		{
			return;
		}
		for (int i = 1; i < _slideCollections.Length; i++)
		{
			if (_slideCollections[i].playAfterSeconds <= _slideCollections[i - 1].playAfterSeconds)
			{
				_slideCollections[i].playAfterSeconds = _slideCollections[i - 1].playAfterSeconds + 1f;
			}
		}
	}

	protected override void OnSectorOccupantAdded(SectorDetector detector)
	{
		UpdateSlideCollection();
		base.OnSectorOccupantAdded(detector);
	}

	protected override void Update()
	{
		UpdateSlideCollection();
		base.Update();
	}

	private void UpdateSlideCollection()
	{
		if (_activeIndex == _slideCollections.Length - 1)
		{
			return;
		}
		for (int i = _activeIndex + 1; i < _slideCollections.Length; i++)
		{
			if (TimeLoop.GetSecondsElapsed() >= _slideCollections[i].playAfterSeconds)
			{
				_activeIndex = i;
				SetSlideCollection(_slideCollections[i].slideCollection);
				break;
			}
		}
	}
}
