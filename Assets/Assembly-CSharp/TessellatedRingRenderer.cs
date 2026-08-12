using Tessellation;
using UnityEngine;

[ExecuteInEditMode]
public class TessellatedRingRenderer : TessellatedRenderer
{
	[Space]
	[SerializeField]
	[Range(0f, 1f)]
	private float _thickness;

	private Ring _tessellatedRing;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_tessellatedRing == null)
		{
			_tessellatedRing = new Ring();
		}
	}

	protected override void ReconfigureMeshBounds()
	{
		for (int i = 0; i < _tessellationMeshGroup.variants.Length; i++)
		{
			_tessellationMeshGroup.variants[i].bounds = new Bounds(new Vector3(0f, 0.75f, 0f), new Vector3(2f, 0.5f, 2f));
		}
	}

	protected override void Rebuild(OWCamera owCamera)
	{
		if (!ShouldRenderInCamera(owCamera) || _tessellatedRing == null || _tessellationMeshGroup == null)
		{
			return;
		}
		Vector3 localPos = base.transform.InverseTransformPoint(owCamera.transform.position);
		float num = Mathf.Abs(new Vector2(localPos.x, localPos.z).magnitude - 1f);
		num = Mathf.Max(num - _thickness, 0f);
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
		_tessellatedRing.Init(_thickness);
		_tessellatedRing.Tessellate(num2, localPos, _LODRadius, _thickness);
		if (owCamera.useFarCamera && owCamera.farCamera != null)
		{
			Camera[] cameras = new Camera[2] { owCamera.mainCamera, owCamera.farCamera };
			_tessellatedRing.Draw(_tessellationMeshGroup, base.transform.localToWorldMatrix, _materials, cameras, _cullingMode, base.gameObject.layer);
		}
		else
		{
			_tessellatedRing.Draw(_tessellationMeshGroup, base.transform.localToWorldMatrix, _materials, owCamera.mainCamera, _cullingMode, base.gameObject.layer);
		}
	}

	protected override void Clear(OWCamera owCamera)
	{
		if (_tessellatedRing != null)
		{
			_tessellatedRing.Clear();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.clear;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		OWGizmos.DrawCylinder(Vector3.zero, Quaternion.identity, -2f, -1f);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = (OWGizmos.IsDirectlySelected(base.gameObject) ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.25f));
		Gizmos.matrix = base.transform.localToWorldMatrix;
		OWGizmos.DrawWireCylinder(Vector3.zero, Quaternion.AngleAxis(45f, Vector3.up), 2.002f, 1f);
		if (_thickness > 0f)
		{
			OWGizmos.DrawWireCylinder(Vector3.zero, Quaternion.AngleAxis(45f, Vector3.up), 2.002f, 1f - _thickness);
		}
	}
}
