using System.Collections.Generic;
using UnityEngine;

public class NomaiNodeController : MonoBehaviour
{
	[SerializeField]
	private NomaiInterfaceSlot _resetSlot;

	[SerializeField]
	private NomaiInterfaceNode[] _nodes;

	[SerializeField]
	private NodeConnection[] _connections;

	[SerializeField]
	private Material _inactiveMaterial;

	[SerializeField]
	private Material _activeMaterial;

	private List<int> _activeNodes;

	private void Start()
	{
		for (int i = 0; i < _nodes.Length; i++)
		{
			_nodes[i].slot.OnSlotActivated += OnSlotActivated;
			_nodes[i].slot.SetCancelsDragOnCollision(cancelsDrag: false);
		}
		for (int j = 0; j < _connections.Length; j++)
		{
			_connections[j].SetActive(active: false);
		}
		if (_resetSlot != null)
		{
			_resetSlot.OnSlotActivated += OnResetSlotActivated;
		}
		_activeNodes = new List<int>(16);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _nodes.Length; i++)
		{
			_nodes[i].slot.OnSlotActivated -= OnSlotActivated;
		}
		if (_resetSlot != null)
		{
			_resetSlot.OnSlotActivated -= OnResetSlotActivated;
		}
	}

	private void OnResetSlotActivated(NomaiInterfaceSlot slot)
	{
		ResetNodes();
	}

	public bool CheckCoordinate(int[] coordinate)
	{
		if (coordinate.Length != _activeNodes.Count)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < coordinate.Length; i++)
		{
			if (coordinate[i] == _activeNodes[i])
			{
				num++;
			}
		}
		if (num == coordinate.Length)
		{
			return true;
		}
		num = 0;
		for (int j = 0; j < coordinate.Length; j++)
		{
			if (coordinate[j] == _activeNodes[coordinate.Length - 1 - j])
			{
				num++;
			}
		}
		return num == coordinate.Length;
	}

	private void ResetNodes()
	{
		_activeNodes.Clear();
		for (int i = 0; i < _nodes.Length; i++)
		{
			_nodes[i].active = false;
			_nodes[i].renderer.material = _inactiveMaterial;
		}
		for (int j = 0; j < _connections.Length; j++)
		{
			_connections[j].SetActive(active: false);
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		NomaiInterfaceNode nomaiInterfaceNode = SlotToNode(slot);
		int nodeID = GetNodeID(nomaiInterfaceNode);
		if (nomaiInterfaceNode.active)
		{
			nomaiInterfaceNode.active = false;
			nomaiInterfaceNode.renderer.material = _inactiveMaterial;
			_activeNodes.Remove(nodeID);
		}
		else
		{
			_activeNodes.Add(nodeID);
			nomaiInterfaceNode.active = true;
			nomaiInterfaceNode.renderer.material = _activeMaterial;
		}
		UpdateConnections();
	}

	private void UpdateConnections()
	{
		for (int i = 0; i < _connections.Length; i++)
		{
			bool active = false;
			for (int j = 0; j < _activeNodes.Count; j++)
			{
				if (_activeNodes[j] == _connections[i].nodeOneID && (GetActiveNodeAtIndex(j - 1) == _connections[i].nodeTwoID || GetActiveNodeAtIndex(j + 1) == _connections[i].nodeTwoID))
				{
					active = true;
					break;
				}
			}
			_connections[i].SetActive(active);
		}
	}

	private int GetActiveNodeAtIndex(int index)
	{
		if (index >= 0 && index < _activeNodes.Count)
		{
			return _activeNodes[index];
		}
		return -1;
	}

	private NomaiInterfaceNode SlotToNode(NomaiInterfaceSlot slot)
	{
		for (int i = 0; i < _nodes.Length; i++)
		{
			if (_nodes[i].slot == slot)
			{
				return _nodes[i];
			}
		}
		return null;
	}

	private int GetNodeID(NomaiInterfaceNode node)
	{
		for (int i = 0; i < _nodes.Length; i++)
		{
			if (_nodes[i] == node)
			{
				return i;
			}
		}
		return -1;
	}
}
