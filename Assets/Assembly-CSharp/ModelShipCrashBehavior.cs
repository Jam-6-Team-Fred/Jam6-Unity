using UnityEngine;

public class ModelShipCrashBehavior : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _crashEffect;

	private float _lastCrashTime;

	private void Awake()
	{
		this.GetRequiredComponent<ImpactSensor>().OnImpact += OnImpact;
	}

	private void OnDestroy()
	{
		this.GetRequiredComponent<ImpactSensor>().OnImpact -= OnImpact;
	}

	private void OnImpact(ImpactData impactData)
	{
		if (impactData.speed > 10f && Time.time > _lastCrashTime + 1f)
		{
			_lastCrashTime = Time.time;
			_crashEffect.Play();
			GetComponent<OWAudioSource>().PlayOneShot(AudioType.TH_ModelShipCrash);
			DialogueConditionManager.SharedInstance.SetConditionState("CrashedModelRocket", conditionState: true);
			DialogueConditionManager.SharedInstance.SetConditionState("LandedModelRocket");
		}
		AstroObject component = impactData.otherCollider.attachedRigidbody.GetComponent<AstroObject>();
		if (component != null && component.GetAstroObjectName() == AstroObject.Name.TimberMoon)
		{
			Achievements.Earn(Achievements.Type.HEARTH_TO_MOON);
		}
	}
}
