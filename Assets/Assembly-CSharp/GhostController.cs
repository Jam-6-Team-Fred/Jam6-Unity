using System.Collections.Generic;
using GhostEnums;
using UnityEngine;

public class GhostController : MonoBehaviour
{
	private enum FacingState
	{
		None = 0,
		FaceDirection = 1,
		FaceVelocity = 2,
		FacePosition = 3,
		FaceTransform = 4,
		FaceNode = 5,
		FaceNodeList = 6,
		Spin = 7
	}

	[SerializeField]
	private GhostNodeMap _nodeMap;

	[SerializeField]
	private CapsuleCollider _ghostCollider;

	[SerializeField]
	private DreamLanternController _lantern;

	[SerializeField]
	private GhostGrabController _grabController;

	[SerializeField]
	private Transform _lanternCarrySocket;

	private GhostEffects _effects;

	private Transform _nodeRoot;

	private GhostNode.NodeLayer _nodeLayer;

	private CapsuleCollider _playerCollider;

	private FacingState _facingState;

	private float _angularVelocity;

	private float _angularAcceleration;

	private float _targetDegreesPerSecond;

	private int _faceNodeListIndex = -1;

	private float _faceNodeListDelay;

	private float _faceNodeTimer;

	private bool _faceNodeListAutoFocus;

	private Vector3 _localFacePosition;

	private Transform _faceTransform;

	private Vector3 _localFaceDirection;

	private GhostNode _faceNode;

	private List<GhostNode> _faceNodeList = new List<GhostNode>(16);

	private Vector3 _velocity = Vector3.zero;

	private float _acceleration;

	private bool _movementPaused;

	private bool _moveToTargetPosition;

	private Vector3 _localTargetPosition;

	private float _targetSpeed;

	private bool _followNodePath;

	private int _pathIndex = -1;

	private List<GhostNode> _nodePath;

	private GhostEdge _startEdge;

	private bool _hasFinalPathPosition;

	private Vector3 _finalPathPosition;

	private bool _updateLantern;

	private float _targetLanternFocus;

	private float _lanternFocusRate;

	private bool _lerpingLanternToSocket;

	private float _lanternSocketLerpStartTime;

	private float _lanternSocketLerpLength;

	private Vector3 _lanternSocketLerpStartPosition;

	private Quaternion _lanternSocketLerpStartRotation;

	private Vector3 _localPointOnEdge;

	public OWEvent OnArriveAtPosition = new OWEvent(16);

	public OWEvent<GhostNode> OnTraversePathNode = new OWEvent<GhostNode>(16);

	public OWEvent<GhostNode> OnFaceNode = new OWEvent<GhostNode>(16);

	public OWEvent OnFinishFaceNodeList = new OWEvent(16);

	public OWEvent OnNodeMapChanged = new OWEvent(4);

	public void Initialize(GhostNode.NodeLayer layer, GhostEffects effects)
	{
		_effects = effects;
		Transform nodeRoot = (base.transform.parent = _nodeMap.transform);
		_nodeRoot = nodeRoot;
		_nodeLayer = layer;
		_grabController.Initialize(effects);
		_lantern.SetLit(lit: true);
		MoveLanternToCarrySocket();
		_playerCollider = Locator.GetPlayerBody().GetComponent<CapsuleCollider>();
	}

	public void SetNodeMap(GhostNodeMap nodeMap)
	{
		if (!(_nodeMap == nodeMap))
		{
			StopMoving();
			StopFacing();
			_nodeMap = nodeMap;
			Transform nodeRoot = (base.transform.parent = nodeMap.transform);
			_nodeRoot = nodeRoot;
			OnNodeMapChanged.Invoke();
		}
	}

	public Transform GetNodeRoot()
	{
		return _nodeRoot;
	}

	public DreamLanternController GetDreamLanternController()
	{
		return _lantern;
	}

	public GhostGrabController GetGrabController()
	{
		return _grabController;
	}

	public CapsuleCollider GetCollider()
	{
		return _ghostCollider;
	}

	public GhostNodeMap GetNodeMap()
	{
		return _nodeMap;
	}

	public GhostNode.NodeLayer GetNodeLayer()
	{
		return _nodeLayer;
	}

	public void SetNodeLayer(GhostNode.NodeLayer layer)
	{
		_nodeLayer = layer;
	}

	public Vector3 GetLocalForward()
	{
		return WorldToLocalDirection(base.transform.forward);
	}

	public Vector3 GetLocalFeetPosition()
	{
		return base.transform.localPosition;
	}

