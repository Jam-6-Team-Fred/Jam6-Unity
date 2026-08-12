using UnityEngine;

[ExecuteInEditMode]
public class CustomAspectRatio : MonoBehaviour
{
	[SerializeField]
	private float _aspectRatio = 1.3333334f;

	private void Awake()
	{
		GetComponent<Camera>().aspect = _aspectRatio;
	}
}
