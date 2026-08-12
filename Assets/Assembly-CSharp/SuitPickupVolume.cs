using UnityEngine;

public class SuitPickupVolume : MonoBehaviour
{
	[SerializeField]
	private bool _isTrainingSuit;

	[SerializeField]
	private bool _allowSuitReturn = true;

	[Space(10f)]
	[SerializeField]
	private GameObject _suitGeometry;

	[SerializeField]
	private GameObject[] _toolGeometry = new GameObject[0];

	[SerializeField]
	private float _toolPickupDelay = 0.5f;

	[Space(10f)]
	[SerializeField]
	private Collider _suitCollider;

	[Space(10f)]
	[SerializeField]
	private bool _enableSuitOptionsMenu;

	private SuitMenuManager _suitSettingsMenu;

	private bool _isSuitSettingsOpen;

	private bool _containsSuit = true;

	private MultipleInteractionVolume _interactVolume;

	private float _timer;

	private int _index;

	private OWCollider _suitOWCollider;

	private int _pickupSuitCommandIndex = -1;

	private int _openMenuCommandIndex = -1;

	private void Awake()
	{
		_interactVolume = this.GetRequiredComponentInChildren<MultipleInteractionVolume>();
		_interactVolume.OnPressInteract += OnPressInteract;
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
		if (_suitCollider != null)
		{
			_suitOWCollider = _suitCollider.gameObject.GetAddComponent<OWCollider>();
		}
	}

	private void Start()
	{
		InitializeInteractions();
		base.enabled = false;
	}

	private void InitializeInteractions()
	{
		_pickupSuitCommandIndex = _interactVolume.AddInteraction(InputLibrary.interact, InputMode.Character, UITextType.SuitUpPrompt, startEnabled: true, displayCommandIcon: true);
		bool flag = _enableSuitOptionsMenu && PlayerData.GetPersistentCondition("PREFLIGHT_CHECKLIST_UNLOCKED");
		_openMenuCommandIndex = _interactVolume.AddInteraction(InputLibrary.interactSecondary, InputMode.Character, UITextType.SuitMenuInteract, flag, displayCommandIcon: true);
		if (flag && !PlayerData.GetPersistentCondition("HAS_USED_PREFLIGHT_CHECKLIST"))
		{
			_interactVolume.GetInteractionAt(1).cachedScreenPrompt.SetDisplayState(ScreenPrompt.DisplayState.Attention);
		}
	}

	private void OnDestroy()
	{
		_interactVolume.OnPressInteract -= OnPressInteract;
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
	}

	private void Update()
	{
		if (_index >= _toolGeometry.Length)
		{
			base.enabled = false;
			return;
		}
		_timer += Time.deltaTime;
		if (_timer >= _toolPickupDelay)
		{
			_toolGeometry[_index].SetActive(_containsSuit);
			_timer = 0f;
			_index++;
		}
	}

	private void OnSuitUp()
	{
		if (_containsSuit)
		{
			_interactVolume.EnableSingleInteraction(value: false, _pickupSuitCommandIndex);
		}
	}

	private void OnRemoveSuit()
	{
		if (_containsSuit)
		{
			_interactVolume.EnableSingleInteraction(value: true, _pickupSuitCommandIndex);
		}
	}

	private void OnPressInteract(IInputCommands command)
	{
		if (_isSuitSettingsOpen)
		{
			return;
		}
		if (command == _interactVolume.GetInteractionAt(_pickupSuitCommandIndex).inputCommand)
		{
			if (_containsSuit)
			{
				_containsSuit = false;
				if (_allowSuitReturn)
				{
					_interactVolume.ChangePrompt(UITextType.ReturnSuitPrompt, _pickupSuitCommandIndex);
				}
				else
				{
					_interactVolume.EnableSingleInteraction(value: false, _pickupSuitCommandIndex);
				}
				Locator.GetPlayerTransform().GetComponent<PlayerSpacesuit>().SuitUp(_isTrainingSuit);
			}
			else
			{
				_containsSuit = true;
				_interactVolume.ChangePrompt(UITextType.SuitUpPrompt, _pickupSuitCommandIndex);
				Locator.GetPlayerTransform().GetComponent<PlayerSpacesuit>().RemoveSuit();
			}
			_timer = 0f;
			_index = 0;
			if (_suitGeometry != null)
			{
				_suitGeometry.SetActive(_containsSuit);
			}
			if (_suitOWCollider != null)
			{
				_suitOWCollider.SetActivation(_containsSuit);
			}
			base.enabled = true;
		}
		else if (command == _interactVolume.GetInteractionAt(_openMenuCommandIndex).inputCommand && !_isSuitSettingsOpen && MenuStackManager.SharedInstance.GetMenuCount() == 0 && TryOpenSuitSettingsMenu())
		{
			_interactVolume.EnableSingleInteraction(value: false, _openMenuCommandIndex);
		}
	}

	public bool TryOpenSuitSettingsMenu()
	{
		if (_suitSettingsMenu == null)
		{
			_suitSettingsMenu = Locator.GetSceneMenuManager().suitMenu;
		}
		if (_suitSettingsMenu == null)
		{
			return false;
		}
		bool flag = true;
		if (OWInput.GetInputMode() != InputMode.Character)
		{
			flag = false;
		}
		if (flag)
		{
			_interactVolume.GetInteractionAt(1).cachedScreenPrompt.SetDisplayState(ScreenPrompt.DisplayState.Normal);
			PlayerData.SetPersistentCondition("HAS_USED_PREFLIGHT_CHECKLIST", state: true);
			_suitSettingsMenu.OpenSuitMenu();
			if (!_isSuitSettingsOpen)
			{
				_suitSettingsMenu.OnDeactivateSuitMenu += OnDeactivateSuitMenu;
			}
		}
		return _isSuitSettingsOpen;
	}

	private void OnDeactivateSuitMenu()
	{
		_isSuitSettingsOpen = false;
		_interactVolume.EnableSingleInteraction(value: true, _openMenuCommandIndex);
		_interactVolume.ResetInteraction();
		_suitSettingsMenu.OnDeactivateSuitMenu -= OnDeactivateSuitMenu;
	}
}
