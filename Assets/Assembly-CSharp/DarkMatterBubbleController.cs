using UnityEngine;

public class DarkMatterBubbleController : MonoBehaviour
{
	private Renderer _effectBubbleRenderer;

	private int _propID_Fade;

	[SerializeField]
	private HazardDetector _hazardDetector;

	[SerializeField]
	private float _fadeLength = 1f;

	private bool _inDarkMatter;

	private float _fade;

	private Vector3 _prevDarkMatterDir;

	private void Awake()
	{
		_effectBubbleRenderer = GetComponentInChildren<Renderer>();
		_propID_Fade = Shader.PropertyToID("_Fade");
		_inDarkMatter = false;
		_fade = 0f;
		_prevDarkMatterDir = Vector3.zero;
		_effectBubbleRenderer.material.SetFloat(_propID_Fade, _fade);
		base.enabled = false;
		_effectBubbleRenderer.enabled = false;
		_hazardDetector.OnHazardsUpdated += OnHazardsUpdated;
	}

	private void OnDestroy()
	{
		_hazardDetector.OnHazardsUpdated -= OnHazardsUpdated;
	}

	private void OnHazardsUpdated()
	{
		_inDarkMatter = _hazardDetector.InHazardType(HazardVolume.HazardType.DARKMATTER);
		if (_inDarkMatter && !base.enabled)
		{
			_fade = 0f;
			_effectBubbleRenderer.material.SetFloat(_propID_Fade, _fade);
			base.enabled = true;
			_effectBubbleRenderer.enabled = true;
		}
	}

	private void LateUpdate()
	{
		_fade = Mathf.MoveTowards(_fade, _inDarkMatter ? 1f : 0f, Time.deltaTime / _fadeLength);
		Vector3 vector = _prevDarkMatterDir;
		if (_inDarkMatter)
		{
			vector = _hazardDetector.GetDarkMatterVolume().transform.position - _effectBubbleRenderer.transform.position;
		}
		_effectBubbleRenderer.transform.rotation = Quaternion.LookRotation(vector, base.transform.parent.up) * Quaternion.Euler(90f, 0f, 0f);
		_effectBubbleRenderer.material.SetFloat(_propID_Fade, _fade);
		if (!_inDarkMatter && _fade <= 0f)
		{
			base.enabled = false;
			_effectBubbleRenderer.enabled = false;
		}
		_prevDarkMatterDir = vector;
	}
}
