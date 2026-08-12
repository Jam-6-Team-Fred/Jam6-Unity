using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BlinkingRenderer : MonoBehaviour
{
	[SerializeField]
	private float _onSeconds = 1f;

	[SerializeField]
	private float _offSeconds = 1f;

	private bool _visible;

	private Renderer[] _renderers;

	private float _lastSwitchTime;

	private float _startTime;

	private float _duration = -1f;

	private void Awake()
	{
		_renderers = GetComponentsInChildren<Renderer>();
		if (base.enabled)
		{
			OnEnable();
		}
		else
		{
			OnDisable();
		}
	}

	private void OnEnable()
	{
		_visible = true;
		_startTime = (_lastSwitchTime = Time.time);
		Renderer[] renderers = _renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = true;
		}
	}

	private void OnDisable()
	{
		_visible = false;
		_duration = -1f;
		Renderer[] renderers = _renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = false;
		}
	}

	public void Activate(float duration = -1f)
	{
		_duration = duration;
		base.enabled = true;
	}

	private void Update()
	{
		if (_visible && Time.time > _lastSwitchTime + _onSeconds)
		{
			_visible = false;
			_lastSwitchTime = Time.time;
			Renderer[] renderers = _renderers;
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].enabled = false;
			}
		}
		else if (!_visible && Time.time > _lastSwitchTime + _offSeconds)
		{
			_visible = true;
			_lastSwitchTime = Time.time;
			Renderer[] renderers = _renderers;
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].enabled = true;
			}
		}
		if (_duration > 0f && Time.time > _startTime + _duration)
		{
			base.enabled = false;
		}
	}
}
