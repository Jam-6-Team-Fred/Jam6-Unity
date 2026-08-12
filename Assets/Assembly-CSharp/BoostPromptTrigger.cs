using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class BoostPromptTrigger : MonoBehaviour
{
	private OWTriggerVolume _trigger;

	private ScreenPrompt _boostPrompt;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		_boostPrompt = new ScreenPrompt(InputLibrary.thrustUp, InputLibrary.boost, UITextLibrary.GetString(UITextType.BoosterPrompt) + "<CMD1>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "  +<CMD2>", ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void Update()
	{
		bool flag = Vector3.Angle(Locator.GetPlayerTransform().forward, base.transform.forward) < 60f;
		_boostPrompt.SetVisibility(!PlayerData.GetAutoBoost() && flag && !PlayerState.InZeroG());
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			Locator.GetPromptManager().AddScreenPrompt(_boostPrompt, PromptPosition.Center);
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			Locator.GetPromptManager().RemoveScreenPrompt(_boostPrompt);
			base.enabled = false;
		}
	}
}
