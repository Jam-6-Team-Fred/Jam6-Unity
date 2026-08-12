using System.Collections.Generic;
using UnityEngine;

public class SectorResolutionScale : SectoredMonoBehaviour
{
	private static List<SectorResolutionScale> s_activeList = new List<SectorResolutionScale>(8);

	[Header("Per-Platform Target Resolutions")]
	[SerializeField]
	private DynamicResolutionManager.TargetResolution _xboxOne = DynamicResolutionManager.TargetResolution.Full;

	[SerializeField]
	private DynamicResolutionManager.TargetResolution _xboxOneS = DynamicResolutionManager.TargetResolution.Full;

	[SerializeField]
	private DynamicResolutionManager.TargetResolution _xboxOneX = DynamicResolutionManager.TargetResolution.Full;

	[SerializeField]
	private DynamicResolutionManager.TargetResolution _playstation4 = DynamicResolutionManager.TargetResolution.Full;

	[SerializeField]
	private DynamicResolutionManager.TargetResolution _playstation4Pro = DynamicResolutionManager.TargetResolution.Full;

	[SerializeField]
	private DynamicResolutionManager.TargetResolution _xboxSeriesS = DynamicResolutionManager.TargetResolution.Full;

	[SerializeField]
	private DynamicResolutionManager.TargetResolution _xboxSeriesX = DynamicResolutionManager.TargetResolution.Full;

	[SerializeField]
	private DynamicResolutionManager.TargetResolution _playstation5 = DynamicResolutionManager.TargetResolution.Full;

	public DynamicResolutionManager.TargetResolution targetResolution => DynamicResolutionManager.TargetResolution.Full;

	protected override void Awake()
	{
	}

	protected override void OnDestroy()
	{
		s_activeList.Remove(this);
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (targetResolution != DynamicResolutionManager.TargetResolution.Full && sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			s_activeList.Add(this);
			UpdateResolutionSettings();
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (targetResolution != DynamicResolutionManager.TargetResolution.Full && sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			s_activeList.Remove(this);
			UpdateResolutionSettings();
		}
	}

	private void UpdateResolutionSettings()
	{
		DynamicResolutionManager.TargetResolution targetResolution = DynamicResolutionManager.TargetResolution.Full;
		for (int i = 0; i < s_activeList.Count; i++)
		{
			if (s_activeList[i].targetResolution < targetResolution || targetResolution == DynamicResolutionManager.TargetResolution.Full)
			{
				targetResolution = s_activeList[i].targetResolution;
			}
		}
		DynamicResolutionManager.SetTargetResolution(targetResolution);
	}

	public static DynamicResolutionManager.TargetResolution GetLowestTargetResolution()
	{
		DynamicResolutionManager.TargetResolution targetResolution = DynamicResolutionManager.TargetResolution.Full;
		for (int i = 0; i < s_activeList.Count; i++)
		{
			if (s_activeList[i].targetResolution < targetResolution || targetResolution == DynamicResolutionManager.TargetResolution.Full)
			{
				targetResolution = s_activeList[i].targetResolution;
			}
		}
		return targetResolution;
	}
}
