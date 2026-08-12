using UnityEngine;

public class RotateToAstroObject : RotateToPoint
{
	[SerializeField]
	private AstroObject.Name _astroObjectLock;

	private void FixedUpdate()
	{
		if (_quaternionTargetMode)
		{
			_hasTargetLock = CheckLockedOn();
			IncrementalRotate(Time.fixedDeltaTime);
		}
		else if (_astroObjectLock != 0)
		{
			AstroObject astroObject = Locator.GetAstroObject(_astroObjectLock);
			if (astroObject == null)
			{
				_hasTargetLock = false;
				return;
			}
			_target = astroObject.transform.position;
			_hasTargetLock = CheckLockedOn();
			IncrementalRotate(Time.fixedDeltaTime);
		}
		else
		{
			_hasTargetLock = false;
		}
	}

	public void SetNewAstroTarget(AstroObject.Name name, bool resetRampUp)
	{
		if (resetRampUp)
		{
			ResetRotationSpeed(resetRampUp);
		}
		_astroObjectLock = name;
	}
}
