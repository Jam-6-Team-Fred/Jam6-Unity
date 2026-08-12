using UnityEngine;

public abstract class AbstractDoor : MonoBehaviour
{
	[SerializeField]
	private AbstractGhostDoorInterface _interface;

	[SerializeField]
	protected bool _startOpen;

	protected bool _open;

	public OWEvent OnOpen = new OWEvent(1);

	public OWEvent OnClose = new OWEvent(1);

	private void Awake()
	{
		SetOpenImmediate(_startOpen);
		if (_interface != null)
		{
			_interface.SetStartingPosition(_startOpen);
		}
	}

	protected virtual void Start()
	{
		base.enabled = false;
		if (_interface != null)
		{
			_interface.OnOpen += Open;
			_interface.OnClose += Close;
		}
	}

	private void OnDestroy()
	{
		if (_interface != null)
		{
			_interface.OnOpen -= Open;
			_interface.OnClose -= Close;
		}
	}

	public bool IsOpen()
	{
		return _open;
	}

	public virtual void SetOpenImmediate(bool open)
	{
		_open = open;
	}

	public virtual bool IsOpening()
	{
		if (_open)
		{
			return base.enabled;
		}
		return false;
	}

	public virtual bool IsClosing()
	{
		if (!_open)
		{
			return base.enabled;
		}
		return false;
	}

	public virtual void Open()
	{
		base.enabled = true;
		_open = true;
		OnOpen.Invoke();
	}

	public virtual void Close()
	{
		base.enabled = true;
		_open = false;
		OnClose.Invoke();
	}
}
