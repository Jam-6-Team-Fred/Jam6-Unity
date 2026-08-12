using UnityEngine;

public abstract class NomaiRotator : MonoBehaviour
{
	protected bool _locked;

	[SerializeField]
	protected float _cycleLength = 1f;

	[Space(10f)]
	[SerializeField]
	protected NomaiInterfaceSlot[] _openSwitches = new NomaiInterfaceSlot[0];

	[SerializeField]
	protected NomaiInterfaceSlot[] _closeSwitches = new NomaiInterfaceSlot[0];

	[SerializeField]
	protected NomaiInterfaceSlot[] _cycleSwitches = new NomaiInterfaceSlot[0];

	[Space(10f)]
	[SerializeField]
	protected OWAudioSource _audioSource;

	protected virtual void Start()
	{
		for (int i = 0; i < _openSwitches.Length; i++)
		{
			if (_openSwitches[i] != null)
			{
				_openSwitches[i].OnSlotActivated += OnOpenSwitchTriggered;
			}
		}
		for (int j = 0; j < _closeSwitches.Length; j++)
		{
			if (_closeSwitches[j] != null)
			{
				_closeSwitches[j].OnSlotActivated += OnCloseSwitchTriggered;
			}
		}
		for (int k = 0; k < _cycleSwitches.Length; k++)
		{
			if (_cycleSwitches[k] != null)
			{
				_cycleSwitches[k].OnSlotActivated += OnCycleSwitchTriggered;
			}
		}
	}

	protected void OnDestroy()
	{
		for (int i = 0; i < _openSwitches.Length; i++)
		{
			if (_openSwitches[i] != null)
			{
				_openSwitches[i].OnSlotActivated -= OnOpenSwitchTriggered;
			}
		}
		for (int j = 0; j < _closeSwitches.Length; j++)
		{
			if (_closeSwitches[j] != null)
			{
				_closeSwitches[j].OnSlotActivated -= OnCloseSwitchTriggered;
			}
		}
		for (int k = 0; k < _cycleSwitches.Length; k++)
		{
			if (_cycleSwitches[k] != null)
			{
				_cycleSwitches[k].OnSlotActivated -= OnCycleSwitchTriggered;
			}
		}
	}

	public abstract void Open(NomaiInterfaceSlot slot);

	public abstract void Close(NomaiInterfaceSlot slot);

	public abstract void Cycle(NomaiInterfaceSlot slot);

	public abstract bool IsOpen();

	public abstract bool IsCycling();

	public abstract bool IsOpening();

	public abstract bool IsClosing();

	public virtual void Lock()
	{
		_locked = true;
	}

	public virtual void Unlock()
	{
		_locked = false;
	}

	public virtual bool IsLocked()
	{
		return _locked;
	}

	protected void OnOpenSwitchTriggered(NomaiInterfaceSlot slot)
	{
		Open(slot);
	}

	protected void OnCloseSwitchTriggered(NomaiInterfaceSlot slot)
	{
		Close(slot);
	}

	protected void OnCycleSwitchTriggered(NomaiInterfaceSlot slot)
	{
		Cycle(slot);
	}
}
