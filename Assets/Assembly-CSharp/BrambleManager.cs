using UnityEngine;

public class BrambleManager : MonoBehaviour
{
	private BrambleGrower[] _brambleGrowers;

	private int _growIndex;

	private float _intervalFraction;

	private float _lastGrowFraction;

	private void Start()
	{
		_brambleGrowers = new BrambleGrower[base.transform.childCount];
		int num = 0;
		foreach (Transform item in base.transform)
		{
			_brambleGrowers[num] = item.gameObject.AddComponent<BrambleGrower>();
			num++;
		}
		_growIndex = _brambleGrowers.Length - 1;
		_intervalFraction = 1f / (float)_brambleGrowers.Length;
	}

	private void Update()
	{
		if (TimeLoop.GetFractionElapsed() > _lastGrowFraction + _intervalFraction)
		{
			_brambleGrowers[_growIndex].Grow();
			_growIndex--;
			_lastGrowFraction = TimeLoop.GetFractionElapsed();
			if (_growIndex < 0)
			{
				base.enabled = false;
			}
		}
	}
}
