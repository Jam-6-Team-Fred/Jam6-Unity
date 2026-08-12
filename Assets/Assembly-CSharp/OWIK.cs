using System;
using System.Collections.Generic;
using UnityEngine;

public class OWIK : MonoBehaviour
{
	public enum IKState
	{
		NotRun = 0,
		Success = 1,
		Fail = 2
	}

	public enum UpdateCycle
	{
		Update = 0,
		LateUpdate = 1,
		Manual = 2
	}

	[Serializable]
	public struct JointInfo
	{
		public Transform transform;

		public Transform hint;

		public bool useHint;

		public Constraint constraint;

		public bool isFixed;

		public bool updatePosition;

		public bool variableLength;

		public Vector2 variableLengthRange;

		public Vector3 manualRotationOffset;
	}

	[Serializable]
	public struct Constraint
	{
		public float xMidAngle;

		public float xAngle;

		public float yMidAngle;

		public float yAngle;

		public Vector2 this[int i]
		{
			get
			{
				switch (i)
				{
				case 0:
					return new Vector2(xMidAngle, xAngle);
				case 1:
					return new Vector2(yMidAngle, yAngle);
				default:
					return Vector2.zero;
				}
			}
		}
	}

	public struct ConstraintAxis
	{
		public Vector3 xAxis;

		public Vector3 yAxis;

		public Vector3 zAxis;

		public Vector3 this[int i]
		{
			get
			{
				switch (i)
				{
				case 0:
					return xAxis;
				case 1:
					return yAxis;
				case 2:
					return zAxis;
				default:
					return Vector2.zero;
				}
			}
		}
	}

	[SerializeField]
	private JointInfo[] _joints;

	[SerializeField]
	private Transform _root;

	[SerializeField]
	private Transform _goal;

	[SerializeField]
	private bool _constrain;

	[SerializeField]
	private UpdateCycle _updateCycle;

	[SerializeField]
	private float _marginOfError;

	[SerializeField]
	private int _iterLimit;

	[SerializeField]
	private float _weight = 1f;

	[Header("Debug")]
	[SerializeField]
	private bool _showConstraints = true;

	private Vector3 _vectorGoalPos;

	private float[] _cachedLengths;

	private Vector3[] _runtimeJointPos;

	private ConstraintAxis[] _cachedConstraintCenters;

	private ConstraintAxis[] _cachedConstraintAxes;

	private Quaternion[] _cachedStartBoneRotations;

	private Quaternion[] _cachedStartEffectorRotations;

	private float _totalLength;

	private float[] _deltaAngles;

	private bool[] _constrained;

	private Vector3[] _jointDirs;

	private List<OWIK> _childSystems = new List<OWIK>(0);

	public UpdateCycle updateCycle
	{
		get
		{
			return _updateCycle;
		}
		set
		{
			_updateCycle = value;
			base.enabled = _updateCycle != UpdateCycle.Manual;
		}
	}

	public Vector3[] jointPositions => _runtimeJointPos;

	public void GetJointPositionsNonAlloc(ref Vector3[] jointsArray)
	{
		jointsArray = _runtimeJointPos;
	}

	public void SetGoalPosition(Vector3 goalPos)
	{
		_vectorGoalPos = goalPos;
	}

	public void SetWeight(float weight)
	{
		_weight = weight;
	}

	private void Start()
	{
		Initialize();
		OWIK componentInParent = base.transform.parent.GetComponentInParent<OWIK>();
		if (componentInParent != null)
		{
			componentInParent.AddChildSystem(this);
			updateCycle = UpdateCycle.Manual;
		}
	}

	private void Update()
	{
		if (_updateCycle == UpdateCycle.Update)
		{
			Solve();
		}
	}

	private void LateUpdate()
	{
		if (_updateCycle == UpdateCycle.LateUpdate)
		{
			Solve();
		}
	}

