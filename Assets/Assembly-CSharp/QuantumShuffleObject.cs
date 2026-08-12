using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(QuantumObject))]
public class QuantumShuffleObject : QuantumObject
{
	[SerializeField]
	private Transform[] _shuffledObjects;

	private Vector3[] _localPositions;

	private List<int> _indexList;

	protected override void Awake()
	{
		base.Awake();
		_indexList = new List<int>(_shuffledObjects.Length);
		_localPositions = new Vector3[_shuffledObjects.Length];
		for (int i = 0; i < _shuffledObjects.Length; i++)
		{
			_localPositions[i] = _shuffledObjects[i].localPosition;
		}
	}

	protected override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
	{
		_indexList.Clear();
		for (int i = 0; i < _localPositions.Length; i++)
		{
			_indexList.Add(i);
		}
		for (int j = 0; j < _shuffledObjects.Length; j++)
		{
			int num = _indexList[Random.Range(0, _indexList.Count)];
			_indexList.Remove(num);
			_shuffledObjects[j].localPosition = _localPositions[num];
		}
		return true;
	}
}
