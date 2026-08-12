using UnityEngine;

public class TextureAnimatorMultipleMats : MonoBehaviour
{
	[SerializeField]
	private Vector2 _direction = Vector2.up;

	[SerializeField]
	private float _rate = 0.05f;

	private float _offset;

	private Material _material;

	private void Awake()
	{
		_material = GetComponent<Renderer>().materials[1];
		_offset = Random.value;
	}

	private void Update()
	{
		_offset += _rate * _material.mainTextureScale.y * Time.deltaTime;
		_material.SetTextureOffset("_MainTex", _direction * _offset);
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