	private void Initialize()
	{
		_totalLength = 0f;
		_cachedLengths = new float[_joints.Length - 1];
		_cachedConstraintCenters = new ConstraintAxis[_joints.Length];
		_cachedConstraintAxes = new ConstraintAxis[_joints.Length];
		_cachedStartEffectorRotations = new Quaternion[_joints.Length - 1];
		_cachedStartBoneRotations = new Quaternion[_joints.Length - 1];
		_runtimeJointPos = new Vector3[_joints.Length];
		_deltaAngles = new float[_joints.Length];
		_constrained = new bool[_joints.Length];
		_jointDirs = new Vector3[_joints.Length];
		for (int i = 0; i < _joints.Length; i++)
		{
			if (i < _joints.Length - 1)
			{
				_cachedLengths[i] = Vector3.Distance(_joints[i].transform.position, _joints[i + 1].transform.position);
				_totalLength += _cachedLengths[i];
				Vector3 normalized = (_joints[i + 1].transform.position - _joints[i].transform.position).normalized;
				Quaternion q = Quaternion.LookRotation(normalized, Vector3.Cross(normalized, _root.right));
				_cachedStartBoneRotations[i] = _root.InverseTransformRotation(_joints[i].transform.rotation);
				_cachedStartEffectorRotations[i] = _root.InverseTransformRotation(q);
			}
			Vector3 vector = JointParent(i).InverseTransformDirection(_joints[i].transform.forward);
			Vector3 vector2 = JointParent(i).InverseTransformDirection(_joints[i].transform.right);
			Vector3 vector3 = JointParent(i).InverseTransformDirection(_joints[i].transform.up);
			vector = JointParent(i).InverseTransformDirection(GetPrimaryForwardAxis(i));
			vector2 = JointParent(i).InverseTransformDirection(GetPrimaryRotationAxis(i));
			vector3 = JointParent(i).InverseTransformDirection(Vector3.Cross(GetPrimaryRotationAxis(i), GetPrimaryForwardAxis(i)));
			Vector3 xAxis = Quaternion.AngleAxis(_joints[i].constraint.xMidAngle, vector2) * vector;
			Vector3 yAxis = Quaternion.AngleAxis(_joints[i].constraint.yMidAngle, vector3) * vector;
			Vector3 zAxis = vector;
			_cachedConstraintCenters[i] = new ConstraintAxis
			{
				xAxis = xAxis,
				yAxis = yAxis,
				zAxis = zAxis
			};
			_cachedConstraintAxes[i] = new ConstraintAxis
			{
				xAxis = vector2,
				yAxis = vector3,
				zAxis = vector
			};
		}
		base.enabled = _updateCycle != UpdateCycle.Manual;
	}

	public void Solve()
	{
		for (int i = 0; i < _joints.Length; i++)
		{
			_runtimeJointPos[i] = _joints[i].transform.position;
		}
		Vector3 origBasePos = _runtimeJointPos[0];
		ApplyHints();
		int num = 0;
		Vector3 b;
		do
		{
			BackwardChain();
			ForwardChain(origBasePos);
			num++;
			if (num >= _iterLimit)
			{
				break;
			}
			b = ((_goal == null) ? _vectorGoalPos : _goal.position);
		}
		while (!(Vector3.Distance(_runtimeJointPos[_runtimeJointPos.Length - 1], b) <= _marginOfError));
		for (int j = 0; j < _joints.Length; j++)
		{
			if (!_joints[j].isFixed)
			{
				if (_joints[j].updatePosition)
				{
					_joints[j].transform.position = _runtimeJointPos[j];
				}
				if (j < _joints.Length - 1)
				{
					Vector3 normalized = (_runtimeJointPos[j + 1] - _runtimeJointPos[j]).normalized;
					_jointDirs[j] = normalized;
					Quaternion q = Quaternion.LookRotation(normalized, Vector3.Cross(normalized, _root.right));
					Quaternion quaternion = _root.InverseTransformRotation(q) * Quaternion.Inverse(_cachedStartEffectorRotations[j]);
					Quaternion b2 = _root.rotation * Quaternion.Euler(_joints[j].manualRotationOffset) * (quaternion * _cachedStartBoneRotations[j]);
					_joints[j].transform.rotation = Quaternion.Slerp(_joints[j].transform.rotation, b2, _weight);
				}
			}
		}
		for (int k = 0; k < _childSystems.Count; k++)
		{
			_childSystems[k].Solve();
		}
	}

	private void ApplyHints()
	{
		for (int i = 1; i < _runtimeJointPos.Length; i++)
		{
			if (_joints[i - 1].useHint && !(_joints[i - 1].hint == null))
			{
				Vector3 normalized = (_joints[i - 1].hint.position - _runtimeJointPos[i - 1]).normalized;
				Vector3 vector = _runtimeJointPos[i - 1] + normalized * _cachedLengths[i - 1] - _runtimeJointPos[i];
				for (int j = i; j < _runtimeJointPos.Length; j++)
				{
					_runtimeJointPos[j] += vector;
				}
			}
		}
	}

