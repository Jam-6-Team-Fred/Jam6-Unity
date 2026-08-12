using UnityEngine;

public interface IRaycastInteractable
{
	void Observe(RaycastHit hit);

	void UpdateInteractVolume();

	void LoseFocus();

	bool IsFocused();
}
