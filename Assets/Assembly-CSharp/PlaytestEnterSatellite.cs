public class PlaytestEnterSatellite : SectoredMonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.CompareTag("PlayerDetector"))
		{
			PlaytestDiscoveryPrompt instance = PlaytestDiscoveryPrompt.instance;
			if (instance != null && instance.enterSatelliteLoop < 0)
			{
				instance.enterSatelliteLoop = TimeLoop.GetLoopCount();
			}
		}
	}
}
