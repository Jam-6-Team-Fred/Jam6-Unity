using UnityEngine;

public interface IItemDropTarget
{
	Transform GetItemDropTargetTransform(GameObject raycastTarget);

	void AddDroppedItem(GameObject dropTarget, OWItem item);
}
