using UnityEngine;

public class DreamRaftFluidDetector : AlignToSurfaceFluidDetector
{
	[Space]
	[SerializeField]
	private DreamRaftController _dreamRaftController;

	public DreamRaftController GetDreamRaftController()
	{
		return _dreamRaftController;
	}
}
