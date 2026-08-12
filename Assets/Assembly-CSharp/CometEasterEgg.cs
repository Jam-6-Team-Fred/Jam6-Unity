using UnityEngine;

public class CometEasterEgg : MonoBehaviour
{
	[SerializeField]
	private DreamCandle _dreamCandle;

	[SerializeField]
	private OWAudioSource _purrAudioSource;

	private CometEasterEggSkyboxRenderer _skyboxRenderer;

	private void Awake()
	{
		_dreamCandle.OnLitStateChanged += new OWEvent.OWCallback(OnDreamCandleLitStateChanged);
	}

	private void Start()
	{
		_skyboxRenderer = Locator.GetSkyboxTransform().GetComponentInChildren<CometEasterEggSkyboxRenderer>();
		if (_skyboxRenderer == null)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		_dreamCandle.OnLitStateChanged -= new OWEvent.OWCallback(OnDreamCandleLitStateChanged);
	}

	private void OnDreamCandleLitStateChanged()
	{
		if (_dreamCandle.IsLit() && _skyboxRenderer != null)
		{
			_skyboxRenderer.RevealComet(0.5f);
			_purrAudioSource.PlayDelayed(1f);
		}
	}
}
