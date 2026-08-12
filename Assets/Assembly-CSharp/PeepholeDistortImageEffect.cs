using UnityEngine;

[ExecuteInEditMode]
public class PeepholeDistortImageEffect : MonoBehaviour
{
	private Material _material;

	[Range(0.01f, 5f)]
	[SerializeField]
	private float _exponent = 1f;

	[Range(0f, 4f)]
	[Tooltip("Intensity multiplier on the x-axis. Set it to 0 to disable distortion on this axis.")]
	[SerializeField]
	private float _xMultiplier = 1f;

	[Range(0f, 4f)]
	[Tooltip("Intensity multiplier on the y-axis. Set it to 0 to disable distortion on this axis.")]
	[SerializeField]
	private float _yMultiplier = 1f;

	private Material material
	{
		get
		{
			if (_material == null)
			{
				_material = new Material(shader);
			}
			return _material;
		}
	}

	private Shader shader => Shader.Find("Hidden/PeepholeDistort");

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetFloat("_xMul", _xMultiplier);
		material.SetFloat("_yMul", _yMultiplier);
		material.SetFloat("_exp", _exponent);
		Graphics.Blit(source, destination, material);
	}
}
