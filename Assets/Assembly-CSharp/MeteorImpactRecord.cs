using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MeteorImpactRecord : ScriptableObject
{
	public List<FragmentData> fragmentData;

	private int loopIndex;

	public void Print()
	{
		FragmentDataComparer comparer = new FragmentDataComparer();
		fragmentData.Sort(comparer);
		for (int num = fragmentData.Count - 1; num >= 0; num--)
		{
			string text = "";
			for (int i = 0; i <= loopIndex; i++)
			{
				text = text + fragmentData[num].hitCount[i] + " ";
			}
			Debug.Log("Total: " + fragmentData[num].totalHitCount + "   Per Loop: " + text + "   " + fragmentData[num].name);
		}
	}

	public void IncrementLoopIndex()
	{
		loopIndex++;
	}

	public void Reset()
	{
		loopIndex = 0;
		fragmentData.Clear();
	}

	public void AddFragment(string fragmentName)
	{
		for (int i = 0; i < fragmentData.Count; i++)
		{
			if (fragmentData[i].name.Equals(fragmentName))
			{
				Debug.LogError("Fragment already added to list: " + fragmentName);
				return;
			}
		}
		FragmentData item = new FragmentData(fragmentName);
		fragmentData.Add(item);
	}

	public void OnImpactFragment(string fragmentName)
	{
		for (int i = 0; i < fragmentData.Count; i++)
		{
			if (fragmentData[i].name.Equals(fragmentName))
			{
				fragmentData[i].OnImpact(loopIndex);
				return;
			}
		}
		Debug.LogError("No record of fragment: " + fragmentName);
	}
}
