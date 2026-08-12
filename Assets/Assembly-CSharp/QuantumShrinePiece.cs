using UnityEngine;

public class QuantumShrinePiece : SocketedQuantumObject
{
	[SerializeField]
	private QuantumShrine _quantumShrine;

	[SerializeField]
	private QuantumSocket _shrineSocket;

	[SerializeField]
	private QuantumSocket _supportSocket;

	protected override void Awake()
	{
		base.Awake();
		_shrineSocket.OnNewlyObscured += OnSocketObscured;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_shrineSocket.OnNewlyObscured -= OnSocketObscured;
	}

	protected override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
	{
		if (_quantumShrine.IsPlayerInside())
		{
			return false;
		}
		return base.ChangeQuantumState(skipInstantVisibilityCheck);
	}

	protected override void OnSocketObscured(QuantumSocket socket)
	{
		if (socket != _shrineSocket)
		{
			base.OnSocketObscured(socket);
			return;
		}
		SocketedQuantumObject component = _quantumShrine.GetComponent<SocketedQuantumObject>();
		if ((_supportSocket == null || _supportSocket.IsOccupied()) && component.IsLocked() && !_socketCollapseDirty && !socket.IsOccupied() && Vector3.Distance(socket.transform.position, Locator.GetPlayerTransform().position) < 50f)
		{
			_recentlyObscuredSocket = socket;
			_socketCollapseDirty = AttemptCollapse();
			_recentlyObscuredSocket = null;
		}
	}
}