	public float GetSpeed()
	{
		return _velocity.magnitude;
	}

	public Vector3 GetRelativeVelocity()
	{
		return base.transform.InverseTransformDirection(LocalToWorldDirection(_velocity));
	}

	public float GetAngularVelocity()
	{
		return _angularVelocity;
	}

	public Vector3 WorldToLocalPosition(Vector3 worldPos)
	{
		return _nodeRoot.InverseTransformPoint(worldPos);
	}

	public Vector3 LocalToWorldPosition(Vector3 localPos)
	{
		return _nodeRoot.TransformPoint(localPos);
	}

	public Vector3 WorldToLocalDirection(Vector3 worldDir)
	{
		return _nodeRoot.InverseTransformDirection(worldDir);
	}

	public Vector3 LocalToWorldDirection(Vector3 localDir)
	{
		return _nodeRoot.TransformDirection(localDir);
	}

	public float GetAngleToLocalPosition(Vector3 localPos)
	{
		Vector3 from = localPos - GetLocalFeetPosition();
		from.y = 0f;
		return Vector3.Angle(from, GetLocalForward());
	}

	public void ExtinguishLantern()
	{
		_lantern.SetLit(lit: false);
	}

	public bool IsLanternFocused()
	{
		return _lantern.IsFocused();
	}

	public bool IsLanternLightVisible()
	{
		if (_lantern.IsLit())
		{
			return !_lantern.IsConcealed();
		}
		return false;
	}

	public float GetUnfocusedLanternRange()
	{
		return _lantern.GetMinRange();
	}

	public float GetFocusedLanternRange()
	{
		return _lantern.GetMaxRange();
	}

	public void MoveLanternToCarrySocket(bool lerpTransform = false, float lerpLength = 0.1f)
	{
		MoveLanternToSocket(_lanternCarrySocket, lerpTransform, lerpLength);
	}

	public void MoveLanternToSocket(Transform lanternSocket, bool lerpTransform = false, float lerpLength = 0.1f)
	{
		_lantern.transform.parent = lanternSocket;
		if (lerpTransform)
		{
			_lerpingLanternToSocket = true;
			_lanternSocketLerpStartTime = Time.timeSinceLevelLoad;
			_lanternSocketLerpLength = lerpLength;
			_lanternSocketLerpStartPosition = _lantern.transform.localPosition;
			_lanternSocketLerpStartRotation = _lantern.transform.localRotation;
		}
		else
		{
			_lerpingLanternToSocket = false;
			_lantern.transform.localPosition = Vector3.zero;
			_lantern.transform.localRotation = Quaternion.identity;
		}
		_lantern.transform.localScale = Vector3.one;
	}

	public void SetLanternConcealed(bool concealed, bool playAudio = true)
	{
		if (playAudio && _lantern.IsConcealed() != concealed)
		{
			_effects.PlayLanternAudio(concealed ? AudioType.Artifact_Conceal : AudioType.Artifact_Unconceal);
		}
		_lantern.SetConcealed(concealed);
		if (concealed)
		{
			_lantern.SetFocus(0f);
			_updateLantern = false;
		}
	}

	public void ChangeLanternFocus(float focus, float focusRate = 2f)
	{
		if (focus > 0f)
		{
			_lantern.SetConcealed(concealed: false);
		}
		if (focus > _targetLanternFocus)
		{
			_effects.PlayLanternAudio(AudioType.Artifact_Focus);
		}
		else if (focus < _targetLanternFocus)
		{
			_effects.PlayLanternAudio(AudioType.Artifact_Unfocus);
		}
		_updateLantern = true;
		_targetLanternFocus = focus;
		_lanternFocusRate = focusRate;
	}

	public void Update_Controller()
	{
		if (_updateLantern)
		{
			_lantern.MoveTowardFocus(_targetLanternFocus, _lanternFocusRate);
			if (OWMath.ApproxEquals(_lantern.GetFocus(), _targetLanternFocus))
			{
				_lantern.SetFocus(_targetLanternFocus);
				_updateLantern = false;
			}
		}
		if (_lerpingLanternToSocket)
		{
			float t = Mathf.InverseLerp(_lanternSocketLerpStartTime, _lanternSocketLerpStartTime + _lanternSocketLerpLength, Time.timeSinceLevelLoad);
			t = Mathf.SmoothStep(0f, 1f, t);
			_lantern.transform.localPosition = Vector3.Lerp(_lanternSocketLerpStartPosition, Vector3.zero, t);
			_lantern.transform.localRotation = Quaternion.Lerp(_lanternSocketLerpStartRotation, Quaternion.identity, t);
			if (t >= 1f)
			{
				_lerpingLanternToSocket = false;
			}
		}
	}

