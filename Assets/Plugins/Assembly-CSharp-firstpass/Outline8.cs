using System.Collections.Generic;
using UnityEngine;

public class Outline8 : ModifiedShadow
{
	public override void ModifyVertices(List<UIVertex> verts)
	{
		if (!IsActive())
		{
			return;
		}
		verts.Capacity = verts.Count * 9;
		int count = verts.Count;
		int num = 0;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (i != 0 || j != 0)
				{
					int num2 = num + count;
					ApplyShadow(verts, base.effectColor, num, num2, base.effectDistance.x * (float)i, base.effectDistance.y * (float)j);
					num = num2;
				}
			}
		}
	}
}
