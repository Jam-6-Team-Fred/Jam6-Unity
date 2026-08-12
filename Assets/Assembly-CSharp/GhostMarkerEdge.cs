using System;

[Serializable]
public class GhostMarkerEdge
{
	public GhostNodeMarker markerOne;

	public GhostNodeMarker markerTwo;

	public GhostMarkerEdge(GhostNodeMarker markerOne, GhostNodeMarker markerTwo)
	{
		this.markerOne = markerOne;
		this.markerTwo = markerTwo;
	}

	public bool Equals(GhostMarkerEdge edge)
	{
		return Equals(edge.markerOne, edge.markerTwo);
	}

	public bool Equals(GhostNodeMarker markerOne, GhostNodeMarker markerTwo)
	{
		if (!(this.markerOne == markerOne) || !(this.markerTwo == markerTwo))
		{
			if (this.markerOne == markerTwo)
			{
				return this.markerTwo == markerOne;
			}
			return false;
		}
		return true;
	}

	public bool ContainsNode(GhostNodeMarker marker)
	{
		if (!(marker == markerOne))
		{
			return marker == markerTwo;
		}
		return true;
	}
}
