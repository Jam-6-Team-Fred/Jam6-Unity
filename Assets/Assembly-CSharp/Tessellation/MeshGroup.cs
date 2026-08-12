using System;
using UnityEngine;

namespace Tessellation
{
	[Serializable]
	public class MeshGroup : ScriptableObject
	{
		public Mesh[] variants;

		public MeshGroup()
		{
			variants = new Mesh[16];
		}

		public Mesh GetVariant(bool splitUpper, bool splitRight, bool splitLower, bool splitLeft)
		{
			int num = 0;
			if (splitUpper)
			{
				num |= 1;
			}
			if (splitRight)
			{
				num |= 2;
			}
			if (splitLower)
			{
				num |= 4;
			}
			if (splitLeft)
			{
				num |= 8;
			}
			return variants[num];
		}
	}
}
