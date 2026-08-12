using System;
using System.Collections.Generic;

public static class ComparerLibrary
{
	[Serializable]
	public class DuplicateFloatKeyComparer : IComparer<float>
	{
		public int Compare(float f1, float f2)
		{
			int num = f1.CompareTo(f2);
			if (num == 0)
			{
				return 1;
			}
			return num;
		}
	}

	[Serializable]
	public class IntComparer : IEqualityComparer<int>
	{
		public bool Equals(int i1, int i2)
		{
			return i1.Equals(i2);
		}

		public int GetHashCode(int i)
		{
			return i.GetHashCode();
		}
	}

	[Serializable]
	public class StringComparer : IEqualityComparer<string>
	{
		public bool Equals(string s1, string s2)
		{
			return s1.Equals(s2);
		}

		public int GetHashCode(string s)
		{
			return s.GetHashCode();
		}
	}

	public static readonly IntComparer intEqComparer = new IntComparer();

	public static readonly StringComparer stringEqComparer = new StringComparer();

	public static readonly DuplicateFloatKeyComparer dupKeyFloatComparer = new DuplicateFloatKeyComparer();
}
