using UnityEngine;

public class DreamEyeMaskController : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve _cutoffCurve;

	private OWRenderer _renderer;

	private void Start()
	{
		_renderer = GetComponent<OWRenderer>();
		SetEyesOpenFraction(0f);
		AlarmSequenceController alarmSequenceController = Locator.GetAlarmSequenceController();
		if (alarmSequenceController != null)
		{
			alarmSequenceController.RegisterDreamEyeMaskController(this);
		}
		GlobalMessenger<bool>.AddListener("StartSleepingAtCampfire", CaptureCampfireScreenshot);
	}

	private void OnDestroy()
	{
		GlobalMessenger<bool>.RemoveListener("StartSleepingAtCampfire", CaptureCampfireScreenshot);
	}

	public void CaptureCampfireScreenshot(bool isDreamCampfire)
	{
	}

	public void SetEyesOpenFraction(float fraction)
	{
		float cutoff = _cutoffCurve.Evaluate(fraction);
		_renderer.SetCutoff(cutoff);
		_renderer.GetRenderer().enabled = fraction > 0.001f;
	}
}
