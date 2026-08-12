using UnityEngine;

[RequireComponent(typeof(InteractVolume))]
public class PlayerRecoveryPoint : MonoBehaviour
{
	[SerializeField]
	private bool _refuelsPlayer = true;

	[SerializeField]
	private bool _healsPlayer;

	[SerializeField]
	private bool _cleansVisor;

	[SerializeField]
	private bool _DLCFuelTank;

	private SingleInteractionVolume _interactVolume;

	private PlayerResources _playerResources;

	private VisorEffectController _playerVisor;

	private PlayerAudioController _playerAudioController;

	private bool _recovering;

	private void Awake()
	{
		_interactVolume = this.GetRequiredComponent<SingleInteractionVolume>();
		_interactVolume.OnPressInteract += OnPressInteract;
		_interactVolume.OnGainFocus += OnGainFocus;
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
	}

	private void Start()
	{
		_interactVolume.DisableInteraction();
		_playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
		_playerVisor = Locator.GetPlayerCamera().GetComponentInChildren<VisorEffectController>();
		_playerAudioController = Locator.GetPlayerAudioController();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_interactVolume.OnPressInteract -= OnPressInteract;
		_interactVolume.OnGainFocus -= OnGainFocus;
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
	}

	private void OnSuitUp()
	{
		_interactVolume.EnableInteraction();
	}

	private void OnRemoveSuit()
	{
		_interactVolume.DisableInteraction();
	}

	private void OnGainFocus()
	{
		if (_playerResources == null)
		{
			_playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
		}
		bool num = _playerResources.GetHealthFraction() == 1f;
		bool num2 = _playerResources.GetFuelFraction() == 1f;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (!num2 && _refuelsPlayer)
		{
			flag = true;
		}
		if (!num && _healsPlayer)
		{
			flag2 = true;
		}
		if (_playerVisor.GetDirtFraction() > 0.01f && _cleansVisor)
		{
			flag3 = true;
		}
		UITextType uITextType = UITextType.None;
		if (flag2 && flag)
		{
			uITextType = UITextType.RefillPrompt_0;
			if (flag3)
			{
				uITextType = UITextType.RefillPrompt_1;
			}
			_interactVolume.SetKeyCommandVisible(value: true);
		}
		else if (flag2)
		{
			uITextType = UITextType.RefillPrompt_2;
			if (flag3)
			{
				uITextType = UITextType.RefillPrompt_3;
			}
			_interactVolume.SetKeyCommandVisible(value: true);
		}
		else if (flag)
		{
			uITextType = UITextType.RefillPrompt_4;
			if (flag3)
			{
				uITextType = UITextType.RefillPrompt_5;
			}
			_interactVolume.SetKeyCommandVisible(value: true);
		}
		else if (flag3)
		{
			uITextType = UITextType.RefillPrompt_6;
		}
		else if (_refuelsPlayer && _healsPlayer)
		{
			uITextType = UITextType.RefillPrompt_7;
			_interactVolume.SetKeyCommandVisible(value: false);
		}
		else if (_refuelsPlayer)
		{
			uITextType = UITextType.RefillPrompt_8;
			_interactVolume.SetKeyCommandVisible(value: false);
		}
		else if (_healsPlayer)
		{
			uITextType = UITextType.RefillPrompt_9;
			_interactVolume.SetKeyCommandVisible(value: false);
		}
		if (uITextType != 0)
		{
			_interactVolume.ChangePrompt(uITextType);
		}
	}

	private void OnPressInteract()
	{
		bool flag = _playerResources.GetFuelFraction() != 1f;
		bool flag2 = _playerResources.GetHealthFraction() != 1f;
		bool flag3 = _playerVisor.GetDirtFraction() > 0.01f;
		bool flag4 = false;
		if (flag && _refuelsPlayer)
		{
			flag4 = true;
		}
		if (flag2 && _healsPlayer)
		{
			flag4 = true;
		}
		if (flag3 && _cleansVisor)
		{
			flag4 = true;
		}
		if (flag4)
		{
			_playerResources.StartRefillResources(_refuelsPlayer, _healsPlayer, _DLCFuelTank);
			if (_cleansVisor)
			{
				_playerVisor.ClearVisorDirt();
			}
			if (_playerAudioController != null)
			{
				if (flag && _refuelsPlayer)
				{
					_playerAudioController.PlayRefuel();
				}
				if (flag2 && _healsPlayer)
				{
					_playerAudioController.PlayMedkit();
				}
				if (flag3)
				{
					_ = _cleansVisor;
				}
			}
			_recovering = true;
			base.enabled = true;
		}
		else
		{
			_interactVolume.ResetInteraction();
		}
	}

	private void Update()
	{
		if (_recovering)
		{
			bool flag = true;
			if (_refuelsPlayer && _playerResources.GetFuelFraction() < 1f)
			{
				flag = false;
			}
			if (_healsPlayer && _playerResources.GetHealthFraction() < 1f)
			{
				flag = false;
			}
			if (_cleansVisor && _playerVisor.GetDirtFraction() > 0.01f)
			{
				flag = false;
			}
			if (flag)
			{
				_playerResources.StopRefillResources();
				_recovering = false;
			}
		}
		if (!_recovering)
		{
			base.enabled = false;
		}
	}
}
