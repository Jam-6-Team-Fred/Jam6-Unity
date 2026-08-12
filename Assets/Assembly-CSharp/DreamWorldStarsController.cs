using UnityEngine;

public class DreamWorldStarsController : MonoBehaviour
{
	private readonly int _propID_Brightness = Shader.PropertyToID("_Brightness");

	[SerializeField]
	private Renderer _renderer;

	private RingWorldFlickerController _flickerController;

	private float _baseBrightness;

	private void Awake()
	{
		_baseBrightness = _renderer.material.GetFloat(_propID_Brightness);
		base.enabled = false;
	}

	private void Update()
	{
		if (_flickerController == null)
		{
			base.enabled = false;
		}
		else if (_flickerController.IsFlickering())
		{
			float flickerScale = _flickerController.GetFlickerScale();
			_renderer.material.SetFloat(_propID_Brightness, _baseBrightness * flickerScale * flickerScale);
		}
		else
		{
			_renderer.material.SetFloat(_propID_Brightness, _baseBrightness);
			base.enabled = false;
		}
	}

	public void StartFlicker(RingWorldFlickerController flickerController)
	{
		_flickerController = flickerController;
		base.enabled = true;
	}
}
