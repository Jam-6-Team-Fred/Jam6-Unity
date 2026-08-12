using System.Collections.Generic;
using UnityEngine;

public class VisibilityOccluder : MonoBehaviour
{
	private static List<VisibilityOccluder> s_theList = new List<VisibilityOccluder>(64);

	private static Vector3[] _corners = new Vector3[4];

	private static Plane[] _frustum = new Plane[5];

	private BoxShape[] _boxes;

	private SphereShape[] _spheres;

	private void Awake()
	{
		_boxes = GetComponentsInChildren<BoxShape>();
		_spheres = GetComponentsInChildren<SphereShape>();
		_corners = new Vector3[4];
		if (_boxes.Length == 0 && _spheres.Length == 0)
		{
			Debug.LogWarning("Visibility occluder could not find any child shapes.", this);
		}
	}

	private void OnEnable()
	{
		s_theList.Add(this);
	}

	private void OnDisable()
	{
		s_theList.Remove(this);
	}

	public static bool CanYouSee(ShapeVisibilityTracker tracker, Vector3 cameraPos)
	{
		for (int i = 0; i < s_theList.Count; i++)
		{
			VisibilityOccluder visibilityOccluder = s_theList[i];
			for (int j = 0; j < visibilityOccluder._boxes.Length; j++)
			{
				BoxShape boxShape = visibilityOccluder._boxes[j];
				_corners[0] = boxShape.transform.TransformPoint(new Vector3(-0.5f * boxShape.size.x, -0.5f * boxShape.size.y, 0f));
				_corners[1] = boxShape.transform.TransformPoint(new Vector3(0.5f * boxShape.size.x, -0.5f * boxShape.size.y, 0f));
				_corners[2] = boxShape.transform.TransformPoint(new Vector3(0.5f * boxShape.size.x, 0.5f * boxShape.size.y, 0f));
				_corners[3] = boxShape.transform.TransformPoint(new Vector3(-0.5f * boxShape.size.x, 0.5f * boxShape.size.y, 0f));
				_frustum[0].normal = Vector3.Cross(_corners[1] - _corners[0], _corners[2] - _corners[1]).normalized;
				if (Vector3.Dot(_frustum[0].normal, _corners[0] - cameraPos) < 0f)
				{
					_frustum[0].normal = -_frustum[0].normal;
					Vector3 vector = _corners[0];
					_corners[0] = _corners[1];
					_corners[1] = vector;
					vector = _corners[2];
					_corners[2] = _corners[3];
					_corners[3] = vector;
				}
				_frustum[0].distance = 0f - Vector3.Dot(_frustum[0].normal, _corners[0]);
				_frustum[1].normal = Vector3.Cross(_corners[1] - cameraPos, _corners[1] - _corners[0]).normalized;
				_frustum[1].distance = 0f - Vector3.Dot(_frustum[1].normal, _corners[0]);
				_frustum[2].normal = Vector3.Cross(_corners[2] - cameraPos, _corners[2] - _corners[1]).normalized;
				_frustum[2].distance = 0f - Vector3.Dot(_frustum[2].normal, _corners[1]);
				_frustum[3].normal = Vector3.Cross(_corners[3] - cameraPos, _corners[3] - _corners[2]).normalized;
				_frustum[3].distance = 0f - Vector3.Dot(_frustum[3].normal, _corners[2]);
				_frustum[4].normal = Vector3.Cross(_corners[0] - cameraPos, _corners[0] - _corners[3]).normalized;
				_frustum[4].distance = 0f - Vector3.Dot(_frustum[4].normal, _corners[3]);
				if (tracker.IsBlocked(_frustum))
				{
					return false;
				}
			}
			for (int k = 0; k < visibilityOccluder._spheres.Length; k++)
			{
				SphereShape sphereShape = visibilityOccluder._spheres[k];
				Vector3 centerLine = ShapeUtil.Sphere.CalcWorldSpaceCenter(sphereShape) - cameraPos;
				float num = ShapeUtil.Sphere.CalcWorldSpaceRadius(sphereShape);
				float magnitude = centerLine.magnitude;
				if (magnitude > 0.01f)
				{
					centerLine /= magnitude;
					float halfAngle = Mathf.Asin(num / magnitude);
					if (tracker.IsBlocked(cameraPos, centerLine, magnitude, halfAngle))
					{
						return false;
					}
				}
			}
		}
		return true;
	}
}
