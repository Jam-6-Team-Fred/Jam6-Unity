using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RendererVisibilityTracker : VisibilityTracker
{
	[SerializeField]
	private Collider[] _ignoreOcclusionColliders;

	[SerializeField]
	private bool _checkOcclusion;

	[SerializeField]
	private bool _checkFrustumOcclusion;

	private bool _zeroRotation = true;

	private bool _drawBounds = true;

	private bool _visible;

	private bool _visibleToProbe;

	private Renderer _renderer;

	private Quaternion _worldRotation;

	private Transform _transform;

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
		_worldRotation = ((!_zeroRotation) ? base.transform.rotation : Quaternion.identity);
		_transform = base.transform;
	}

	public override bool IsVisibleUsingCameraFrustum()
	{
		if (GeometryUtility.TestPlanesAABB(Locator.GetActiveCamera().GetFrustumPlanes(), _renderer.bounds))
		{
			if (!_checkFrustumOcclusion)
			{
				return true;
			}
			return !IsOccludedFromPosition(Locator.GetActiveCamera().transform.position);
		}
		return false;
	}

	public override bool IsVisible()
	{
		return _visible;
	}

	public override bool IsVisibleToProbe(OWCamera camera)
	{
		return _visibleToProbe;
	}

	public override bool IsPointInside(Vector3 worldPos)
	{
		return false;
	}

	private void OnWillRenderObject()
	{
		if (Locator.GetActiveCamera().UsesCamera(Camera.current))
		{
			_visible = !_checkOcclusion || !IsOccludedFromPosition(Camera.current.transform.position);
		}
		else if (ProbeCamera.GetLastSnapshotCamera() != null && ProbeCamera.GetLastSnapshotCamera().UsesCamera(Camera.current))
		{
			_visibleToProbe = !_checkOcclusion || !IsOccludedFromPosition(Camera.current.transform.position);
		}
	}

	private bool IsOccludedFromPosition(Vector3 worldPos)
	{
		if (Physics.Linecast(worldPos, _transform.position, out var hitInfo, OWLayerMask.quantumOcclusionMask) && (_ignoreOcclusionColliders == null || Array.IndexOf(_ignoreOcclusionColliders, hitInfo.collider) == -1))
		{
			return true;
		}
		return false;
	}

	private void LateUpdate()
	{
		_visible = false;
		_visibleToProbe = false;
		_transform.rotation = _worldRotation;
	}

	private void OnDrawGizmosSelected()
	{
		if (_drawBounds && !_zeroRotation)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(GetComponent<Renderer>().bounds.center, GetComponent<Renderer>().bounds.size);
		}
	}
}
