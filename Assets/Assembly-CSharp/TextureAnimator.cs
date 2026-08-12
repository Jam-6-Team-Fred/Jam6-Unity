using UnityEngine;

public class TextureAnimator : MonoBehaviour
{
	[SerializeField]
	private Vector2 _direction = Vector2.up;

	[SerializeField]
	private float _rate = 0.05f;

	private float _offset;

	private Material _sandMaterial;

	private void Awake()
	{
		_sandMaterial = GetComponent<Renderer>().material;
		_offset = Random.value;
	}

	private void Update()
	{
		_offset += _rate * _sandMaterial.mainTextureScale.y * Time.deltaTime;
		_sandMaterial.SetTextureOffset("_MainTex", _direction * _offset);
		if (_offset > 1f)
		{
			_offset = 0f;
		}
		else if (_offset < 0f)
		{
			_offset = 1f;
		}
	}
}