	public void StopFacing()
	{
		_facingState = FacingState.None;
	}

	public void FacePlayer(TurnSpeed turnSpeed)
	{
		_facingState = FacingState.FaceTransform;
		_faceTransform = Locator.GetPlayerCamera().transform;
		_targetDegreesPerSecond = GhostConstants.GetTurnSpeed(turnSpeed);
		_angularAcceleration = GhostConstants.GetTurnAcceleration(turnSpeed);
	}

	public void FaceVelocity()
	{
		_facingState = FacingState.FaceVelocity;
		TurnSpeed turnSpeed = TurnSpeed.MEDIUM;
		_targetDegreesPerSecond = GhostConstants.GetTurnSpeed(turnSpeed);
		_angularAcceleration = GhostConstants.GetTurnAcceleration(turnSpeed);
	}

	public void FaceLocalDirection(Vector3 localDirection, TurnSpeed turnSpeed)
	{
		FaceLocalDirection(localDirection, GhostConstants.GetTurnSpeed(turnSpeed), GhostConstants.GetTurnAcceleration(turnSpeed));
	}

	public void FaceLocalDirection(Vector3 localDirection, float degreesPerSecond, float turnAcceleration = 360f)
	{
		_facingState = FacingState.FaceDirection;
		_localFaceDirection = localDirection;
		_targetDegreesPerSecond = degreesPerSecond;
		_angularAcceleration = turnAcceleration;
	}

	public void FaceLocalPosition(Vector3 localPosition, TurnSpeed turnSpeed)
	{
		FaceLocalPosition(localPosition, GhostConstants.GetTurnSpeed(turnSpeed), GhostConstants.GetTurnAcceleration(turnSpeed));
	}

	public void FaceLocalPosition(Vector3 localPosition, float degreesPerSecond, float turnAcceleration = 360f)
	{
		_facingState = FacingState.FacePosition;
		_localFacePosition = localPosition;
		_targetDegreesPerSecond = degreesPerSecond;
		_angularAcceleration = turnAcceleration;
	}

	public void FaceNode(GhostNode node, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern = false)
	{
		_facingState = FacingState.FaceNode;
		_faceNode = node;
		_faceNodeTimer = nodeDelay;
		_targetDegreesPerSecond = GhostConstants.GetTurnSpeed(turnSpeed);
		_angularAcceleration = GhostConstants.GetTurnAcceleration(turnSpeed);
		if (autoFocusLantern)
		{
			Vector3 vector = node.localPosition - GetLocalFeetPosition();
			float num = GetUnfocusedLanternRange() - 1f;
			bool flag = vector.sqrMagnitude > num * num;
			ChangeLanternFocus(flag ? 1f : 0f);
		}
	}

	public void FaceNodeList(GhostNode[] nodeList, int numNodes, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern = false)
	{
		if (numNodes == 0)
		{
			OnFinishFaceNodeList.Invoke();
		}
		_facingState = FacingState.FaceNodeList;
		_faceNodeList.Clear();
		for (int i = 0; i < numNodes; i++)
		{
			_faceNodeList.Add(nodeList[i]);
		}
		_targetDegreesPerSecond = GhostConstants.GetTurnSpeed(turnSpeed);
		_angularAcceleration = GhostConstants.GetTurnAcceleration(turnSpeed);
		_faceNodeListDelay = nodeDelay;
		_faceNodeTimer = _faceNodeListDelay;
		_faceNodeListIndex = 0;
		_faceNodeListAutoFocus = autoFocusLantern;
	}

	public void Spin(TurnSpeed turnSpeed)
	{
		_facingState = FacingState.Spin;
		_targetDegreesPerSecond = GhostConstants.GetTurnSpeed(turnSpeed);
		_angularAcceleration = GhostConstants.GetTurnAcceleration(turnSpeed);
	}

	public void PathfindToLocalPosition(Vector3 localPosition, MoveType moveType)
	{
		PathfindToLocalPosition(localPosition, GhostConstants.GetMoveSpeed(moveType), GhostConstants.GetMoveAcceleration(moveType));
	}

