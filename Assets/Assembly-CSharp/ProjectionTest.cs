using UnityEngine;

[ExecuteInEditMode]
public class ProjectionTest : MonoBehaviour
{
	[SerializeField]
	private float _near;

	[SerializeField]
	private float _right;

	[SerializeField]
	private float _left;

	[SerializeField]
	private float _far;

	[SerializeField]
	private float _top;

	[SerializeField]
	private float _bottom;

	[SerializeField]
	private bool _custom;

	[SerializeField]
	private bool _direct;

	[SerializeField]
	private Matrix4x4 _projectionMatrix;

	[SerializeField]
	private bool _customWorld;

	[SerializeField]
	private bool _directWorld;

	[SerializeField]
	private float _rotAngle;

	[SerializeField]
	private Matrix4x4 _worldCameraMatrix;

	private Matrix4x4 matrix
	{
		get
		{
			Matrix4x4 result = default(Matrix4x4);
			result.m00 = 2f * _near / (_right - _left);
			result.m01 = 0f;
			result.m02 = (_right + _left) / (_right - _left);
			result.m03 = 0f;
			result.m10 = 0f;
			result.m11 = 2f * _near / (_top - _bottom);
			result.m12 = (_top + _bottom) / (_top - _bottom);
			result.m13 = 0f;
			result.m20 = 0f;
			result.m21 = 0f;
			result.m22 = (0f - (_far + _near)) / (_far - _near);
			result.m23 = (0f - 2f * _far * _near) / (_far - _near);
			result.m30 = 0f;
			result.m31 = 0f;
			result.m32 = -1f;
			result.m33 = 0f;
			return result;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		Camera component = GetComponent<Camera>();
		if (!_custom)
		{
			component.ResetProjectionMatrix();
		}
		else
		{
			component.projectionMatrix = (_direct ? _projectionMatrix : matrix);
		}
		if (_customWorld)
		{
			component.worldToCameraMatrix = (_directWorld ? (_worldCameraMatrix * Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one)) : worldMat());
		}
		else
		{
			component.ResetWorldToCameraMatrix();
		}
	}

	private Matrix4x4 worldMat()
	{
		return Matrix4x4.Rotate(Quaternion.AngleAxis(_rotAngle, Vector3.up));
	}
}
