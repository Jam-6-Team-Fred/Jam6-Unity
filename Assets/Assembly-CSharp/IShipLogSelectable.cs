using UnityEngine;

public interface IShipLogSelectable
{
	bool CheckPointCollision(Vector3 point);

	void OnGainFocus();

	void OnLoseFocus();

	void OnSelect();

	void MarkAsRead();
}
