using UnityEngine;

public class ThrusterFlameColorSwapper : MonoBehaviour
{
	[SerializeField]
	private Renderer[] _thrusterRenderers = new Renderer[0];

	[SerializeField]
	private Texture2D _thrusterFlameSwapTex;

	[SerializeField]
	private Light[] _thrusterLights = new Light[0];

	[SerializeField]
	private Color _thrusterLightsSwapColor = Color.white;

	private MaterialPropertyBlock _matPropBlock;

	private Color _baseLightColor;

	private bool _swapped;

	private void Awake()
	{
		_matPropBlock = new MaterialPropertyBlock();
		_matPropBlock.SetTexture("_MainTex", _thrusterFlameSwapTex);
		if (_thrusterLights.Length != 0)
		{
			_baseLightColor = _thrusterLights[0].color;
		}
	}

	public void SetFlameColor(bool swapped)
	{
		if (swapped && !_swapped)
		{
			for (int i = 0; i < _thrusterRenderers.Length; i++)
			{
				_thrusterRenderers[i].SetPropertyBlock(_matPropBlock);
			}
			for (int j = 0; j < _thrusterLights.Length; j++)
			{
				_thrusterLights[j].color = _thrusterLightsSwapColor;
			}
		}
		else if (!swapped && _swapped)
		{
			for (int k = 0; k < _thrusterRenderers.Length; k++)
			{
				_thrusterRenderers[k].SetPropertyBlock(null);
			}
			for (int l = 0; l < _thrusterLights.Length; l++)
			{
				_thrusterLights[l].color = _baseLightColor;
			}
		}
		_swapped = swapped;
	}
}
