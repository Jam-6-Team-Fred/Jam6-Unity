using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Primitives/UILineRenderer")]
	public class UILineRenderer : MaskableGraphic
	{
		[SerializeField]
		private Texture m_Texture;

		[SerializeField]
		private Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

		public bool UseCapPoints;

		public float LineThickness = 2f;

		public bool UseMargins;

		public Vector2 Margin;

		public Vector2[] Points;

		public bool relativeSize;

		private static List<Vector2> s_pointList = new List<Vector2>(1024);

		private static Vector2[] s_vertices = new Vector2[4];

		private static Vector2[] s_uvs = new Vector2[4];

		private static UIVertex[] s_vbo = new UIVertex[4];

		public override Texture mainTexture
		{
			get
			{
				if (!(m_Texture == null))
				{
					return m_Texture;
				}
				return Graphic.s_WhiteTexture;
			}
		}

		public Texture texture
		{
			get
			{
				return m_Texture;
			}
			set
			{
				if (!(m_Texture == value))
				{
					m_Texture = value;
					SetVerticesDirty();
					SetMaterialDirty();
				}
			}
		}

		public Rect uvRect
		{
			get
			{
				return m_UVRect;
			}
			set
			{
				if (!(m_UVRect == value))
				{
					m_UVRect = value;
					SetVerticesDirty();
				}
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (Points == null || Points.Length < 2)
			{
				Points = new Vector2[2]
				{
					new Vector2(0f, 0f),
					new Vector2(1f, 1f)
				};
			}
			int num = 24;
			float num2 = base.rectTransform.rect.width;
			float num3 = base.rectTransform.rect.height;
			float num4 = (0f - base.rectTransform.pivot.x) * base.rectTransform.rect.width;
			float num5 = (0f - base.rectTransform.pivot.y) * base.rectTransform.rect.height;
			if (!relativeSize)
			{
				num2 = 1f;
				num3 = 1f;
			}
			if (UseCapPoints)
			{
				s_pointList.Add(Points[0]);
				Vector2 item = Points[0] + (Points[1] - Points[0]).normalized * num;
				s_pointList.Add(item);
			}
			for (int i = 1; i < Points.Length - 1; i++)
			{
				s_pointList.Add(Points[i]);
			}
			if (UseCapPoints)
			{
				Vector2 item2 = Points[Points.Length - 1] - (Points[Points.Length - 1] - Points[Points.Length - 2]).normalized * num;
				s_pointList.Add(item2);
				s_pointList.Add(Points[Points.Length - 1]);
			}
			if (UseMargins)
			{
				num2 -= Margin.x;
				num3 -= Margin.y;
				num4 += Margin.x / 2f;
				num5 += Margin.y / 2f;
			}
			vh.Clear();
			Vector2 vector = Vector2.zero;
			Vector2 vector2 = Vector2.zero;
			for (int j = 1; j < s_pointList.Count; j++)
			{
				Vector2 vector3 = s_pointList[j - 1];
				Vector2 vector4 = s_pointList[j];
				vector3 = new Vector2(vector3.x * num2 + num4, vector3.y * num3 + num5);
				vector4 = new Vector2(vector4.x * num2 + num4, vector4.y * num3 + num5);
				float z = Mathf.Atan2(vector4.y - vector3.y, vector4.x - vector3.x) * 180f / (float)Math.PI;
				Vector2 vector5 = vector3 + new Vector2(0f, (0f - LineThickness) / 2f);
				Vector2 vector6 = vector3 + new Vector2(0f, LineThickness / 2f);
				Vector2 vector7 = vector4 + new Vector2(0f, LineThickness / 2f);
				Vector2 vector8 = vector4 + new Vector2(0f, (0f - LineThickness) / 2f);
				vector5 = RotatePointAroundPivot(vector5, vector3, new Vector3(0f, 0f, z));
				vector6 = RotatePointAroundPivot(vector6, vector3, new Vector3(0f, 0f, z));
				vector7 = RotatePointAroundPivot(vector7, vector4, new Vector3(0f, 0f, z));
				vector8 = RotatePointAroundPivot(vector8, vector4, new Vector3(0f, 0f, z));
				Vector2 vector9 = new Vector2(m_UVRect.xMin, m_UVRect.yMin);
				Vector2 vector10 = new Vector2(m_UVRect.xMin, m_UVRect.yMax);
				Vector2 vector11 = new Vector2(m_UVRect.center.x, m_UVRect.yMin);
				Vector2 vector12 = new Vector2(m_UVRect.center.x, m_UVRect.yMax);
				Vector2 vector13 = new Vector2(m_UVRect.xMax, m_UVRect.yMin);
				Vector2 vector14 = new Vector2(m_UVRect.xMax, m_UVRect.yMax);
				s_vertices[0] = vector;
				s_vertices[1] = vector2;
				s_vertices[2] = vector5;
				s_vertices[3] = vector6;
				s_uvs[0] = vector11;
				s_uvs[1] = vector12;
				s_uvs[2] = vector12;
				s_uvs[3] = vector11;
				if (j > 1)
				{
					vh.AddUIVertexQuad(SetVbo(s_vertices, s_uvs));
				}
				s_vertices[0] = vector5;
				s_vertices[1] = vector6;
				s_vertices[2] = vector7;
				s_vertices[3] = vector8;
				if (j == 1)
				{
					s_uvs[0] = vector9;
					s_uvs[1] = vector10;
					s_uvs[2] = vector12;
					s_uvs[3] = vector11;
				}
				else if (j == s_pointList.Count - 1)
				{
					s_uvs[0] = vector11;
					s_uvs[1] = vector12;
					s_uvs[2] = vector14;
					s_uvs[3] = vector13;
				}
				vh.AddUIVertexQuad(SetVbo(s_vertices, s_uvs));
				vector = vector7;
				vector2 = vector8;
			}
			s_pointList.Clear();
		}

		protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				UIVertex simpleVert = UIVertex.simpleVert;
				simpleVert.color = color;
				simpleVert.position = vertices[i];
				simpleVert.uv0 = uvs[i];
				s_vbo[i] = simpleVert;
			}
			return s_vbo;
		}

		public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
		{
			Vector3 vector = point - pivot;
			vector = Quaternion.Euler(angles) * vector;
			point = vector + pivot;
			return point;
		}

		public void CopyTo(ref UILineRenderer lineRenderer)
		{
			lineRenderer.texture = m_Texture;
			lineRenderer.uvRect = m_UVRect;
			lineRenderer.UseCapPoints = UseCapPoints;
			lineRenderer.LineThickness = LineThickness;
			lineRenderer.UseMargins = UseMargins;
			lineRenderer.Margin = Margin;
			lineRenderer.Points = Points;
			lineRenderer.relativeSize = relativeSize;
		}
	}
}
