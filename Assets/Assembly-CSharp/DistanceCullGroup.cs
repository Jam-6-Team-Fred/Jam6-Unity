using UnityEngine;

[RequireComponent(typeof(CullDistanceTracker))]
public class DistanceCullGroup : CullGroup
{
	[SerializeField]
	private int _minLevel;

	[SerializeField]
	private int _maxLevel;

	private CullDistanceTracker _lodTracker;

	protected override void Awake()
	{
		base.Awake();
		_lodTracker = this.GetRequiredComponent<CullDistanceTracker>();
		_lodTracker.OnChangeLevel += OnChangeLevel;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_lodTracker.OnChangeLevel -= OnChangeLevel;
	}

	private void OnChangeLevel(int level)
	{
		if (level >= _minLevel && level <= _maxLevel && !IsVisible())
		{
			SetVisible(visible: true);
		}
		else if (level < _minLevel || (level > _maxLevel && IsVisible()))
		{
			SetVisible(visible: false);
		}
	}
}
