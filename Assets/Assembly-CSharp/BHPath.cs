using System.Collections.Generic;
using UnityEngine;

public class BHPath
{
	[SerializeField]
	private List<FragmentIntegrity> _fragments;

	private List<List<int>> _pathNodes;

	protected virtual void Awake()
	{
		for (int i = 0; i < _fragments.Count; i++)
		{
			_fragments[i].OnTakeDamage += OnTakeDamage;
			List<int> list = new List<int>(10);
			BHPathNode pathNode = _fragments[i].GetPathNode();
			for (int j = 0; j < _fragments.Count; j++)
			{
				for (int k = 0; k < pathNode.reachableNodes.Count; k++)
				{
					if (_fragments[j].GetPathNode() == pathNode.reachableNodes[k])
					{
						list.Add(j);
					}
				}
			}
			_pathNodes.Add(list);
		}
		RunPathAlgorythm();
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _fragments.Count; i++)
		{
			_fragments[i].OnTakeDamage -= OnTakeDamage;
		}
	}

	private void OnTakeDamage(float integrity)
	{
		if (integrity != 0f)
		{
			return;
		}
		for (int i = 0; i < _fragments.Count; i++)
		{
			if (_fragments[i].GetIntegrity() == 0f)
			{
				_fragments.RemoveAt(i);
				_pathNodes.RemoveAt(i);
				i--;
			}
		}
		RunPathAlgorythm();
	}

	private void RunPathAlgorythm()
	{
	}
}
