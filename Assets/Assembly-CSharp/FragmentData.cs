using System;

[Serializable]
public class FragmentData
{
	public string name;

	public int[] hitCount;

	public int totalHitCount;

	public FragmentData(string name)
	{
		this.name = name;
		hitCount = new int[100];
		totalHitCount = 0;
	}

	public void OnImpact(int loopIndex)
	{
		hitCount[loopIndex]++;
		totalHitCount++;
	}
}
