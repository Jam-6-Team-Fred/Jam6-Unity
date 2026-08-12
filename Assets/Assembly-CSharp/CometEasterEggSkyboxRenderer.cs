using UnityEngine;

public class CometEasterEggSkyboxRenderer : MonoBehaviour
{
	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private SkyboxRenderer _skyboxRenderer;

	private bool _revealed;

	private float _fadeStartTime;

	private void Awake()
	{
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
		_renderer.material.SetColor("_Color", new Color(0f, 0f, 0f, 1f));
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void OnEnterDreamWorld()
	{
		_renderer.enabled = _revealed;
	}

	private void OnExitDreamWorld()
	{
		_renderer.enabled = false;
	}

	public void RevealComet(float delay)
	{
		_renderer.enabled = true;
		_skyboxRenderer.enabled = true;
		_revealed = true;
		_fadeStartTime = Time.timeSinceLevelLoad + delay;
		base.enabled = true;
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.timeSinceLevelLoad - _fadeStartTime) / 3f);
		float num2 = num * num;
		_renderer.material.SetColor("_Color", new Color(num2, num2, num2, 1f));
		if (num >= 1f)
		{
			base.enabled = false;
		}
	}
}
