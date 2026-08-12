using UnityEngine;

public interface IObservable
{
	void LoseFocus();

	void Observe(RaycastHit raycastHit, Vector3 raycastOrigin);
}
