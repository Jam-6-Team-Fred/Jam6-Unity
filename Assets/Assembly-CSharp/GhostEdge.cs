using UnityEngine;

public class GhostEdge
{
	public int index = -1;

	public GhostNode nodeOne;

	public GhostNode nodeTwo;

	public float length;

	public GhostEdge(int index, GhostNode nodeOne, GhostNode nodeTwo)
	{
		this.index = index;
		this.nodeOne = nodeOne;
		this.nodeTwo = nodeTwo;
		length = Vector3.Distance(nodeOne.localPosition, nodeTwo.localPosition);
	}

	public void ToArray(GhostNode[] nodes)
	{
		if (nodes != null && nodes.Length == 2)
		{
			nodes[0] = nodeOne;
			nodes[1] = nodeTwo;
		}
		else
		{
			Debug.LogError("Array is null or the wrong length!");
		}
	}

	public bool Equals(GhostNode nodeOne, GhostNode nodeTwo)
	{
		if (this.nodeOne != nodeOne || this.nodeTwo != nodeTwo)
		{
			if (this.nodeOne == nodeTwo)
			{
				return this.nodeTwo == nodeOne;
			}
			return false;
		}
		return true;
	}

	public bool Equals(GhostEdge edge)
	{
		return Equals(edge.nodeOne, edge.nodeTwo);
	}

	public bool ContainsNode(GhostNode node)
	{
		if (node != nodeOne)
		{
			return node == nodeTwo;
		}
		return true;
	}
}
