using UnityEngine;

public class CloudTextureController : MonoBehaviour
{
	[SerializeField]
	private Texture2D _cloudTex;

	[SerializeField]
	private float _startAlpha = 1f;

	private void Awake()
	{
		GetComponent<Renderer>().material.mainTexture = _cloudTex;
		Color color = GetComponent<Renderer>().material.color;
		color.a = _startAlpha;
	}

	private void Update()
	{
	}
}