	public void PathfindToLocalPosition(Vector3 localPosition, float speed, float acceleration = 10f)
	{
		StopMoving();
		Vector3 closestPointOnEdge;
		GhostEdge ghostEdge = _nodeMap.FindClosestEdge(base.transform.localPosition, out closestPointOnEdge);
		Vector3 closestPointOnEdge2;
		GhostEdge ghostEdge2 = _nodeMap.FindClosestEdge(localPosition, out closestPointOnEdge2);
		if (ghostEdge.Equals(ghostEdge2))
		{
			MoveToLocalPosition(localPosition, speed, acceleration);
			return;
		}
		if (ghostEdge.ContainsNode(ghostEdge2.nodeOne) && Vector3.Distance(localPosition, ghostEdge2.nodeOne.localPosition) < 0.5f)
		{
			MoveToLocalPosition(localPosition, speed, acceleration);
			return;
		}
		if (ghostEdge.ContainsNode(ghostEdge2.nodeTwo) && Vector3.Distance(localPosition, ghostEdge2.nodeTwo.localPosition) < 0.5f)
		{
			MoveToLocalPosition(localPosition, speed, acceleration);
			return;
		}
		_nodePath = _nodeMap.FindPath(base.transform.localPosition, ghostEdge, localPosition, ghostEdge2);
		if (_nodePath == null)
		{
			return;
		}
		_followNodePath = true;
		_pathIndex = _nodePath.Count - 1;
		_moveToTargetPosition = true;
		_localTargetPosition = _nodePath[_pathIndex].localPosition;
		_targetSpeed = speed;
		_acceleration = acceleration;
		_hasFinalPathPosition = true;
		_finalPathPosition = localPosition;
		_startEdge = ghostEdge;
		_localPointOnEdge = closestPointOnEdge;
		if (Vector3.Distance(_nodePath[_pathIndex].localPosition, base.transform.localPosition) < 0.5f)
		{
			if (_nodePath.Count > 1)
			{
				_pathIndex--;
				_localTargetPosition = _nodePath[_pathIndex].localPosition;
			}
			else
			{
				_localTargetPosition = _finalPathPosition;
			}
		}
	}

	public void PathfindToNode(GhostNode node, MoveType moveType)
	{
		PathfindToNode(node, GhostConstants.GetMoveSpeed(moveType), GhostConstants.GetMoveAcceleration(moveType));
	}

	public void PathfindToNode(GhostNode node, float speed, float acceleration = 10f)
	{
		StopMoving();
		Vector3 closestPointOnEdge;
		GhostEdge startEdge = _nodeMap.FindClosestEdge(base.transform.localPosition, out closestPointOnEdge);
		_nodePath = _nodeMap.FindPath(base.transform.localPosition, startEdge, node);
		if (_nodePath != null)
		{
			_followNodePath = true;
			_pathIndex = _nodePath.Count - 1;
			if (_nodePath.Count > 1 && Vector3.Distance(_nodePath[_pathIndex].localPosition, base.transform.localPosition) < 0.5f)
			{
				_pathIndex--;
			}
			_moveToTargetPosition = true;
			_localTargetPosition = _nodePath[_pathIndex].localPosition;
			_targetSpeed = speed;
			_acceleration = acceleration;
			_startEdge = startEdge;
			_localPointOnEdge = closestPointOnEdge;
		}
	}

	public void MoveToLocalPosition(Vector3 localPosition, MoveType moveType)
	{
		MoveToLocalPosition(localPosition, GhostConstants.GetMoveSpeed(moveType), GhostConstants.GetMoveAcceleration(moveType));
	}

	public void MoveToLocalPosition(Vector3 localPosition, float speed, float acceleration = 10f)
	{
		StopMoving();
		_moveToTargetPosition = true;
		_localTargetPosition = localPosition;
		_targetSpeed = speed;
		_acceleration = acceleration;
	}

	public void StopMoving()
	{
		_moveToTargetPosition = false;
		_followNodePath = false;
		_hasFinalPathPosition = false;
	}

	public void StopMovingInstantly()
	{
		_velocity = Vector3.zero;
		StopMoving();
	}

	public bool IsMoving()
	{
		return _moveToTargetPosition;
	}

	public Vector3 GetLocalTargetPosition()
	{
		if (_moveToTargetPosition)
		{
			return _localTargetPosition;
		}
		return base.transform.localPosition;
	}

	public bool IsApproachingEndOfIncline()
	{
		if (_moveToTargetPosition && _followNodePath && _pathIndex > 0 && (_localTargetPosition - base.transform.localPosition).sqrMagnitude < 1f && (_nodePath[_pathIndex - 1].localPosition - _localTargetPosition).normalized.y < 0.258819f)
		{
			return true;
		}
		return false;
	}

