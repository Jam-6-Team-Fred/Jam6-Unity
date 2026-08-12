using UnityEngine;

public class GhostIKTest : MonoBehaviour
{
	[SerializeField]
	private OWIK _leftFootIK;

	[SerializeField]
	private OWIK _rightFootIK;

	[SerializeField]
	private Vector3 _feetPlacementOffset;

	[SerializeField]
	[Range(0f, 0.99f)]
	private float _footIKDampening;

	[SerializeField]
	private float _maxFootVerticalDistance;

	[SerializeField]
	private float strideLength;

	[SerializeField]
	private float _strideArcHeight;

	[SerializeField]
	private bool _adjustRootY;

	[SerializeField]
	private Transform _worldRef;

	[SerializeField]
	private Transform _torsoGoalPivot;

	private Animator _animator;

	private float _lastIKLeftFootWeight;

	private float _lastIKRightFootWeight;

	private Vector3 _currentLeftFootTarget;

	private Vector3 _currentRightFootTarget;

	private Vector3 _currentLeftFootNormal;

	private Vector3 _currentRightFootNormal;

	private Vector3 _nextLeftStride;

	private Vector3 _nextRightStride;

	private Transform _leftToe;

	private Transform _rightToe;

	private Transform _leftFoot;

	private Transform _rightFoot;

	private float _prevLeftFootGroundness;

	private float _prevRightFootGroundness;

	private float _defaultLeftFootHeight;

	private float _defaultRightFootHeight;

	private bool _leftFootStride;

	private bool _rightFootStride;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void Start()
	{
		_leftToe = _animator.GetBoneTransform(HumanBodyBones.LeftToes);
		_rightToe = _animator.GetBoneTransform(HumanBodyBones.RightToes);
		_leftFoot = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
		_rightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
		_defaultLeftFootHeight = base.transform.InverseTransformPoint(_leftToe.position).y;
		_defaultRightFootHeight = base.transform.InverseTransformPoint(_rightToe.position).y;
		_prevLeftFootGroundness = 1f;
		_prevRightFootGroundness = 1f;
		_currentLeftFootTarget = _leftFoot.position;
		_currentRightFootTarget = _rightFoot.position;
	}

