using UnityEngine;

public abstract class VisibilityTracker : MonoBehaviour
{
	public abstract bool IsVisibleUsingCameraFrustum();

	public abstract bool IsVisible();

	public abstract bool IsVisibleToProbe(OWCamera camera);

	public abstract bool IsPointInside(Vector3 worldPos);
}
