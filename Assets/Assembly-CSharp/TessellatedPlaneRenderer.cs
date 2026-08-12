using Tessellation;
using UnityEngine;

[ExecuteInEditMode]
public class TessellatedPlaneRenderer : TessellatedRenderer
{
	[Space]
	[SerializeField]
	private int _tileCountX = 1;

	[SerializeField]
	private int _tileCountY = 1;

	[SerializeField]
	private float _thickness;

	private Tessellation.Plane _tessellatedPlane;

	public int tileCountX
	{
		get
		{
			return _tileCountX;
		}
		set
		{
			_tileCountX = Mathf.Max(value, 1);
		}
	}

	public int tileCountY
	{
		get
		{
			return _tileCountY;
		}
		set
		{
			_tileCountY = Mathf.Max(value, 1);
		}
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		if (_tileCountX < 1)
		{
			_tileCountX = 1;
		}
		if (_tileCountY < 1)
		{
			_tileCountY = 1;
		}
		if (_thickness < 0f)
		{
			_thickness = 0f;
		}
		if (_tessellatedPlane != null)
		{
			if (_tessellatedPlane.GetBaseTileCountX() != _tileCountX || _tessellatedPlane.GetBaseTileCountY() != _tileCountY)
			{
				_tessellatedPlane.SetBaseTileCount(_tileCountX, _tileCountY);
			}
			float num = _thickness / base.transform.lossyScale.y;
			if (!Mathf.Approximately(_tessellatedPlane.GetThickness(), num))
			{
				_tessellatedPlane.SetThickness(num);
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_tessellatedPlane == null)
		{
			_tessellatedPlane = new Tessellation.Plane(_tileCountX, _tileCountY, _thickness / base.transform.lossyScale.y);
		}
	}

	protected override void ReconfigureMeshBounds()
	{
		for (int i = 0; i < _tessellationMeshGroup.variants.Length; i++)
		{
			_tessellationMeshGroup.variants[i].bounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(2f, 0.5f, 2f));
		}
	}

	protected override void Rebuild(OWCamera owCamera)
	{
		if (!ShouldRenderInCamera(owCamera) || _tessellatedPlane == null || _tessellationMeshGroup == null)
		{
			return;
		}
		Vector3 localPos = base.transform.InverseTransformPoint(owCamera.transform.position);
		float num = Mathf.Abs(localPos.y);
		int num2 = 0;
		float num3 = 1 << _LODBias;
		for (int i = 0; i < _maxLOD; i++)
		{
			if (num > num3)
			{
				break;
			}
			num2++;
			num3 *= 0.5f;
		}
		Vector3 lossyScale = base.transform.lossyScale;
		Vector3 localRadius = new Vector3(_LODRadius, _LODRadius, _LODRadius);
		if (lossyScale.x > lossyScale.z)
		{
			localRadius.x *= lossyScale.z / lossyScale.x;
		}
		else if (lossyScale.z > lossyScale.x)
		{
			localRadius.z *= lossyScale.x / lossyScale.z;
		}
		_tessellatedPlane.Init();
		_tessellatedPlane.Tessellate(num2, localPos, localRadius);
		if (owCamera.useFarCamera && owCamera.farCamera != null)
		{
			Camera[] cameras = new Camera[2] { owCamera.mainCamera, owCamera.farCamera };
			_tessellatedPlane.Draw(_tessellationMeshGroup, base.transform.localToWorldMatrix, _materials, cameras, _cullingMode, base.gameObject.layer);
		}
		else
		{
			_tessellatedPlane.Draw(_tessellationMeshGroup, base.transform.localToWorldMatrix, _materials, owCamera.mainCamera, _cullingMode, base.gameObject.layer);
		}
	}

	protected override void Clear(OWCamera owCamera)
	{
		if (_tessellatedPlane != null)
		{
			_tessellatedPlane.Clear();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.clear;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawCube(Vector3.zero, new Vector3(2f, 0f, 2f));
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = (OWGizmos.IsDirectlySelected(base.gameObject) ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.25f));
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(2f, 0f, 2f));
		if (_thickness > 0f)
		{
			Color color = Gizmos.color;
			Gizmos.color = new Color(color.r, color.g, color.b, color.a * 0.125f);
			float num = _thickness / base.transform.lossyScale.y * 0.5f;
			Gizmos.DrawWireCube(new Vector3(0f, num, 0f), new Vector3(2f, 0f, 2f));
			Gizmos.DrawWireCube(new Vector3(0f, 0f - num, 0f), new Vector3(2f, 0f, 2f));
		}
	}
}
