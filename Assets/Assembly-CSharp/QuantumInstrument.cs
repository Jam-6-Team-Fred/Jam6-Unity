using UnityEngine;

public class QuantumInstrument : MonoBehaviour
{
	public delegate void GatherEvent(float flickerOutDuration);

	public delegate void FinishGatherEvent();

	[SerializeField]
	private GameObject[] _activateObjects;

	[SerializeField]
	private GameObject[] _deactivateObjects;

	[SerializeField]
	private bool _gatherWithScope;

	private InteractReceiver _interactReceiver;

	private bool _waitToFlickerOut;

	private float _flickerOutTime;

	private ScreenPrompt _scopeGatherPrompt;

	public event GatherEvent OnGather;

	public event FinishGatherEvent OnFinishGather;

	private void Awake()
	{
		_interactReceiver = GetComponent<InteractReceiver>();
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract += OnPressInteract;
		}
	}

	private void Start()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.SetPromptText(UITextType.GatherPrompt);
		}
		else if (_gatherWithScope)
		{
			_scopeGatherPrompt = new ScreenPrompt(InputLibrary.interact, "<CMD> " + UITextLibrary.GetString(UITextType.GatherPrompt));
			Locator.GetPromptManager().AddScreenPrompt(_scopeGatherPrompt, PromptPosition.Center);
		}
	}

	private void OnDestroy()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract -= OnPressInteract;
		}
	}

	private void OnPressInteract()
	{
		Gather();
		_interactReceiver.DisableInteraction();
	}

	private void Gather()
	{
		float num = 1f;
		GlobalMessenger<float, float>.FireEvent("FlickerOffAndOn", num, 2f);
		_flickerOutTime = Time.time + num;
		_waitToFlickerOut = true;
		if (this.OnGather != null)
		{
			this.OnGather(_flickerOutTime);
		}
	}

	private void Update()
	{
		if (_gatherWithScope && !_waitToFlickerOut)
		{
			_scopeGatherPrompt.SetVisibility(isVisible: false);
			if (Locator.GetToolModeSwapper().GetSignalScope().InZoomMode() && Vector3.Angle(base.transform.position - Locator.GetPlayerCamera().transform.position, Locator.GetPlayerCamera().transform.forward) < 1f)
			{
				_scopeGatherPrompt.SetVisibility(isVisible: true);
				if (OWInput.IsNewlyPressed(InputLibrary.interact))
				{
					Gather();
					Locator.GetPromptManager().RemoveScreenPrompt(_scopeGatherPrompt);
				}
			}
		}
		if (_waitToFlickerOut && Time.time > _flickerOutTime)
		{
			FinishGather();
		}
	}

	private void FinishGather()
	{
		if (this.OnFinishGather != null)
		{
			this.OnFinishGather();
		}
		for (int i = 0; i < _activateObjects.Length; i++)
		{
			_activateObjects[i].SetActive(value: true);
		}
		for (int j = 0; j < _deactivateObjects.Length; j++)
		{
			_deactivateObjects[j].SetActive(value: false);
		}
		base.enabled = false;
	}
}
