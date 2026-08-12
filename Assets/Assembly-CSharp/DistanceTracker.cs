using UnityEngine;

public abstract class DistanceTracker : MonoBehaviour
{
	public abstract Vector3 GetVector();

	public abstract Vector3 GetReverseVector();

	public abstract float GetVectorMagnitude();

	public abstract float GetVectorSquareMagnitude();
}
