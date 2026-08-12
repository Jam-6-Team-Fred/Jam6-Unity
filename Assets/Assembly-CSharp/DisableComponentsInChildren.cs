using UnityEngine;

public class DisableComponentsInChildren : MonoBehaviour
{
	public bool _disableColliders;

	public bool _disableMeshRenderers;

	private bool _disabled;

	private void Update()
	{
		if (_disabled)
		{
			return;
		}
		if (_disableColliders)
		{
			Component[] componentsInChildren = GetComponentsInChildren<Collider>();
			componentsInChildren = componentsInChildren;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Collider)componentsInChildren[i]).enabled = false;
			}
		}
		if (_disableMeshRenderers)
		{
			Component[] componentsInChildren = GetComponentsInChildren<Renderer>();
			componentsInChildren = componentsInChildren;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Renderer)componentsInChildren[i]).enabled = false;
			}
		}
		_disabled = true;
	}
}
