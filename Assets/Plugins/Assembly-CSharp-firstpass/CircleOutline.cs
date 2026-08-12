using System;
using System.Collections.Generic;
using UnityEngine;

public class CircleOutline : ModifiedShadow
{
	[SerializeField]
	private int m_circleCount = 2;

	[SerializeField]
	private int m_firstSample = 4;

	[SerializeField]
	private int m_sampleIncrement = 2;

	public int circleCount
	{
		get
		{
			return m_circleCount;
		}
		set
		{
			m_circleCount = Mathf.Max(value, 1);
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	public int firstSample
	{
		get
		{
			return m_firstSample;
		}
		set
		{
			m_firstSample = Mathf.Max(value, 2);
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	public int sampleIncrement
	{
		get
		{
			return m_sampleIncrement;
		}
		set
		{
			m_sampleIncrement = Mathf.Max(value, 1);
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	protected void OnValidate()
	{
		circleCount = m_circleCount;
		firstSample = m_firstSample;
		sampleIncrement = m_sampleIncrement;
	}

	public override void ModifyVertices(List<UIVertex> verts)
	{
		if (!IsActive())
		{
			return;
		}
		int num = (m_firstSample * 2 + m_sampleIncrement * (m_circleCount - 1)) * m_circleCount / 2;
		verts.Capacity = verts.Count * (num + 1);
		int count = verts.Count;
		int num2 = 0;
		int num3 = m_firstSample;
		float num4 = base.effectDistance.x / (float)circleCount;
		float num5 = base.effectDistance.y / (float)circleCount;
		for (int i = 1; i <= m_circleCount; i++)
		{
			float num6 = num4 * (float)i;
			float num7 = num5 * (float)i;
			float num8 = (float)Math.PI * 2f / (float)num3;
			float num9 = (float)(i % 2) * num8 * 0.5f;
			for (int j = 0; j < num3; j++)
			{
				int num10 = num2 + count;
				ApplyShadow(verts, base.effectColor, num2, num10, num6 * Mathf.Cos(num9), num7 * Mathf.Sin(num9));
				num2 = num10;
				num9 += num8;
			}
			num3 += m_sampleIncrement;
		}
	}
}
