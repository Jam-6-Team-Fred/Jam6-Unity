using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NomaiTextProjector : DecalProjector
{
	[SerializeField]
	private TextAsset _nomaiMeshDataTextAsset;

	[SerializeField]
	private TextAsset _nomaiTextXMLAsset;

	private NomaiMeshData _nomaiMeshData;

	private int _subSegmentsX = 1;

	private int _subSegmentsZ = 1;

	private Texture2D _nomaiTexture;

	private float _xIncrement;

	private float _zIncrement;

	public TextAsset MyNomaiMeshDataAsset
	{
		get
		{
			return _nomaiMeshDataTextAsset;
		}
		set
		{
			_nomaiMeshDataTextAsset = value;
		}
	}

	public TextAsset MyNomaiTextAsset
	{
		get
		{
			return _nomaiTextXMLAsset;
		}
		set
		{
			_nomaiTextXMLAsset = value;
		}
	}

	public override Texture2D GetInspectorTexture()
	{
		return null;
	}

	private void Start()
	{
		_projectorBounds = default(Bounds);
		_pXYAtZMin = default(Plane);
		_pXYAtZMax = default(Plane);
		_pXZAtYMin = default(Plane);
		_pXZAtYMax = default(Plane);
		_pYZAtXMin = default(Plane);
		_pYZAtXMax = default(Plane);
		_planeDict = new Dictionary<Plane, Vector3>();
		Gizmos.color = Color.white;
		_meshTrianglePointList = new List<Vector3>();
		_trimmedTrianglePointList = new List<List<Vector3>>();
		_nomaiMeshData = new NomaiMeshData();
	}

	protected override void SetSizeParameters(int x = -1, int z = -1)
	{
		if (x == -1)
		{
			_xIncrement = base.transform.lossyScale.x;
		}
		else
		{
			_xIncrement = base.transform.lossyScale.x / (float)_subSegmentsX;
		}
		if (z == -1)
		{
			_zIncrement = base.transform.lossyScale.z;
		}
		else
		{
			_zIncrement = base.transform.lossyScale.z / (float)_subSegmentsZ;
		}
		_zeroPos = Vector3.zero - new Vector3(0.5f * base.transform.lossyScale.x, 0f, 0.5f * base.transform.lossyScale.z);
		if (z != -1 && x != -1)
		{
			_zeroPos += new Vector3(_xIncrement * (float)x, 0f, _zIncrement * (float)z);
		}
		_x1Pos = _zeroPos + new Vector3(_xIncrement, 0f, 0f);
		_y1Pos = _zeroPos - new Vector3(0f, base.transform.lossyScale.y, 0f);
		_z1Pos = _zeroPos + new Vector3(0f, 0f, _zIncrement);
		_xyPos = _x1Pos - new Vector3(0f, base.transform.lossyScale.y, 0f);
		_yzPos = _y1Pos + new Vector3(0f, 0f, _zIncrement);
		_xzPos = _z1Pos + new Vector3(_xIncrement, 0f, 0f);
		_xyzPos = _xzPos - new Vector3(0f, base.transform.lossyScale.y, 0f);
		Matrix4x4 matrix4x = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
		_zeroPos = matrix4x.MultiplyPoint3x4(_zeroPos);
		_x1Pos = matrix4x.MultiplyPoint3x4(_x1Pos);
		_y1Pos = matrix4x.MultiplyPoint3x4(_y1Pos);
		_z1Pos = matrix4x.MultiplyPoint3x4(_z1Pos);
		_xyPos = matrix4x.MultiplyPoint3x4(_xyPos);
		_yzPos = matrix4x.MultiplyPoint3x4(_yzPos);
		_xzPos = matrix4x.MultiplyPoint3x4(_xzPos);
		_xyzPos = matrix4x.MultiplyPoint3x4(_xyzPos);
		_listCubeVectors = new Vector3[8] { _zeroPos, _x1Pos, _y1Pos, _z1Pos, _xyPos, _xzPos, _yzPos, _xyzPos };
		float x2 = _zeroPos.x;
		float x3 = _zeroPos.x;
		float y = _zeroPos.y;
		float y2 = _zeroPos.y;
		float z2 = _zeroPos.z;
		float z3 = _zeroPos.z;
		Vector3[] listCubeVectors = _listCubeVectors;
		for (int i = 0; i < listCubeVectors.Length; i++)
		{
			Vector3 vector = listCubeVectors[i];
			if (vector.x > x3)
			{
				x3 = vector.x;
			}
			if (vector.x < x2)
			{
				x2 = vector.x;
			}
			if (vector.y > y2)
			{
				y2 = vector.y;
			}
			if (vector.y < y)
			{
				y = vector.y;
			}
			if (vector.z > z3)
			{
				z3 = vector.z;
			}
			if (vector.z < z2)
			{
				z2 = vector.z;
			}
		}
		_pXYAtZMin.Set3Points(_zeroPos, _x1Pos, _y1Pos);
		_pXYAtZMax.Set3Points(_yzPos, _xzPos, _z1Pos);
		_pXZAtYMin.Set3Points(_z1Pos, _x1Pos, _zeroPos);
		_pXZAtYMax.Set3Points(_y1Pos, _xyPos, _yzPos);
		_pYZAtXMin.Set3Points(_zeroPos, _y1Pos, _z1Pos);
		_pYZAtXMax.Set3Points(_xzPos, _xyPos, _x1Pos);
		if (_planeDict == null)
		{
			_planeDict = new Dictionary<Plane, Vector3>();
		}
		if (_planeDict.Count == 0)
		{
			_planeDict.Add(_pXYAtZMin, _zeroPos);
			_planeDict.Add(_pXYAtZMax, _xyzPos);
			_planeDict.Add(_pXZAtYMin, _zeroPos);
			_planeDict.Add(_pXZAtYMax, _xyzPos);
			_planeDict.Add(_pYZAtXMin, _zeroPos);
			_planeDict.Add(_pYZAtXMax, _xyzPos);
		}
		else if (_planeDict.Count == 6)
		{
			_planeDict.Clear();
			_planeDict.Add(_pXYAtZMin, _zeroPos);
			_planeDict.Add(_pXYAtZMax, _xyzPos);
			_planeDict.Add(_pXZAtYMin, _zeroPos);
			_planeDict.Add(_pXZAtYMax, _xyzPos);
			_planeDict.Add(_pYZAtXMin, _zeroPos);
			_planeDict.Add(_pYZAtXMax, _xyzPos);
		}
		_minExtent = new Vector3(x2, y, z2);
		_maxExtent = new Vector3(x3, y2, z3);
		_projectorBounds.SetMinMax(_minExtent, _maxExtent);
	}

	public MeshFilter[] BuildProjectedMeshes(int indexX = -1, int indexZ = -1)
	{
		List<MeshFilter> list = new List<MeshFilter>();
		MeshFilter[] componentsInChildren = base.transform.root.GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter meshFilter in componentsInChildren)
		{
			if (!(meshFilter == null) && !meshFilter.transform.IsChildOf(base.transform))
			{
				Matrix4x4 sourceMeshRotationMatrix = Matrix4x4.TRS(meshFilter.transform.position, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
				Mesh mesh = BuildProjectionMesh(meshFilter);
				if (mesh != null)
				{
					GameObject gameObject = ((indexX != -1 && indexZ != -1) ? new GameObject(meshFilter.transform.name + "_Decal" + indexX + indexZ) : new GameObject(meshFilter.transform.name + "_Decal"));
					gameObject.transform.SetParent(meshFilter.transform);
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
					gameObject.transform.localScale = Vector3.one;
					MeshFilter f = gameObject.gameObject.AddComponent<MeshFilter>();
					SetMeshFilter(ref f, mesh, sourceMeshRotationMatrix, indexX, indexZ);
					list.Add(f);
				}
			}
		}
		return list.ToArray();
	}

	protected void SetMeshFilter(ref MeshFilter f, Mesh m, Matrix4x4 sourceMeshRotationMatrix, int indexX, int indexZ)
	{
		Vector2[] array = new Vector2[m.vertices.Length];
		Vector2[] array2 = new Vector2[m.vertices.Length];
		Vector2[] array3 = new Vector2[m.vertices.Length];
		Glyph glyph = _nomaiMeshData.MeshGlyphs[indexX, indexZ];
		int num = _nomaiMeshData.NomaiTextIDs[indexX, _nomaiMeshData.MeshNCol - 1 - indexZ];
		Glyph.GlyphRotation glyphRotation = Glyph.GlyphRotation.NO_ROTATION;
		Vector2 center = Vector2.zero;
		Vector2 vector = ((num != -1) ? new Vector2((float)num * 0.1f, 0f) : Vector2.zero);
		Vector2 vector2;
		Vector2 vector3;
		if (glyph != null)
		{
			vector2 = _nomaiMeshData.MeshGlyphs[indexX, indexZ].MinUV1Coord;
			vector3 = _nomaiMeshData.MeshGlyphs[indexX, indexZ].MaxUV1Coord;
			glyphRotation = _nomaiMeshData.MeshGlyphs[indexX, indexZ].rotation;
			center = new Vector2(vector2.x + 0.5f * (vector3.x - vector2.x), vector2.y + 0.5f * (vector3.y - vector2.y));
		}
		else
		{
			vector2 = Vector2.zero;
			vector3 = Vector2.zero;
		}
		Vector3 a = base.transform.InverseTransformPoint(_zeroPos);
		Vector3 a2 = base.transform.InverseTransformPoint(_xyzPos);
		a = Vector3.Scale(a, base.transform.lossyScale);
		a2 = Vector3.Scale(a2, base.transform.lossyScale);
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 point = m.vertices[i];
			Vector3 position = sourceMeshRotationMatrix.MultiplyPoint3x4(point);
			Vector3 a3 = base.transform.InverseTransformPoint(position);
			a3 = Vector3.Scale(a3, base.transform.lossyScale);
			Vector2 vector4 = new Vector2(Mathf.InverseLerp(a.x, a2.x, a3.x), Mathf.InverseLerp(a.z, a2.z, a3.z));
			float x = Mathf.Lerp(vector2.x, vector3.x, vector4.x);
			float y = Mathf.Lerp(vector2.y, vector3.y, vector4.y);
			Vector2 vector5 = new Vector2(x, y);
			switch (glyphRotation)
			{
			case Glyph.GlyphRotation.ROT_CW_90:
				vector5 = RotatePoint2D(center, vector5, 90f);
				break;
			case Glyph.GlyphRotation.ROT_180:
				vector5 = RotatePoint2D(center, vector5, 180f);
				break;
			case Glyph.GlyphRotation.ROT_CW_270:
				vector5 = RotatePoint2D(center, vector5, 270f);
				break;
			}
			Vector2 vector6 = new Vector2((vector4.x + (float)indexX) / (float)_subSegmentsX, (vector4.y + (float)indexZ) / (float)_subSegmentsZ);
			array[i] = vector5;
			array2[i] = vector;
			array3[i] = vector6;
		}
		f.sharedMesh = m;
		f.sharedMesh.uv = array;
		f.sharedMesh.uv2 = array2;
		f.sharedMesh.uv3 = array3;
	}

	private Vector2 RotatePoint2D(Vector2 center, Vector2 point, float degrees)
	{
		Vector2 vector = point - center;
		float num = Mathf.Sin(degrees * ((float)Math.PI / 180f));
		float num2 = Mathf.Cos(degrees * ((float)Math.PI / 180f));
		vector = new Vector2(vector.x * num2 - vector.y * num, vector.x * num + vector.y * num2);
		return vector + center;
	}
}
