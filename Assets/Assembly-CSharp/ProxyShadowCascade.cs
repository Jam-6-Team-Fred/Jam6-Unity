using System;

public static class ProxyShadowCascade
{
	[Flags]
	public enum Flags
	{
		Near = 1,
		Mid = 2,
		Far = 4,
		Final = 8
	}

	[Serializable]
	public struct Division
	{
		public Flags shadowGroup;

		public float fraction;

		public Division(Flags cascadeShadowGroup, float cascadeFraction)
		{
			shadowGroup = cascadeShadowGroup;
			fraction = cascadeFraction;
		}
	}

	public const int numCascadeFlags = 4;

	public static Flags IndexToCascadeFlag(int index)
	{
		switch (index)
		{
		case 0:
			return Flags.Near;
		case 1:
			return Flags.Mid;
		case 2:
			return Flags.Far;
		case 3:
			return Flags.Final;
		default:
			return (Flags)0;
		}
	}

	public static int CascadeFlagToIndex(Flags flag)
	{
		switch (flag)
		{
		case Flags.Near:
			return 0;
		case Flags.Mid:
			return 1;
		case Flags.Far:
			return 2;
		case Flags.Final:
			return 3;
		default:
			return -1;
		}
	}
}
