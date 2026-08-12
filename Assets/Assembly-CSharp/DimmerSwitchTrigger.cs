using System.Collections;
using UnityEngine;

public class DimmerSwitchTrigger : MonoBehaviour
{
	[SerializeField]
	private Light _light;

	[SerializeField]
	private Renderer _lightSourceRenderer;

	private Texture _illumTex;

	private void Start()
	{
		_light.enabled = false;
		if (_lightSourceRenderer != null)
		{
			_illumTex = _lightSourceRenderer.material.GetTexture("_Illum");
			_lightSourceRenderer.material.SetTexture("_Illum", (Texture)Resources.Load("Textures/invisibleTexture"));
		}
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (!_light.enabled && hitCollider.tag == "PlayerDetector")
		{
			if (_lightSourceRenderer != null)
			{
				_lightSourceRenderer.material.SetTexture("_Illum", _illumTex);
			}
			_light.enabled = true;
			StartCoroutine(FadeIntensity(0f, _light.intensity, 2f));
		}
	}

	private IEnumerator FadeIntensity(float startIntensity, float finalIntensity, float duration)
	{
		float initTime = Time.time;
		while (true)
		{
			float num = Mathf.Clamp01((Time.time - initTime) / duration);
			_light.intensity = Mathf.Lerp(startIntensity, finalIntensity, num);
			if (!(num >= 1f))
			{
				yield return null;
				continue;
			}
			break;
		}
	}
}
