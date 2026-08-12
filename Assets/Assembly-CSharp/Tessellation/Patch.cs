using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tessellation
{
	public class Patch
	{
		public enum RenderMode
		{
			Planar = 0,
			Cubic = 1,
			Spherical = 2,
			Cylindrical = 3
		}

		public enum CullingMode
		{
			None = 0,
			Normal = 1,
			ShadowCasting = 2
		}

		private struct Edge
		{
			private Patch _p0;

			private Patch _p1;

			public Patch p0 => _p0;

			public Patch p1
			{
				get
				{
					if (_p1 == null)
					{
						return _p0;
					}
					return _p1;
				}
			}

			public bool isDualEdge
			{
				get
				{
					if (_p0 != null)
					{
						return _p1 != null;
					}
					return false;
				}
			}

			public Edge(Patch neighbor1, Patch neighbor2 = null)
			{
				_p0 = neighbor1;
				_p1 = neighbor2;
			}

			public void Set(Patch neighbor1, Patch neighbor2)
			{
				_p0 = neighbor1;
				_p1 = neighbor2;
			}

			public void Clear()
			{
				_p0 = null;
				_p1 = null;
			}

			public bool IsConnectedTo(Patch patch)
			{
				if (patch == null)
				{
					return false;
				}
				if (_p0 != patch)
				{
					return _p1 == patch;
				}
				return true;
			}
		}

		private struct Neighbors
		{
			private Edge[] _array;

			public Edge upper => _array[0];

			public Edge right => _array[1];

			public Edge lower => _array[2];

			public Edge left => _array[3];

			public Edge this[int index] => _array[index];

			public void Init()
			{
				_array = new Edge[4];
			}

			public void Clear()
			{
				_array[0].Clear();
				_array[1].Clear();
				_array[2].Clear();
				_array[3].Clear();
			}

			public void SetNeighborEdge(int index, Patch neighbor1, Patch neighbor2 = null)
			{
				_array[index].Set(neighbor1, neighbor2);
			}
		}

		private struct Subpatches
		{
			public bool created;

			private Patch[] _array;

			public Patch upperLeft
			{
				get
				{
					return _array[0];
				}
				set
				{
					_array[0] = value;
				}
			}

			public Patch upperRight
			{
				get
				{
					return _array[1];
				}
				set
				{
					_array[1] = value;
				}
			}

			public Patch lowerRight
			{
				get
				{
					return _array[2];
				}
				set
				{
					_array[2] = value;
				}
			}

			public Patch lowerLeft
			{
				get
				{
					return _array[3];
				}
				set
				{
					_array[3] = value;
				}
			}

			public Patch this[int index]
			{
				get
				{
					return _array[index];
				}
				set
				{
					_array[index] = value;
				}
			}

			public void Init()
			{
				_array = new Patch[4];
			}

			public void Clear()
			{
				created = false;
				_array[0] = null;
				_array[1] = null;
				_array[2] = null;
				_array[3] = null;
			}
		}

		private struct Bounds2D
		{
			public Vector2 center;

			public Vector2 extents;

			public Vector2 size
			{
				get
				{
					return extents * 2f;
				}
				set
				{
					extents.x = value.x * 0.5f;
					extents.y = value.y * 0.5f;
				}
			}

			public float minX => center.x - extents.x;

			public float minY => center.y - extents.y;

			public float maxX => center.x + extents.x;

			public float maxY => center.y + extents.y;

			public Vector3 center3D => new Vector3(center.x, 0f, center.y);

			public Vector3 extents3D => new Vector3(extents.x, 0f, extents.y);

			public Vector3 size3D => new Vector3(extents.x * 2f, 0f, extents.y * 2f);

			public Bounds2D(Vector2 center, Vector2 size)
			{
				this.center = center;
				extents = size * 0.5f;
			}

			public Bounds2D(float centerX, float centerY, float sizeX, float sizeY)
			{
				center = new Vector2(centerX, centerY);
				extents = new Vector2(sizeX * 0.5f, sizeY * 0.5f);
			}

			public void Set(Vector2 center, Vector2 size)
			{
				this.center = center;
				extents = size * 0.5f;
			}

			public void Set(float centerX, float centerY, float sizeX, float sizeY)
			{
				center.Set(centerX, centerY);
				extents.Set(sizeX * 0.5f, sizeY * 0.5f);
			}

			public bool Contains(Vector2 point)
			{
				if (point.x >= minX && point.x <= maxX && point.y >= minY)
				{
					return point.y <= maxY;
				}
				return false;
			}

			public bool Intersects(Bounds2D bounds)
			{
				if (minX <= bounds.maxX && maxX >= bounds.minX && minY <= bounds.maxY)
				{
					return maxY >= bounds.minY;
				}
				return false;
			}

			public bool Intersects(Vector2 point, float radius)
			{
				Bounds2D bounds = new Bounds2D(point.x, point.y, radius * 2f, radius * 2f);
				if (!Intersects(bounds))
				{
					return false;
				}
				Vector2 vector = new Vector2(Mathf.Clamp(point.x, minX, maxX), Mathf.Clamp(point.y, minY, maxY));
				return (point - vector).sqrMagnitude <= radius * radius;
			}
		}

		private static MaterialPropertyBlock s_matPropBlock = new MaterialPropertyBlock();

		private static int s_propID_localPosMatrix = Shader.PropertyToID("_LocalPosMatrix");

		private RenderMode renderMode;

		private float cullPadding;

		private int level;

		private Neighbors neighbors;

		private Subpatches subpatches;

		private Bounds2D localBounds;

		private static Vector3[] s_patchVertices = new Vector3[4];

		public Patch()
		{
			neighbors.Init();
			subpatches.Init();
			Init();
		}

		public void Init(RenderMode mode = RenderMode.Spherical, float padding = 0f)
		{
			renderMode = mode;
			cullPadding = padding;
			level = 0;
			neighbors.Clear();
			subpatches.Clear();
			localBounds.center.Set(0f, 0f);
			localBounds.extents.Set(1f, 1f);
		}

		public void SetLocalBounds(Vector2 center, Vector2 size)
		{
			localBounds.Set(center, size);
		}

		public void Clear()
		{
			if (subpatches.created)
			{
				for (int i = 0; i < 4; i++)
				{
					subpatches[i].Clear();
				}
				subpatches.Clear();
			}
			PatchPool.ReturnPatch(this);
		}

		public void Tessellate(int maxLevel, Vector3 facePosition, float faceRadius)
		{
			if (level != maxLevel && localBounds.Intersects(new Vector2(facePosition.x, facePosition.z), faceRadius))
			{
				if (!subpatches.created)
				{
					Split();
				}
				for (int i = 0; i < 4; i++)
				{
					subpatches[i].Tessellate(maxLevel, facePosition, faceRadius * 0.5f);
				}
			}
		}

		public void Tessellate(int maxLevel, Vector3 facePosition, Vector3 faceBoundsSize)
		{
			if (level != maxLevel && localBounds.Intersects(new Bounds2D(facePosition.x, facePosition.z, faceBoundsSize.x * 2f, faceBoundsSize.z * 2f)))
			{
				if (!subpatches.created)
				{
					Split();
				}
				for (int i = 0; i < 4; i++)
				{
					subpatches[i].Tessellate(maxLevel, facePosition, faceBoundsSize * 0.5f);
				}
			}
		}

		private void Split()
		{
			for (int i = 0; i < 4; i++)
			{
				Edge edge = neighbors[i];
				if (edge.p0 != null && edge.p0.level < level)
				{
					edge.p0.Split();
				}
			}
			subpatches.created = true;
			for (int j = 0; j < 4; j++)
			{
				subpatches[j] = PatchPool.GetPatch(renderMode, cullPadding);
				subpatches[j].level = level + 1;
			}
			subpatches.upperLeft.SetNeighbors(neighbors.upper.p0, subpatches.upperRight, subpatches.lowerLeft, neighbors.left.p1);
			subpatches.upperRight.SetNeighbors(neighbors.upper.p1, neighbors.right.p0, subpatches.lowerRight, subpatches.upperLeft);
			subpatches.lowerRight.SetNeighbors(subpatches.upperRight, neighbors.right.p1, neighbors.lower.p0, subpatches.lowerLeft);
			subpatches.lowerLeft.SetNeighbors(subpatches.upperLeft, subpatches.lowerRight, neighbors.lower.p1, neighbors.left.p0);
			Vector2 vector = new Vector2(localBounds.extents.x * 0.5f, localBounds.extents.y * 0.5f);
			subpatches.upperLeft.localBounds.center = new Vector2(localBounds.center.x - vector.x, localBounds.center.y + vector.y);
			subpatches.upperRight.localBounds.center = new Vector2(localBounds.center.x + vector.x, localBounds.center.y + vector.y);
			subpatches.lowerRight.localBounds.center = new Vector2(localBounds.center.x + vector.x, localBounds.center.y - vector.y);
			subpatches.lowerLeft.localBounds.center = new Vector2(localBounds.center.x - vector.x, localBounds.center.y - vector.y);
			subpatches.upperLeft.localBounds.size = localBounds.extents;
			subpatches.upperRight.localBounds.size = localBounds.extents;
			subpatches.lowerRight.localBounds.size = localBounds.extents;
			subpatches.lowerLeft.localBounds.size = localBounds.extents;
			UpdateNeighborEdges(neighbors.upper, subpatches.upperLeft, subpatches.upperRight);
			UpdateNeighborEdges(neighbors.right, subpatches.upperRight, subpatches.lowerRight);
			UpdateNeighborEdges(neighbors.lower, subpatches.lowerRight, subpatches.lowerLeft);
			UpdateNeighborEdges(neighbors.left, subpatches.lowerLeft, subpatches.upperLeft);
		}

		public void SetNeighbors(Patch upper, Patch right, Patch lower, Patch left)
		{
			neighbors.SetNeighborEdge(0, upper);
			neighbors.SetNeighborEdge(1, right);
			neighbors.SetNeighborEdge(2, lower);
			neighbors.SetNeighborEdge(3, left);
		}

		public Patch GetNeighbor(int neighborIndex)
		{
			return neighbors[neighborIndex].p0;
		}

		public int GetLeafCount()
		{
			int num = 0;
			if (subpatches.created)
			{
				for (int i = 0; i < 4; i++)
				{
					num += subpatches[i].GetLeafCount();
				}
			}
			else
			{
				num++;
			}
			return num;
		}

		private void UpdateNeighborEdges(Edge edge, Patch p0, Patch p1)
		{
			if (edge.p0 == null)
			{
				return;
			}
			int matchingEdgeIndex = GetMatchingEdgeIndex(edge.p0.neighbors, this);
			if (matchingEdgeIndex != -1)
			{
				if (edge.isDualEdge)
				{
					edge.p0.neighbors.SetNeighborEdge(matchingEdgeIndex, p0);
					edge.p1.neighbors.SetNeighborEdge(matchingEdgeIndex, p1);
				}
				else
				{
					edge.p0.neighbors.SetNeighborEdge(matchingEdgeIndex, p1, p0);
				}
			}
		}

		private static int GetMatchingEdgeIndex(Neighbors neighbors, Patch connectedPatch)
		{
			if (neighbors.upper.IsConnectedTo(connectedPatch))
			{
				return 0;
			}
			if (neighbors.right.IsConnectedTo(connectedPatch))
			{
				return 1;
			}
			if (neighbors.lower.IsConnectedTo(connectedPatch))
			{
				return 2;
			}
			if (neighbors.left.IsConnectedTo(connectedPatch))
			{
				return 3;
			}
			return -1;
		}

		private bool Cull(UnityEngine.Plane[] frustumPlanes, Matrix4x4 matrix)
		{
			switch (renderMode)
			{
			case RenderMode.Planar:
				return CullPlanar(frustumPlanes, matrix);
			case RenderMode.Cubic:
			case RenderMode.Spherical:
				return CullSpherical(frustumPlanes, matrix);
			case RenderMode.Cylindrical:
				return CullCylindrical(frustumPlanes, matrix);
			default:
				return false;
			}
		}

		private bool CullPlanar(UnityEngine.Plane[] frustumPlanes, Matrix4x4 matrix)
		{
			Vector3 center3D = localBounds.center3D;
			Vector3 extents3D = localBounds.extents3D;
			extents3D.y += cullPadding;
			Vector3 vector = matrix.MultiplyPoint3x4(center3D);
			Vector3 b = vector + matrix.MultiplyVector(extents3D);
			float num = Vector3.Distance(vector, b);
			for (int i = 0; i < frustumPlanes.Length; i++)
			{
				if (Vector3.Dot(vector, frustumPlanes[i].normal) + frustumPlanes[i].distance < 0f - num)
				{
					return true;
				}
			}
			return false;
		}

		private bool CullSpherical(UnityEngine.Plane[] frustumPlanes, Matrix4x4 matrix)
		{
			Vector3 center3D = localBounds.center3D;
			center3D.y = 1f;
			Vector3 extents3D = localBounds.extents3D;
			s_patchVertices[0] = CubicToSpherical(new Vector3(center3D.x + extents3D.x, center3D.y, center3D.z + extents3D.z));
			s_patchVertices[1] = CubicToSpherical(new Vector3(center3D.x - extents3D.x, center3D.y, center3D.z - extents3D.z));
			s_patchVertices[2] = CubicToSpherical(new Vector3(center3D.x - extents3D.x, center3D.y, center3D.z + extents3D.z));
			s_patchVertices[3] = CubicToSpherical(new Vector3(center3D.x + extents3D.x, center3D.y, center3D.z - extents3D.z));
			Vector3 point = new Vector3((s_patchVertices[0].x + s_patchVertices[1].x + s_patchVertices[2].x + s_patchVertices[3].x) * 0.25f, (s_patchVertices[0].y + s_patchVertices[1].y + s_patchVertices[2].y + s_patchVertices[3].y) * 0.25f, (s_patchVertices[0].z + s_patchVertices[1].z + s_patchVertices[2].z + s_patchVertices[3].z) * 0.25f);
			int num = 0;
			float num2 = 0f;
			for (int i = 0; i < 4; i++)
			{
				Vector3 vector = new Vector3(s_patchVertices[i].x - point.x, s_patchVertices[i].y - point.y, s_patchVertices[i].z - point.z);
				float num3 = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
				if (num3 > num2)
				{
					num = i;
					num2 = num3;
				}
			}
			Vector3 vector2 = matrix.MultiplyPoint3x4(point);
			Vector3 b = matrix.MultiplyPoint3x4(s_patchVertices[num]);
			float num4 = Vector3.Distance(vector2, b);
			num4 += 10f;
			for (int j = 0; j < frustumPlanes.Length; j++)
			{
				if (Vector3.Dot(vector2, frustumPlanes[j].normal) + frustumPlanes[j].distance < 0f - num4)
				{
					return true;
				}
			}
			return false;
		}

		private bool CullCylindrical(UnityEngine.Plane[] frustumPlanes, Matrix4x4 matrix)
		{
			Vector3 center3D = localBounds.center3D;
			center3D.y = 1f;
			Vector3 extents3D = localBounds.extents3D;
			Vector3 vector = CubicToCylindrical(new Vector3(center3D.x + extents3D.x, center3D.y, center3D.z + extents3D.z));
			Vector3 vector2 = CubicToCylindrical(new Vector3(center3D.x - extents3D.x, center3D.y, center3D.z - extents3D.z));
			float num = 1f - cullPadding;
			Vector3 vector3 = new Vector3(vector2.x * num, vector2.y * num, vector2.z);
			Vector3 point = (vector + vector2) * 0.5f;
			Vector3 vector4 = new Vector3(vector2.x - point.x, vector2.y - point.y, vector2.z - point.z);
			Vector3 vector5 = new Vector3(vector3.x - point.x, vector3.y - point.y, vector3.z - point.z);
			float num2 = vector4.x * vector4.x + vector4.y * vector4.y + vector4.z * vector4.z;
			float num3 = vector5.x * vector5.x + vector5.y * vector5.y + vector5.z * vector5.z;
			Vector3 point2 = ((num2 > num3) ? vector2 : vector3);
			Vector3 vector6 = matrix.MultiplyPoint3x4(point);
			Vector3 b = matrix.MultiplyPoint3x4(point2);
			float num4 = Vector3.Distance(vector6, b);
			for (int i = 0; i < frustumPlanes.Length; i++)
			{
				if (Vector3.Dot(vector6, frustumPlanes[i].normal) + frustumPlanes[i].distance < 0f - num4)
				{
					return true;
				}
			}
			return false;
		}

		private MaterialPropertyBlock GetUpdatedProperties(MaterialPropertyBlock blockToUse = null)
		{
			Vector3 center3D = localBounds.center3D;
			if (renderMode == RenderMode.Cubic || renderMode == RenderMode.Spherical || renderMode == RenderMode.Cylindrical)
			{
				center3D.y = 1f;
			}
			Matrix4x4 value = Matrix4x4.TRS(center3D, Quaternion.identity, localBounds.extents3D);
			if (blockToUse != null)
			{
				blockToUse.SetMatrix(s_propID_localPosMatrix, value);
				return blockToUse;
			}
			s_matPropBlock.SetMatrix(s_propID_localPosMatrix, value);
			return s_matPropBlock;
		}

		private void Draw_Internal(Mesh mesh, Matrix4x4 matrix, Material material, Camera camera = null, int layer = 0, MaterialPropertyBlock properties = null, ShadowCastingMode shadowCastingMode = ShadowCastingMode.On)
		{
			Graphics.DrawMesh(mesh, matrix, material, layer, camera, 0, properties, shadowCastingMode);
		}

		private void Draw_Internal(Mesh mesh, Matrix4x4 matrix, Material[] materials, Camera camera = null, int layer = 0, MaterialPropertyBlock properties = null, ShadowCastingMode shadowCastingMode = ShadowCastingMode.On)
		{
			for (int i = 0; i < materials.Length; i++)
			{
				Graphics.DrawMesh(mesh, matrix, materials[i], layer, camera, 0, properties, shadowCastingMode);
			}
		}

		public void Draw(Mesh mesh, Matrix4x4 matrix, Material material, Camera camera = null, UnityEngine.Plane[] frustumPlanes = null, CullingMode cullingMode = CullingMode.Normal, int layer = 0, MaterialPropertyBlock properties = null)
		{
			bool flag = false;
			if (frustumPlanes != null && cullingMode != 0)
			{
				flag = Cull(frustumPlanes, matrix);
			}
			if (flag && cullingMode == CullingMode.Normal)
			{
				return;
			}
			if (subpatches.created)
			{
				for (int i = 0; i < 4; i++)
				{
					subpatches[i].Draw(mesh, matrix, material, camera, frustumPlanes, cullingMode, layer, properties);
				}
			}
			else if (flag && cullingMode == CullingMode.ShadowCasting)
			{
				Draw_Internal(mesh, matrix, material, camera, layer, GetUpdatedProperties(properties), ShadowCastingMode.ShadowsOnly);
			}
			else
			{
				Draw_Internal(mesh, matrix, material, camera, layer, GetUpdatedProperties(properties));
			}
		}

		public void Draw(Mesh mesh, Matrix4x4 matrix, Material[] materials, Camera camera = null, UnityEngine.Plane[] frustumPlanes = null, CullingMode cullingMode = CullingMode.Normal, int layer = 0, MaterialPropertyBlock properties = null)
		{
			bool flag = false;
			if (frustumPlanes != null && cullingMode != 0)
			{
				flag = Cull(frustumPlanes, matrix);
			}
			if (flag && cullingMode == CullingMode.Normal)
			{
				return;
			}
			if (subpatches.created)
			{
				for (int i = 0; i < 4; i++)
				{
					subpatches[i].Draw(mesh, matrix, materials, camera, frustumPlanes, cullingMode, layer, properties);
				}
			}
			else if (flag && cullingMode == CullingMode.ShadowCasting)
			{
				Draw_Internal(mesh, matrix, materials, camera, layer, GetUpdatedProperties(properties), ShadowCastingMode.ShadowsOnly);
			}
			else
			{
				Draw_Internal(mesh, matrix, materials, camera, layer, GetUpdatedProperties(properties));
			}
		}

		public void Draw(MeshGroup meshGroup, Matrix4x4 matrix, Material material, Camera camera = null, UnityEngine.Plane[] frustumPlanes = null, CullingMode cullingMode = CullingMode.Normal, int layer = 0, MaterialPropertyBlock properties = null)
		{
			bool flag = false;
			if (frustumPlanes != null && cullingMode != 0)
			{
				flag = Cull(frustumPlanes, matrix);
			}
			if (flag && cullingMode == CullingMode.Normal)
			{
				return;
			}
			if (subpatches.created)
			{
				for (int i = 0; i < 4; i++)
				{
					subpatches[i].Draw(meshGroup, matrix, material, camera, frustumPlanes, cullingMode, layer, properties);
				}
				return;
			}
			Mesh variant = meshGroup.GetVariant(neighbors.upper.isDualEdge, neighbors.right.isDualEdge, neighbors.lower.isDualEdge, neighbors.left.isDualEdge);
			if (flag && cullingMode == CullingMode.ShadowCasting)
			{
				Draw_Internal(variant, matrix, material, camera, layer, GetUpdatedProperties(properties), ShadowCastingMode.ShadowsOnly);
			}
			else
			{
				Draw_Internal(variant, matrix, material, camera, layer, GetUpdatedProperties(properties));
			}
		}

		public void Draw(MeshGroup meshGroup, Matrix4x4 matrix, Material[] materials, Camera camera = null, UnityEngine.Plane[] frustumPlanes = null, CullingMode cullingMode = CullingMode.Normal, int layer = 0, MaterialPropertyBlock properties = null)
		{
			bool flag = false;
			if (frustumPlanes != null && cullingMode != 0)
			{
				flag = Cull(frustumPlanes, matrix);
			}
			if (flag && cullingMode == CullingMode.Normal)
			{
				return;
			}
			if (subpatches.created)
			{
				for (int i = 0; i < 4; i++)
				{
					subpatches[i].Draw(meshGroup, matrix, materials, camera, frustumPlanes, cullingMode, layer, properties);
				}
				return;
			}
			Mesh variant = meshGroup.GetVariant(neighbors.upper.isDualEdge, neighbors.right.isDualEdge, neighbors.lower.isDualEdge, neighbors.left.isDualEdge);
			if (flag && cullingMode == CullingMode.ShadowCasting)
			{
				Draw_Internal(variant, matrix, materials, camera, layer, GetUpdatedProperties(properties), ShadowCastingMode.ShadowsOnly);
			}
			else
			{
				Draw_Internal(variant, matrix, materials, camera, layer, GetUpdatedProperties(properties));
			}
		}

		public void DrawGizmos()
		{
			if (subpatches.created)
			{
				for (int i = 0; i < 4; i++)
				{
					subpatches[i].DrawGizmos();
				}
				return;
			}
			Vector3 center3D = localBounds.center3D;
			if (renderMode == RenderMode.Cubic || renderMode == RenderMode.Spherical || renderMode == RenderMode.Cylindrical)
			{
				center3D.y = 1f;
			}
			Gizmos.color = new Color(level % 2, level / 2 % 2, level / 4 % 2);
			Gizmos.DrawCube(center3D, localBounds.size3D);
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(center3D, localBounds.size3D);
		}

		private Vector3 CubicToSpherical(Vector3 p)
		{
			Vector3 vector = new Vector3(p.x * p.x, p.y * p.y, p.z * p.z);
			Vector3 result = default(Vector3);
			result.x = p.x * Mathf.Sqrt(1f - vector.y * 0.5f - vector.z * 0.5f + vector.y * vector.z * (1f / 3f));
			result.y = p.y * Mathf.Sqrt(1f - vector.z * 0.5f - vector.x * 0.5f + vector.z * vector.x * (1f / 3f));
			result.z = p.z * Mathf.Sqrt(1f - vector.x * 0.5f - vector.y * 0.5f + vector.x * vector.y * (1f / 3f));
			return result;
		}

		private Vector3 CubicToCylindrical(Vector3 p)
		{
			float f = (0f - p.x) * ((float)Math.PI / 4f) + (float)Math.PI / 2f;
			return new Vector3(Mathf.Cos(f), Mathf.Sin(f), p.z);
		}
	}
}
