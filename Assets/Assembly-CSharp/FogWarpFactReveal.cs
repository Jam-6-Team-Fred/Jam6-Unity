using UnityEngine;

[RequireComponent(typeof(FogWarpVolume))]
public class FogWarpFactReveal : MonoBehaviour
{
	[SerializeField]
	private string _factID;

	private FogWarpVolume _fogWarpVolume;

	private void Awake()
	{
		_fogWarpVolume = GetComponent<FogWarpVolume>();
		_fogWarpVolume.OnWarpDetector += OnWarpDetector;
	}

	private void OnDestroy()
	{
		_fogWarpVolume.OnWarpDetector -= OnWarpDetector;
	}

	private void OnWarpDetector(FogWarpDetector detector)
	{
		Locator.GetShipLogManager().RevealFact(_factID);
	}
}
