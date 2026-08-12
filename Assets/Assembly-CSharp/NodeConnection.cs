using System;
using UnityEngine;

[Serializable]
public class NodeConnection
{
	public int nodeOneID;

	public int nodeTwoID;

	public Renderer renderer;

	private OWRenderer _owRenderer;

	public bool ConnectedToNode(int nodeID)
	{
		if (nodeOneID != nodeID)
		{
			return nodeTwoID == nodeID;
		}
		return true;
	}

	public void SetActive(bool active)
	{
		if (_owRenderer == null)
		{
			_owRenderer = renderer.gameObject.GetComponent<OWRenderer>();
		}
		_owRenderer.SetActivation(active);
	}
}
