using UnityEngine;

public class NormalTexAnimator : MonoBehaviour
{
	[SerializeField]
	private Vector2 _MainTexDirection = Vector2.up;

	[SerializeField]
	private float _MainTexRate = 0.05f;

	private float _maintexoffset;

	[SerializeField]
	private Vector2 _NormalTexDirection = Vector2.up;

	[SerializeField]
	private float _NormalTexRate = 0.05f;

	private float _normaltexoffset;

	[SerializeField]
	private Vector2 _IlluminTexDirection = Vector2.up;

	[SerializeField]
	private float _IlluminTexRate = 0.05f;

	private float _illumintexoffset;

	private Material _material;

	private void Awake()
	{
		_material = GetComponent<Renderer>().material;
		_maintexoffset = Random.value;
		_normaltexoffset = Random.value;
		_illumintexoffset = Random.value;
	}

	private void Update()
	{
		_maintexoffset += _MainTexRate * _material.mainTextureScale.y * Time.deltaTime;
		_material.SetTextureOffset("_MainTex", _MainTexDirection * _maintexoffset);
		if (_maintexoffset > 1f)
		{
			_maintexoffset = 0f;
		}
		else if (_maintexoffset < 0f)
		{
			_maintexoffset = 1f;
		}
		_normaltexoffset += _NormalTexRate * _material.mainTextureScale.y * Time.deltaTime;
		_material.SetTextureOffset("_MainTex", _NormalTexDirection * _normaltexoffset);
		if (_normaltexoffset > 1f)
		{
			_normaltexoffset = 0f;
		}
		else if (_normaltexoffset < 0f)
		{
			_normaltexoffset = 1f;
		}
		_illumintexoffset += _IlluminTexRate * _material.mainTextureScale.y * Time.deltaTime;
		_material.SetTextureOffset("_Illum", _IlluminTexDirection * _illumintexoffset);
		if (_illumintexoffset > 1f)
		{
			_illumintexoffset = 0f;
		}
		else if (_illumintexoffset < 0f)
		{
			_illumintexoffset = 1f;
		}
	}
}
