using UnityEngine;

public class NomaiGatewaySlab : MonoBehaviour
{
	public delegate void GatewaySlabStartEvent();

	public delegate void GatewaySlabStopEvent();

	[SerializeField]
	private float _openOffset;

	[SerializeField]
	private bool _rotate;

	[SerializeField]
	private float _speed = 1f;

	[SerializeField]
	private float _acceleration = 0.1f;

	private float _currentSpeed;

	private bool _open;

	private Vector3 _closePosition;

	private Vector3 _openPosition;

	private Quaternion _closeRotation;

	private Quaternion _openRotation;

	public event GatewaySlabStartEvent OnGatewaySlabStart;

	public event GatewaySlabStopEvent OnGatewaySlabStop;

	private void Awake()
	{
		base.enabled = false;
		if (_rotate)
		{
			_closeRotation = base.transform.localRotation;
			_openRotation = Quaternion.AngleAxis(_openOffset, Vector3.up) * base.transform.localRotation;
		}
		else
		{
			_closePosition = base.transform.localPosition;
			_openPosition = _closePosition - Vector3.right * _openOffset;
		}
	}

	public void OpenImmediate()
	{
		_open = true;
		if (_rotate)
		{
			base.transform.localRotation = (_open ? _openRotation : _closeRotation);
		}
		else
		{
			base.transform.localPosition = (_open ? _openPosition : _closePosition);
		}
	}

	public void SetOpen(bool open)
	{
		if (open != _open)
		{
			_open = open;
			bool flag = base.enabled;
			base.enabled = true;
			if (this.OnGatewaySlabStart != null && !flag)
			{
				this.OnGatewaySlabStart();
			}
		}
	}

	public bool IsOpen()
	{
		return _open;
	}

	public float GetOpenFraction()
	{
		if (_rotate)
		{
			float value = Quaternion.Angle(base.transform.localRotation, _closeRotation);
			return Mathf.InverseLerp(0f, _openOffset, value);
		}
		if (!base.enabled)
		{
			if (!_open)
			{
				return 0f;
			}
			return 1f;
		}
		return Mathf.InverseLerp(_closePosition.x, _openPosition.x, base.transform.localPosition.x);
	}

	private void FixedUpdate()
	{
		if (_rotate)
		{
			Quaternion quaternion = (_open ? _openRotation : _closeRotation);
			_currentSpeed = Mathf.MoveTowards(_currentSpeed, _speed, _acceleration * Time.deltaTime);
			base.transform.localRotation = Quaternion.RotateTowards(base.transform.localRotation, quaternion, _currentSpeed * Time.deltaTime);
			if (Quaternion.Angle(base.transform.localRotation, quaternion) < 0.1f)
			{
				_currentSpeed = 0f;
				base.transform.localRotation = quaternion;
				base.enabled = false;
				if (this.OnGatewaySlabStop != null)
				{
					this.OnGatewaySlabStop();
				}
			}
			return;
		}
		Vector3 localPosition = (_open ? _openPosition : _closePosition);
		float target = (_open ? (0f - _speed) : _speed);
		_currentSpeed = Mathf.MoveTowards(_currentSpeed, target, _acceleration * Time.deltaTime);
		base.transform.localPosition = base.transform.localPosition + Vector3.right * _currentSpeed * Time.deltaTime;
		if ((_open && base.transform.localPosition.x < _openPosition.x) || (!_open && base.transform.localPosition.x > _closePosition.x))
		{
			_currentSpeed = 0f;
			base.transform.localPosition = localPosition;
			base.enabled = false;
			if (this.OnGatewaySlabStop != null)
			{
				this.OnGatewaySlabStop();
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!_rotate)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(base.transform.position, base.transform.position - base.transform.right * _openOffset);
		}
	}
}
