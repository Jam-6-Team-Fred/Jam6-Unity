using UnityEngine;

public class EyeSpawnPoint : SpawnPoint
{
	[SerializeField]
	private EyeState _eyeState;

	public EyeState GetEyeState()
	{
		return _eyeState;
	}
}
