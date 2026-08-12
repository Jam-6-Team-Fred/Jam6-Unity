using UnityEngine;

public class MatchSocketTransform : MonoBehaviour
{
	[SerializeField]
	private Transform _socket;

	[SerializeField]
	private bool _update;

	[SerializeField]
	private bool _lock;

	private void Reset()
	{
		if (_socket == null)
		{
			_socket = base.transform.parent;
		}
	}

	private void OnValidate()
	{
		if (_update)
		{
			_update = false;
		}
		if (_socket != null && !_lock)
		{
			base.transform.position = _socket.position;
			base.transform.rotation = _socket.rotation;
		}
	}
}
