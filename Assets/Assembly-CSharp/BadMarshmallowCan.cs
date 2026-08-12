using UnityEngine;

[RequireComponent(typeof(InteractReceiver))]
public class BadMarshmallowCan : MonoBehaviour
{
	[SerializeField]
	private Texture2D _badMallowTex;

	private InteractReceiver _interactReceiver;

	private bool _pickedUp;

	private bool _bigHeadModeEnabled;

	private static bool s_warpedToEyeWithBigHeadModeEnabled;

	private void Awake()
	{
		_interactReceiver = this.GetRequiredComponent<InteractReceiver>();
		_interactReceiver.OnPressInteract += OnPressInteract;
		GlobalMessenger<float>.AddListener("EatMarshmallow", OnEatMarshmallow);
		GlobalMessenger.AddListener("StartVesselWarp", OnStartVesselWarp);
	}

	private void Start()
	{
		_interactReceiver.SetPromptText(UITextType.ItemPickUpPrompt);
	}

	private void OnDestroy()
	{
		_interactReceiver.OnPressInteract -= OnPressInteract;
		GlobalMessenger<float>.RemoveListener("EatMarshmallow", OnEatMarshmallow);
		GlobalMessenger.RemoveListener("StartVesselWarp", OnStartVesselWarp);
	}

	private void OnPressInteract()
	{
		_pickedUp = true;
		Marshmallow componentInChildren = Locator.GetPlayerBody().GetComponentInChildren<Marshmallow>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.SetTexture(_badMallowTex);
		}
		Locator.GetPlayerAudioController().PlayBadMarshmallowCanPickUp();
		base.gameObject.SetActive(value: false);
	}

	private void OnEatMarshmallow(float toastedFraction)
	{
		if (_pickedUp && !_bigHeadModeEnabled && toastedFraction < 1f)
		{
			GlobalMessenger.FireEvent("EnableBigHeadMode");
			Locator.GetPlayerAudioController().PlayBadMarshmallowEat();
			_bigHeadModeEnabled = true;
		}
	}

	private void OnStartVesselWarp()
	{
		if (_bigHeadModeEnabled)
		{
			s_warpedToEyeWithBigHeadModeEnabled = true;
		}
	}

	public static void CheckEotUState()
	{
		if (s_warpedToEyeWithBigHeadModeEnabled)
		{
			GlobalMessenger.FireEvent("EnableBigHeadMode");
			s_warpedToEyeWithBigHeadModeEnabled = false;
		}
	}
}