	public void SetMovementPaused(bool paused)
	{
		_movementPaused = paused;
	}

	private void HandleArriveAtTargetPosition()
	{
		bool flag = true;
		if (_followNodePath)
		{
			int pathIndex = _pathIndex;
			_pathIndex--;
			if (_pathIndex >= 0)
			{
				flag = false;
				_localTargetPosition = _nodePath[_pathIndex].localPosition;
			}
			else
			{
				_followNodePath = false;
				if (_hasFinalPathPosition)
				{
					flag = false;
					_hasFinalPathPosition = false;
					_localTargetPosition = _finalPathPosition;
				}
			}
			if (!flag)
			{
				OnTraversePathNode.Invoke(_nodePath[pathIndex]);
			}
		}
		if (flag)
		{
			_moveToTargetPosition = false;
			OnArriveAtPosition.Invoke();
		}
	}

	public void FixedUpdate_Controller()
	{
		if (_moveToTargetPosition)
		{
			Vector3 vector = _localTargetPosition - base.transform.localPosition;
			Vector3 normalized = vector.normalized;
			float magnitude = vector.magnitude;
			bool flag = !_followNodePath || (_pathIndex == 0 && !_hasFinalPathPosition);
			float magnitude2 = _velocity.magnitude;
			float num = magnitude2 * magnitude2 / (2f * _acceleration);
			if (_movementPaused)
			{
				_velocity = Vector3.MoveTowards(_velocity, Vector3.zero, _acceleration * Time.fixedDeltaTime);
				UpdatePositionFromVelocity();
			}
			else if (flag && magnitude <= num)
			{
				_velocity = Vector3.MoveTowards(_velocity, Vector3.zero, _acceleration * Time.fixedDeltaTime);
				UpdatePositionFromVelocity();
				if (_velocity.magnitude <= 0.001f)
				{
					HandleArriveAtTargetPosition();
				}
			}
			else
			{
				Vector3 target = normalized * _targetSpeed;
				_velocity = Vector3.MoveTowards(_velocity, target, _acceleration * Time.fixedDeltaTime);
				UpdatePositionFromVelocity();
				float num2 = (flag ? 0.5f : num);
				if (Vector3.Distance(base.transform.localPosition, _localTargetPosition) < num2)
				{
					HandleArriveAtTargetPosition();
				}
			}
		}
		else
		{
			_velocity = Vector3.MoveTowards(_velocity, Vector3.zero, _acceleration * Time.fixedDeltaTime);
			UpdatePositionFromVelocity();
		}
		Vector3 localDirection = GetLocalForward();
		switch (_facingState)
		{
		case FacingState.FaceDirection:
			localDirection = _localFaceDirection;
			break;
		case FacingState.FaceVelocity:
			if (_velocity.x * _velocity.x + _velocity.z * _velocity.z > 0.0001f)
			{
				localDirection = _velocity;
			}
			break;
		case FacingState.FacePosition:
			localDirection = _localFacePosition - _nodeRoot.InverseTransformPoint(base.transform.position);
			break;
		case FacingState.FaceTransform:
			localDirection = WorldToLocalPosition(_faceTransform.position) - _nodeRoot.InverseTransformPoint(base.transform.position);
			break;
		case FacingState.FaceNode:
		{
			Vector3 localPosition2 = _faceNode.localPosition;
			localDirection = localPosition2 - GetLocalFeetPosition();
			if (GetAngleToLocalPosition(localPosition2) < 1f)
			{
				_faceNodeTimer -= Time.deltaTime;
				if (_faceNodeTimer <= 0f)
				{
					_facingState = FacingState.None;
					GhostNode faceNode = _faceNode;
					_faceNode = null;
					OnFaceNode.Invoke(faceNode);
				}
			}
			break;
		}
		case FacingState.FaceNodeList:
		{
			Vector3 localPosition = _faceNodeList[_faceNodeListIndex].localPosition;
			Vector3 vector2 = localPosition - GetLocalFeetPosition();
			if (GetAngleToLocalPosition(localPosition) > 1f)
			{
				localDirection = vector2;
				break;
			}
			_faceNodeTimer -= Time.deltaTime;
			if (!(_faceNodeTimer <= 0f))
			{
				break;
			}
			OnFaceNode.Invoke(_faceNodeList[_faceNodeListIndex]);
			_faceNodeListIndex++;
			if (_faceNodeListIndex >= _faceNodeList.Count)
			{
				_facingState = FacingState.None;
				_faceNodeListIndex = -1;
				_faceNodeList.Clear();
				OnFinishFaceNodeList.Invoke();
				break;
			}
			if (_faceNodeListAutoFocus)
			{
				float num3 = GetUnfocusedLanternRange() - 1f;
				bool flag2 = vector2.sqrMagnitude > num3 * num3;
				ChangeLanternFocus(flag2 ? 1f : 0f);
			}
			_faceNodeTimer = _faceNodeListDelay;
			break;
		}
		case FacingState.Spin:
			localDirection = WorldToLocalDirection(base.transform.right);
			break;
		}
		TurnTowardsLocalDirection(localDirection, _targetDegreesPerSecond);
	}

