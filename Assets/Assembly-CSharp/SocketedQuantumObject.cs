using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SocketedQuantumObject : QuantumObject
{
	[SerializeField]
	private Vector3 _localOffset = Vector3.zero;

	[SerializeField]
	private float _newlyObscuredSocketProbability = 1f;

	[FormerlySerializedAs("_quantumSockets")]
	[SerializeField]
	private QuantumSocket[] _sockets;

	[FormerlySerializedAs("_quantumSocketRoot")]
	[SerializeField]
	private GameObject _socketRoot;

	[SerializeField]
	private bool _alignWithGravity = true;

	[SerializeField]
	private bool _alignWithSocket;

	[FormerlySerializedAs("_rotateOnCollapse")]
	[SerializeField]
	private bool _randomYRotation = true;

	protected QuantumSocket _occupiedSocket;

	protected QuantumSocket _recentlyObscuredSocket;

	[SerializeField]
	[HideInInspector]
	private bool _prebuilt;

	[SerializeField]
	[HideInInspector]
	private List<QuantumSocket> _socketList;

	[SerializeField]
	[HideInInspector]
	private List<QuantumSocket> _childSockets;

	protected bool _socketCollapseDirty;

	private GravityVolume _gravityVolume;

	private int _failedNewlyObscuredSocketRolls;

	private const int CHECK_DEPTH = 20;

	private void OnValidate()
	{
		if (_alignWithSocket && _alignWithGravity)
		{
			_alignWithSocket = (_alignWithGravity = false);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (!_prebuilt)
		{
			FindSocketsInHierarchy();
		}
	}

	protected override void Start()
	{
		if (_newlyObscuredSocketProbability > 0f)
		{
			for (int i = 0; i < _socketList.Count; i++)
			{
				if (_socketList[i].GetVisibilityObject() != null)
				{
					_socketList[i].OnNewlyObscured += OnSocketObscured;
				}
			}
		}
		if (_alignWithGravity)
		{
			_gravityVolume = GetComponentInParent<OWRigidbody>().GetAttachedGravityVolume();
		}
		base.Start();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		for (int i = 0; i < _socketList.Count; i++)
		{
			_socketList[i].OnNewlyObscured -= OnSocketObscured;
		}
	}

	private void FindSocketsInHierarchy()
	{
		_childSockets = new List<QuantumSocket>(GetComponentsInChildren<QuantumSocket>());
		_socketList = new List<QuantumSocket>();
		for (int i = 0; i < _sockets.Length; i++)
		{
			if (_sockets[i] == null)
			{
				Debug.LogWarning("Socket null or missing!", this);
				continue;
			}
			for (int j = 0; j < _sockets[i].GetProbabilityMultiplier(); j++)
			{
				if (!_childSockets.Contains(_sockets[i]))
				{
					_socketList.Add(_sockets[i]);
				}
			}
		}
		if (!(_socketRoot != null))
		{
			return;
		}
		QuantumSocket[] componentsInChildren = _socketRoot.GetComponentsInChildren<QuantumSocket>();
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			for (int l = 0; l < componentsInChildren[k].GetProbabilityMultiplier(); l++)
			{
				if (!_childSockets.Contains(componentsInChildren[k]))
				{
					_socketList.Add(componentsInChildren[k]);
				}
			}
		}
	}

	public QuantumSocket GetCurrentSocket()
	{
		return _occupiedSocket;
	}

	public void SetQuantumSockets(QuantumSocket[] sockets)
	{
		_sockets = sockets;
		_socketList.Clear();
		for (int i = 0; i < _sockets.Length; i++)
		{
			_socketList.Add(_sockets[i]);
		}
	}

	protected override bool CheckIllumination()
	{
		if (!(_occupiedSocket != null) || !(_occupiedSocket.GetVisibilityObject() != null))
		{
			return base.CheckIllumination();
		}
		return _occupiedSocket.GetVisibilityObject().IsIlluminated();
	}

	protected override void Update()
	{
		base.Update();
		_socketCollapseDirty = false;
	}

	protected override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
	{
		for (int i = 0; i < _childSockets.Count; i++)
		{
			if (_childSockets[i].IsOccupied())
			{
				return false;
			}
		}
		if (_socketList.Count <= 1)
		{
			Debug.LogError("Not enough quantum sockets in list!", this);
			Debug.Break();
			return false;
		}
		List<QuantumSocket> list = new List<QuantumSocket>();
		for (int j = 0; j < _socketList.Count; j++)
		{
			if (!_socketList[j].IsOccupied() && _socketList[j].IsActive())
			{
				list.Add(_socketList[j]);
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		if (_recentlyObscuredSocket != null)
		{
			MoveToSocket(_recentlyObscuredSocket);
			_recentlyObscuredSocket = null;
			return true;
		}
		QuantumSocket occupiedSocket = _occupiedSocket;
		for (int k = 0; k < 20; k++)
		{
			int index = Random.Range(0, list.Count);
			MoveToSocket(list[index]);
			if (skipInstantVisibilityCheck)
			{
				return true;
			}
			bool flag = false;
			if (!((!IsPlayerEntangled()) ? (CheckIllumination() ? CheckVisibilityInstantly() : CheckPointInside(Locator.GetPlayerCamera().transform.position)) : CheckIllumination()))
			{
				return true;
			}
			list.RemoveAt(index);
			if (list.Count == 0)
			{
				break;
			}
		}
		MoveToSocket(occupiedSocket);
		return false;
	}

	protected void MoveToSocket(QuantumSocket socket)
	{
		if (_occupiedSocket != null)
		{
			_occupiedSocket.ReleaseQuantumObject();
		}
		if (!(socket != null))
		{
			return;
		}
		_occupiedSocket = socket;
		_occupiedSocket.SetQuantumObject(this);
		base.transform.position = socket.transform.TransformPoint(_localOffset);
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
		if (_alignWithGravity)
		{
			if (_gravityVolume != null)
			{
				Vector3 toDirection = base.transform.position - _gravityVolume.GetCenterPosition();
				Quaternion quaternion = Quaternion.FromToRotation(base.transform.up, toDirection);
				base.transform.rotation = quaternion * base.transform.rotation;
			}
			else
			{
				Debug.LogError("Can't align to gravity. Gravity volume is NULL!", this);
			}
		}
		else if (_alignWithSocket)
		{
			base.transform.rotation = _occupiedSocket.transform.rotation;
		}
		if (_randomYRotation)
		{
			float num = Random.Range(0f, 360f);
			base.transform.localRotation *= Quaternion.Euler(Vector3.up * num);
		}
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
	}

	protected virtual void OnSocketObscured(QuantumSocket socket)
	{
		if (_socketCollapseDirty || socket.IsOccupied() || (!socket.GetVisibilityObject().IsIlluminated() && socket.GetVisibilityObject().CheckPointInside(Locator.GetPlayerCamera().transform.position)))
		{
			return;
		}
		float num = Vector3.Distance(socket.transform.position, Locator.GetPlayerTransform().position);
		float num2 = Vector3.Distance(base.transform.position, Locator.GetPlayerTransform().position);
		if (num < 50f && num < num2)
		{
			if (Random.value <= _newlyObscuredSocketProbability || _failedNewlyObscuredSocketRolls > 4)
			{
				_failedNewlyObscuredSocketRolls = 0;
				_recentlyObscuredSocket = socket;
				_socketCollapseDirty = AttemptCollapse();
				_recentlyObscuredSocket = null;
			}
			else
			{
				_failedNewlyObscuredSocketRolls++;
			}
		}
	}
}
