using UnityEngine;

public class SandfallHazardVolume : HazardVolume
{
	[SerializeField]
	private SandFunnelTriggerVolume _sandfallTriggerVolume;

	public override float GetDamagePerSecond(HazardDetector detector)
	{
		if (detector.CompareTag("PlayerDetector") && Locator.GetPlayerController().IsGrounded() && _sandfallTriggerVolume.IsObjectExposed(detector.gameObject))
		{
			return _damagePerSecond;
		}
		if (detector.CompareTag("ProbeDetector") && Locator.GetProbe().IsAnchored() && _sandfallTriggerVolume.IsObjectExposed(detector.gameObject))
		{
			return _damagePerSecond;
		}
		return 0f;
	}

	public override HazardType GetHazardType()
	{
		return HazardType.SANDFALL;
	}
}