	private void BackwardChain()
	{
		Vector3 vector = ((_goal == null) ? _vectorGoalPos : _goal.position);
		_runtimeJointPos[_runtimeJointPos.Length - 1] = vector;
		for (int num = _runtimeJointPos.Length - 2; num >= 0; num--)
		{
			Vector3 normalized = (_runtimeJointPos[num] - _runtimeJointPos[num + 1]).normalized;
			_runtimeJointPos[num] = _runtimeJointPos[num + 1] + normalized * _cachedLengths[num];
		}
	}

	private void ForwardChain(Vector3 origBasePos)
	{
		_runtimeJointPos[0] = origBasePos;
		for (int i = 0; i < _runtimeJointPos.Length - 1; i++)
		{
			if (!_joints[i].isFixed)
			{
				Vector3 vector = (_runtimeJointPos[i + 1] - _runtimeJointPos[i]).normalized;
				if (_constrain)
				{
					vector = Constrain(i, vector);
				}
				float num = _cachedLengths[i];
				if (_joints[i + 1].variableLength)
				{
					Vector3 vector2 = ((_goal == null) ? _vectorGoalPos : _goal.position);
					Vector3 b = ((i == _joints.Length - 2) ? vector2 : _runtimeJointPos[i + 1]);
					num = Mathf.Min(Vector3.Distance(_runtimeJointPos[i], b), num + _joints[i + 1].variableLengthRange.y);
					num = Mathf.Max(num, num - _joints[i + 1].variableLengthRange.x);
				}
				_runtimeJointPos[i + 1] = _runtimeJointPos[i] + vector * num;
			}
		}
	}

	private Vector3 Constrain(int i, Vector3 dir)
	{
		Constraint constraint = _joints[i].constraint;
		Vector3 vector = JointParent(i).InverseTransformDirection(dir);
		for (int j = 0; j <= 1; j++)
		{
			Vector2 vector2 = constraint[j];
			if (!(vector2.y >= 360f))
			{
				Vector3 to = Vector3.ProjectOnPlane(vector, _cachedConstraintAxes[i][j]);
				float num = OWMath.Angle(_cachedConstraintCenters[i][j], to, _cachedConstraintAxes[i][j]);
				float num2 = Mathf.Abs(num) - vector2.y / 2f;
				if (j == 0)
				{
					_deltaAngles[i] = num;
					_constrained[i] = num2 > 0f;
				}
				if (num2 > 0f)
				{
					vector = Quaternion.AngleAxis(num2 * (0f - Mathf.Sign(num)), _cachedConstraintAxes[i][j]) * vector;
				}
			}
		}
		dir = JointParent(i).TransformDirection(vector);
		return dir;
	}

	private Transform JointParent(int index)
	{
		if (index <= 0 || _joints[index - 1].transform == _joints[index].transform.parent)
		{
			return _joints[index].transform.parent;
		}
		return _joints[index - 1].transform;
	}

	private Vector3 GetPrimaryRotationAxis(int jointIndex)
	{
		Transform transform = _joints[jointIndex].transform;
		Vector3 result = transform.right;
		float num = float.MaxValue;
		float num2 = Mathf.Abs(Vector3.Dot(transform.right, _root.right));
		if (1f - num2 < num)
		{
			num = 1f - num2;
		}
		float num3 = Mathf.Abs(Vector3.Dot(transform.up, _root.right));
		if (1f - num3 < num)
		{
			result = transform.up;
			num = 1f - num3;
		}
		float num4 = Mathf.Abs(Vector3.Dot(transform.forward, _root.right));
		if (1f - num4 < num)
		{
			result = transform.forward;
		}
		return result;
	}

	private Vector3 GetPrimaryForwardAxis(int jointIndex)
	{
		if (jointIndex >= _joints.Length - 1)
		{
			return _joints[jointIndex].transform.forward;
		}
		Transform transform = _joints[jointIndex].transform;
		Vector3 normalized = (_joints[jointIndex + 1].transform.position - transform.position).normalized;
		Vector3 result = transform.right;
		float num = float.MaxValue;
		float num2 = Mathf.Abs(Vector3.Dot(transform.right, normalized));
		if (1f - num2 < num)
		{
			num = 1f - num2;
		}
		float num3 = Mathf.Abs(Vector3.Dot(transform.up, normalized));
		if (1f - num3 < num)
		{
			result = transform.up;
			num = 1f - num3;
		}
		float num4 = Mathf.Abs(Vector3.Dot(transform.forward, normalized));
		if (1f - num4 < num)
		{
			result = transform.forward;
		}
		return result;
	}

	public void AddChildSystem(OWIK childSystem)
	{
		_childSystems.Add(childSystem);
	}
}