	private void UpdatePositionFromVelocity()
	{
		Vector3 vector = base.transform.localPosition + _velocity * Time.fixedDeltaTime;
		if (_ghostCollider != null && _ghostCollider.enabled && _playerCollider != null && !_grabController.enabled)
		{
			Vector3 positionB = WorldToLocalPosition(_playerCollider.transform.position);
			Quaternion rotationB = _nodeRoot.InverseTransformRotation(_playerCollider.transform.rotation);
			if (Physics.ComputePenetration(_ghostCollider, vector, base.transform.localRotation, _playerCollider, positionB, rotationB, out var direction, out var distance))
			{
				vector += direction * distance;
			}
		}
		base.transform.localPosition = vector;
	}

	private void TurnTowardsLocalDirection(Vector3 localDirection, float targetDegreesPerSecond)
	{
		Vector3 from = localDirection;
		from.y = 0f;
		float x = Vector3.Angle(from, localDirection) * (0f - Mathf.Sign(localDirection.y));
		_lantern.transform.localEulerAngles = new Vector3(x, 0f, 0f);
		localDirection.y = 0f;
		float num = OWMath.Angle(WorldToLocalDirection(base.transform.forward), localDirection, Vector3.up);
		float num2 = Mathf.Sign(num);
		float num3 = _angularVelocity * _angularVelocity / (2f * _angularAcceleration);
		float target = targetDegreesPerSecond * num2;
		if ((num2 > 0f && num <= num3) || (num2 < 0f && num >= 0f - num3))
		{
			target = 0f;
		}
		_angularVelocity = Mathf.MoveTowards(_angularVelocity, target, _angularAcceleration * Time.fixedDeltaTime);
		float num4 = _angularVelocity * Time.fixedDeltaTime;
		if ((num2 > 0f && num4 > num) || (num2 < 0f && num4 < num))
		{
			num4 = num;
		}
		Quaternion localRotation = Quaternion.AngleAxis(num4, Vector3.up) * base.transform.localRotation;
		base.transform.localRotation = localRotation;
	}

	private void TurnTowardsLocalDirection_Old(Vector3 localDirection, float targetDegreesPerSecond)
	{
		Vector3 from = localDirection;
		from.y = 0f;
		float x = Vector3.Angle(from, localDirection) * (0f - Mathf.Sign(localDirection.y));
		_lantern.transform.localEulerAngles = new Vector3(x, 0f, 0f);
		localDirection.y = 0f;
		Quaternion to = Quaternion.AngleAxis(OWMath.Angle(WorldToLocalDirection(base.transform.forward), localDirection, Vector3.up), Vector3.up) * base.transform.localRotation;
		base.transform.localRotation = Quaternion.RotateTowards(base.transform.localRotation, to, Time.deltaTime * targetDegreesPerSecond);
	}

	private void OnDrawGizmosSelected()
	{
		if (_followNodePath)
		{
			for (int i = 0; i < _nodePath.Count; i++)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere(LocalToWorldPosition(_nodePath[i].localPosition), 0.6f);
			}
			if (_startEdge != null)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireCube(LocalToWorldPosition(_startEdge.nodeOne.localPosition), Vector3.one);
				Gizmos.DrawWireCube(LocalToWorldPosition(_startEdge.nodeTwo.localPosition), Vector3.one);
			}
			if (_hasFinalPathPosition)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(LocalToWorldPosition(_finalPathPosition), 0.6f);
			}
		}
		if (_nodeRoot != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(_nodeRoot.TransformPoint(_localPointOnEdge), 0.5f);
			Vector3 vector = base.transform.position + base.transform.up;
			Gizmos.DrawLine(vector, vector + _nodeRoot.TransformDirection(_velocity));
		}
	}
}
