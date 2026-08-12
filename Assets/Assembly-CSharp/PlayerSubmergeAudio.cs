using UnityEngine;

public class PlayerSubmergeAudio : MonoBehaviour
{
	private OWAudioSource _source;

	private void Awake()
	{
		_source = GetComponent<OWAudioSource>();
		GlobalMessenger<float>.AddListener("PlayerCameraEnterWater", OnPlayerCameraEnterWater);
		GlobalMessenger<float>.AddListener("ShipEnterWater", OnShipEnterWater);
	}

	private void OnDestroy()
	{
		GlobalMessenger<float>.RemoveListener("PlayerCameraEnterWater", OnPlayerCameraEnterWater);
		GlobalMessenger<float>.RemoveListener("ShipEnterWater", OnShipEnterWater);
	}

	private void OnPlayerCameraEnterWater(float relativeSpeed)
	{
		if (Mathf.InverseLerp(5f, 15f, relativeSpeed) > 0f)
		{
			_source.PlayOneShot(AudioType.Submerge_Player);
		}
	}

	private void OnShipEnterWater(float relativeSpeed)
	{
		float num = Mathf.InverseLerp(5f, 15f, relativeSpeed);
		if (PlayerState.IsInsideShip() && num > 0f)
		{
			_source.PlayOneShot(AudioType.Submerge_Ship, num);
		}
	}
}
