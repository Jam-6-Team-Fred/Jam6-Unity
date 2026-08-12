using UnityEngine;

public class AstroObjectDistanceTracker : DistanceTracker
{
	[SerializeField]
	private AstroObject.Name _astroObjectName1;

	[SerializeField]
	private AstroObject.Name _astroObjectName2;

	private AstroObject _astroObject1;

	private AstroObject _astroObject2;

	private Vector3 _vectorOneTwo;

	private void Start()
	{
		_astroObject1 = Locator.GetAstroObject(_astroObjectName1);
		_astroObject2 = Locator.GetAstroObject(_astroObjectName2);
		if (_astroObject1 == null)
		{
			Debug.LogWarning(string.Concat("AstroObject ", _astroObjectName1, " is null"));
		}
		if (_astroObject2 == null)
		{
			Debug.LogWarning(string.Concat("AstroObject ", _astroObjectName2, " is null"));
		}
	}

	private void FixedUpdate()
	{
		_vectorOneTwo = _astroObject2.transform.position - _astroObject1.transform.position;
	}

	public override Vector3 GetVector()
	{
		return _vectorOneTwo;
	}

	public override Vector3 GetReverseVector()
	{
		return _vectorOneTwo * -1f;
	}

	public override float GetVectorMagnitude()
	{
		return _vectorOneTwo.magnitude;
	}

	public override float GetVectorSquareMagnitude()
	{
		return _vectorOneTwo.sqrMagnitude;
	}
}
