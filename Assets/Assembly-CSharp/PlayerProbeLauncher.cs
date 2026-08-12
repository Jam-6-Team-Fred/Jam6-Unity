using UnityEngine;

public class PlayerProbeLauncher : ProbeLauncher
{
	protected override bool IsLaunchObstructed()
	{
		if (_name != Name.Player)
		{
			return false;
		}
		Vector3 direction = _launcherTransform.position - Locator.GetPlayerTransform().position;
		if (Physics.Raycast(Locator.GetPlayerTransform().position, direction, out var hitInfo, direction.magnitude, OWLayerMask.physicalMask))
		{
			IgnoreCollision component = hitInfo.collider.GetComponent<IgnoreCollision>();
			if (component == null || !component.IgnoresProbe())
			{
				return true;
			}
		}
		if (Physics.Raycast(_launcherTransform.position, _launcherTransform.forward, out hitInfo, 0.5f, OWLayerMask.physicalMask))
		{
			IgnoreCollision component2 = hitInfo.collider.GetComponent<IgnoreCollision>();
			if (component2 == null || !component2.IgnoresProbe())
			{
				return true;
			}
		}
		return false;
	}
}
