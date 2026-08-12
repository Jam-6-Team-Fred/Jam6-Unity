using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RepairVolume : MonoBehaviour
{
	public delegate void RepairEvent();

	[SerializeField]
	private float _repairDistance = 3f;

	private float _secondsToRepair = 3f;

	private float _repairFraction;

	private bool _isRepairing;

	private InteractReceiver _interactReceiver;

	public event RepairEvent OnCompleteRepair;

	private void Awake()
	{
		_interactReceiver = base.gameObject.AddComponent<InteractReceiver>();
		_interactReceiver.OnGainFocus += OnGainFocus;
		_interactReceiver.OnLoseFocus += OnLoseFocus;
		_interactReceiver.OnPressInteract += OnPressInteract;
		_interactReceiver.OnReleaseInteract += OnReleaseInteract;
	}

	private void Start()
	{
		_interactReceiver.SetPromptText(UITextType.HoldPrompt, UITextType.RepairPrompt);
		_interactReceiver.SetInteractRange(_repairDistance);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_interactReceiver.OnGainFocus -= OnGainFocus;
		_interactReceiver.OnLoseFocus -= OnLoseFocus;
		_interactReceiver.OnPressInteract -= OnPressInteract;
		_interactReceiver.OnReleaseInteract -= OnReleaseInteract;
	}

	public InteractReceiver GetInteractReceiver()
	{
		return _interactReceiver;
	}

	public void Activate()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Deactivate()
	{
		base.enabled = false;
		_interactReceiver.DisableInteraction();
		base.gameObject.SetActive(value: false);
	}

	public float GetRepairFraction()
	{
		return _repairFraction;
	}

	public void ResetVolume()
	{
		_repairFraction = 0f;
		_interactReceiver.DisableInteraction();
	}

	private void Update()
	{
		if (_isRepairing && _repairFraction < 1f)
		{
			_repairFraction += Time.deltaTime / _secondsToRepair;
			_repairFraction = Mathf.Clamp01(_repairFraction);
			if (_repairFraction >= 1f && this.OnCompleteRepair != null)
			{
				GlobalMessenger.FireEvent("FinishRepairing");
				this.OnCompleteRepair();
				_isRepairing = false;
			}
		}
	}

	private void OnGainFocus()
	{
		base.enabled = true;
	}

	private void OnLoseFocus()
	{
		if (_repairFraction < 1f)
		{
			_interactReceiver.ResetInteraction();
		}
		if (_isRepairing)
		{
			GlobalMessenger.FireEvent("StopRepairing");
		}
		_isRepairing = false;
		base.enabled = false;
	}

	private void OnPressInteract()
	{
		_isRepairing = true;
		GlobalMessenger.FireEvent("StartRepairing");
	}

	private void OnReleaseInteract()
	{
		if (_repairFraction < 1f)
		{
			_interactReceiver.ResetInteraction();
		}
		_isRepairing = false;
		GlobalMessenger.FireEvent("StopRepairing");
	}
}
