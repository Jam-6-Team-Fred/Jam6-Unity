using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ClearCameraOnStart : MonoBehaviour
{
	private void Start()
	{
		Camera component = GetComponent<Camera>();
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = component.targetTexture;
		GL.Clear(clearDepth: true, clearColor: true, Color.black);
		RenderTexture.active = active;
	}
}
