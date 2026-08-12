using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Effects/Letter Spacing", 15)]
	public class TextStyleApplier : BaseMeshEffect
	{
		private const string SupportedTagRegexPattersn = "<b>|</b>|<i>|</i>|<size=.*?>|</size>|<color=.*?>|</color>|<material=.*?>|</material>";

		private const string LatinCharactersRegexPattern = "[0-z]";

		[SerializeField]
		private bool useRichText;

		[SerializeField]
		private float m_spacing;

		[SerializeField]
		private Font m_font;

		[SerializeField]
		private float m_fixedWidth;

		private float m_rawWidth;

		private float m_widthOffset;

		[SerializeField]
		private LayoutElement parentOverrideLayoutElement;

		private LayoutElement m_layoutElement;

		private Text _text;

		private Text textComponent
		{
			get
			{
				if (_text == null)
				{
					_text = GetComponent<Text>();
				}
				return _text;
			}
		}

		public float spacing
		{
			get
			{
				return m_spacing;
			}
			set
			{
				if (m_spacing != value)
				{
					m_spacing = value;
					if (base.graphic != null)
					{
						base.graphic.SetVerticesDirty();
					}
				}
			}
		}

		public Font font
		{
			get
			{
				return m_font;
			}
			set
			{
				if (!(m_font == value))
				{
					m_font = value;
					if (m_font != null && textComponent != null)
					{
						textComponent.font = m_font;
					}
					if (base.graphic != null)
					{
						base.graphic.SetVerticesDirty();
					}
				}
			}
		}

		public float fixedWidth
		{
			get
			{
				return m_fixedWidth;
			}
			set
			{
				if (m_fixedWidth != value)
				{
					m_fixedWidth = value;
					if (base.graphic != null)
					{
						base.graphic.SetVerticesDirty();
					}
				}
			}
		}

		public float widthOffset => m_widthOffset;

		protected TextStyleApplier()
		{
		}

		private string[] GetLines()
		{
			IList<UILineInfo> lines = textComponent.cachedTextGenerator.lines;
			string[] array = new string[lines.Count];
			for (int i = 0; i < lines.Count; i++)
			{
				if (i + 1 < lines.Count)
				{
					int length = lines[i + 1].startCharIdx - 1 - lines[i].startCharIdx;
					array[i] = textComponent.text.Substring(lines[i].startCharIdx, length);
				}
				else
				{
					array[i] = textComponent.text.Substring(lines[i].startCharIdx);
				}
			}
			return array;
		}

		public void ModifyVertices(List<UIVertex> verts)
		{
			if (!IsActive())
			{
				return;
			}
			if (textComponent == null)
			{
				Debug.LogWarning("LetterSpacing: Missing Text component");
				return;
			}
			string[] lines = GetLines();
			float num = spacing * (float)textComponent.fontSize / 100f;
			float num2 = 0f;
			int num3 = 0;
			switch (textComponent.alignment)
			{
			case TextAnchor.UpperLeft:
			case TextAnchor.MiddleLeft:
			case TextAnchor.LowerLeft:
				num2 = 0f;
				break;
			case TextAnchor.UpperCenter:
			case TextAnchor.MiddleCenter:
			case TextAnchor.LowerCenter:
				num2 = 0.5f;
				break;
			case TextAnchor.UpperRight:
			case TextAnchor.MiddleRight:
			case TextAnchor.LowerRight:
				num2 = 1f;
				break;
			}
			float num4 = float.MinValue;
			float rawWidth = float.MinValue;
			float num5 = float.MinValue;
			for (int i = 0; i < lines.Length; i++)
			{
				string text = WithoutRichText(lines[i]);
				int length = text.Length;
				float num6 = (float)(length - 1) * num * num2;
				float num7 = (float)(length - 1) * num;
				float num8 = 0f;
				int num9 = 0;
				int num10 = 0;
				while (num9 < text.Length)
				{
					if (text[num9] != ' ')
					{
						int index = num3 * 6;
						int index2 = num3 * 6 + 1;
						int index3 = num3 * 6 + 2;
						int num11 = num3 * 6 + 3;
						int index4 = num3 * 6 + 4;
						int index5 = num3 * 6 + 5;
						if (num11 > verts.Count - 1)
						{
							return;
						}
						UIVertex value = verts[index];
						UIVertex value2 = verts[index2];
						UIVertex value3 = verts[index3];
						UIVertex value4 = verts[num11];
						UIVertex value5 = verts[index4];
						UIVertex value6 = verts[index5];
						bool flag = false;
						if (m_fixedWidth > 0f)
						{
							if (Regex.Matches(text.Substring(num10, 1), "[0-z]").Count > 0)
							{
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
						if (flag)
						{
							float num12 = verts[num3 * 6].position.x;
							float num13 = num12;
							for (int j = 1; j < 6; j++)
							{
								float x = verts[num3 * 6 + j].position.x;
								if (x < num12)
								{
									num12 = x;
								}
								if (x > num13)
								{
									num13 = x;
								}
							}
							float num14 = num13 - num12;
							num8 += num14;
						}
						else
						{
							num8 += m_fixedWidth + num;
						}
						Vector3 vector = Vector3.right * (num * (float)num10 - num6);
						value.position += vector;
						value2.position += vector;
						value3.position += vector;
						value4.position += vector;
						value5.position += vector;
						value6.position += vector;
						verts[index] = value;
						verts[index2] = value2;
						verts[index3] = value3;
						verts[num11] = value4;
						verts[index4] = value5;
						verts[index5] = value6;
						num3++;
					}
					num9++;
					num10++;
				}
				if (num8 + num7 > num5)
				{
					rawWidth = num8;
					num4 = num7;
					num5 = num8 + num7;
				}
			}
			m_widthOffset = num4;
			m_rawWidth = rawWidth;
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!IsActive())
			{
				return;
			}
			List<UIVertex> list = new List<UIVertex>();
			vh.GetUIVertexStream(list);
			ModifyVertices(list);
			vh.Clear();
			vh.AddUIVertexTriangleStream(list);
			if (parentOverrideLayoutElement != null)
			{
				parentOverrideLayoutElement.preferredWidth = (m_rawWidth + m_widthOffset) * base.graphic.rectTransform.localScale.x;
				return;
			}
			if (m_layoutElement == null)
			{
				m_layoutElement = GetComponent<LayoutElement>();
			}
			if (m_layoutElement != null)
			{
				m_layoutElement.preferredWidth = (m_rawWidth + m_widthOffset) * base.graphic.rectTransform.localScale.x;
			}
		}

		private string WithoutRichText(string line)
		{
			line = Regex.Replace(line, "<b>|</b>|<i>|</i>|<size=.*?>|</size>|<color=.*?>|</color>|<material=.*?>|</material>", "");
			return line;
		}
	}
}
