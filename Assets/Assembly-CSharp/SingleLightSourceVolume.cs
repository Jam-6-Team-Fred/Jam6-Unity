using UnityEngine;

public class SingleLightSourceVolume : LightSourceVolume
{
	[SerializeField]
	private OWLight2 _light;

	protected override void Awake()
	{
		base.Awake();
		LinkLightSource(_light);
	}
}