	private void LateUpdate()
	{
		float num = Mathf.Clamp01(1f - (base.transform.InverseTransformPoint(_leftToe.position).y - _defaultLeftFootHeight));
		float num2 = Mathf.Clamp01(1f - (base.transform.InverseTransformPoint(_rightToe.position).y - _defaultRightFootHeight));
		_leftFootStride = num < 1f;
		_rightFootStride = num2 < 1f;
		Vector3 position = _leftToe.position;
		Vector3 position2 = _leftToe.position;
		if (Physics.Raycast(_leftToe.position + base.transform.up * 1.5f, -base.transform.up * 3.5f, out var hitInfo, 2f))
		{
			position = hitInfo.point + base.transform.TransformVector(_feetPlacementOffset);
			if (num >= 1f && _prevLeftFootGroundness < 1f)
			{
				_currentLeftFootTarget = _worldRef.InverseTransformPoint(position);
				_currentLeftFootNormal = _worldRef.InverseTransformDirection(hitInfo.normal);
			}
			MeasureStrides(0);
		}
		position2 = _worldRef.TransformPoint(_currentLeftFootTarget);
		if (_leftFootStride)
		{
			position2 = ArcPoint(_leftToe.position, position2, _worldRef.TransformPoint(_nextLeftStride), out var _);
		}
		Vector3 position3 = _rightToe.position;
		Vector3 position4 = _rightToe.position;
		if (Physics.Raycast(_rightToe.position + base.transform.up * 1.5f, -base.transform.up * 3.5f, out var hitInfo2, 2f))
		{
			position3 = hitInfo2.point + base.transform.TransformVector(_feetPlacementOffset);
			if (num2 >= 1f && _prevRightFootGroundness < 1f)
			{
				_currentRightFootTarget = _worldRef.InverseTransformPoint(position3);
				_currentRightFootNormal = _worldRef.InverseTransformDirection(hitInfo2.normal);
			}
			MeasureStrides(1);
		}
		position4 = _worldRef.TransformPoint(_currentRightFootTarget);
		if (_rightFootStride)
		{
			position4 = ArcPoint(_rightToe.position, position4, _worldRef.TransformPoint(_nextRightStride), out var _);
		}
		if (_adjustRootY)
		{
			base.transform.localPosition = Vector3.zero;
			Transform boneTransform = _animator.GetBoneTransform(HumanBodyBones.Hips);
			Vector3 vector = base.transform.InverseTransformPoint(position2);
			Vector3 vector2 = base.transform.InverseTransformPoint(position4);
			Vector3 vector3 = base.transform.InverseTransformPoint(boneTransform.position);
			Vector3 obj = ((vector.y < vector2.y) ? vector : vector2);
			Vector3 position5 = base.transform.position;
			float num3 = Mathf.Clamp(Mathf.Abs(obj.y - vector3.y) - _maxFootVerticalDistance, 0f, float.PositiveInfinity);
			position5 += base.transform.TransformVector(new Vector3(0f, 0f - num3, 0f));
			if (Vector3.Distance(base.transform.position, position5) <= 0.5f)
			{
				base.transform.position = position5;
			}
		}
		_prevLeftFootGroundness = num;
		_prevRightFootGroundness = num2;
		_leftFootIK.SetGoalPosition(position2);
		_leftFootIK.Solve();
		_rightFootIK.SetGoalPosition(position4);
		_rightFootIK.Solve();
		if (_torsoGoalPivot != null)
		{
			_torsoGoalPivot.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Locator.GetPlayerBody().transform.position - _torsoGoalPivot.position, _torsoGoalPivot.up), _torsoGoalPivot.up);
		}
	}

	private void OnAnimatorIK(int layerIndex)
	{
		_animator.SetLookAtPosition(Locator.GetPlayerBody().transform.position);
		_animator.SetLookAtWeight(1f);
	}

	private Vector3 ArcPoint(Vector3 footPoint, Vector3 point1, Vector3 point2, out float progress)
	{
		float sqrMagnitude = (point2 - point1).sqrMagnitude;
		Vector3 vector = Vector3.Project(footPoint - point1, point2 - point1);
		progress = vector.sqrMagnitude / sqrMagnitude;
		float num = 0f - Mathf.Pow(2f * progress - 1f, 2f) + 1f;
		return point1 + vector + _worldRef.up * num * _strideArcHeight;
	}

	private void MeasureStride(int footIdx)
	{
	}

	private void MeasureStrides(int footIdx)
	{
		float @float = _animator.GetFloat("Speed");
		Vector3 obj = ((footIdx == 0) ? _worldRef.TransformPoint(_currentLeftFootTarget) : _worldRef.TransformPoint(_currentRightFootTarget));
		Vector3 vector = -Vector3.Cross((footIdx == 0) ? _worldRef.TransformDirection(_currentLeftFootNormal) : _worldRef.TransformDirection(_currentRightFootNormal), base.transform.right);
		if (Physics.Raycast(obj + base.transform.up * 1.5f + vector * strideLength * @float, -base.transform.up * 3.5f, out var hitInfo, 2f))
		{
			if (footIdx <= 0)
			{
				_nextLeftStride = _worldRef.InverseTransformPoint(hitInfo.point) + base.transform.TransformVector(_feetPlacementOffset);
			}
			else
			{
				_nextRightStride = _worldRef.InverseTransformPoint(hitInfo.point) + base.transform.TransformVector(_feetPlacementOffset);
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (_animator != null)
		{
			Transform boneTransform = _animator.GetBoneTransform(HumanBodyBones.LeftToes);
			Transform boneTransform2 = _animator.GetBoneTransform(HumanBodyBones.RightToes);
			Transform boneTransform3 = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			Transform boneTransform4 = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(boneTransform.position, 0.05f);
			Gizmos.DrawSphere(boneTransform2.position, 0.05f);
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(boneTransform3.position, 0.05f);
			Gizmos.DrawSphere(boneTransform4.position, 0.05f);
			Transform boneTransform5 = _animator.GetBoneTransform(HumanBodyBones.Hips);
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(boneTransform5.position, 0.2f);
			Gizmos.color = (_leftFootStride ? Color.red : Color.green);
			Gizmos.DrawWireSphere(_worldRef.TransformPoint(_currentLeftFootTarget), 0.1f);
			Gizmos.DrawWireSphere(_worldRef.TransformPoint(_nextLeftStride), 0.1f);
			Gizmos.color = (_rightFootStride ? Color.red : Color.green);
			Gizmos.DrawWireSphere(_worldRef.TransformPoint(_currentRightFootTarget), 0.1f);
			Gizmos.DrawWireSphere(_worldRef.TransformPoint(_nextRightStride), 0.1f);
		}
	}
}
