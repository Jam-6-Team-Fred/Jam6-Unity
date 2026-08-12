using UnityEngine;

public class LudicrousSpeed : MonoBehaviour
{
	private bool _engageLudicrousSpeed;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.L))
		{
			_engageLudicrousSpeed = true;
		}
	}

	private void FixedUpdate()
	{
		if (_engageLudicrousSpeed)
		{
			_engageLudicrousSpeed = false;
			Locator.GetShipBody().AddVelocityChange(Locator.GetShipBody().transform.forward * 25000f);
			MonoBehaviour.print("ENGAGE LUDICROUS SPEED");
			Debug.Break();
		}
	}
}
