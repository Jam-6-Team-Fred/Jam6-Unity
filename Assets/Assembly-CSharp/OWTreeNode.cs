using System.Collections.Generic;

public class OWTreeNode<T>
{
	private T _nodeValue;

	private OWTreeNode<T> _parent;

	private List<OWTreeNode<T>> _children;

	public OWTreeNode<T> Parent => _parent;

	public List<OWTreeNode<T>> Children => _children;

	public T Value => _nodeValue;

	public OWTreeNode(T value)
	{
		_parent = null;
		_children = new List<OWTreeNode<T>>();
		_nodeValue = value;
	}

	public OWTreeNode(OWTreeNode<T> parent, T value)
	{
		_parent = parent;
		_children = new List<OWTreeNode<T>>();
		_nodeValue = value;
	}

	public bool IsRootNode()
	{
		return _parent == null;
	}

	public OWTreeNode<T> GetRootNode()
	{
		OWTreeNode<T> oWTreeNode = this;
		while (oWTreeNode._parent != null)
		{
			oWTreeNode = oWTreeNode._parent;
		}
		return oWTreeNode;
	}

	public void SetParent(OWTreeNode<T> newParent)
	{
		if (_parent != null)
		{
			_parent._children.Remove(this);
		}
		if (newParent != null && !newParent._children.Contains(this))
		{
			newParent._children.Add(this);
		}
		_parent = newParent;
	}

	public void AddChild(OWTreeNode<T> childNode)
	{
		childNode.SetParent(this);
		if (!_children.Contains(childNode))
		{
			_children.Add(childNode);
		}
	}

	public void AddChildren(List<OWTreeNode<T>> childrenlist)
	{
		for (int i = 0; i < childrenlist.Count; i++)
		{
			AddChild(childrenlist[i]);
		}
	}
}
