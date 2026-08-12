using UnityEngine;

public class OverrideColliderSwapDelay : MonoBehaviour
{
	[SerializeField]
	private bool _ignoreSwapDelay = true;

	private void OnEnable()
	{
		OWCollider addComponent = base.gameObject.GetAddComponent<OWCollider>();
		if (_ignoreSwapDelay)
		{
			addComponent.IgnorePhysicsSwapDelay();
		}
		Object.Destroy(this);
	}
}
