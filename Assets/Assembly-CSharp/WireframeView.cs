using UnityEngine;

[ExecuteInEditMode]
public class WireframeView : MonoBehaviour
{
	[SerializeField]
	private bool _viewAsWireframe;

	private void OnPreRender()
	{
		GL.wireframe = _viewAsWireframe;
	}

	private void OnPostRender()
	{
		GL.wireframe = false;
	}
}
