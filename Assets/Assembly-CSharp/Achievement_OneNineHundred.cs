using UnityEngine;

public class Achievement_OneNineHundred : MonoBehaviour
{
	[SerializeField]
	private DreamCandle _candle;

	[SerializeField]
	private ParticleSystem[] _particles;

	[SerializeField]
	private GameObject[] _candleObjects;

	[SerializeField]
	private GameObject _korokObject;

	[SerializeField]
	private OWAudioSource _musicSource;

	private bool _achievementGained;

	private void Awake()
	{
		_candle.OnLitStateChanged += new OWEvent.OWCallback(OnLitStateChanged);
	}

	private void Start()
	{
		_korokObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		_candle.OnLitStateChanged -= new OWEvent.OWCallback(OnLitStateChanged);
	}

	public void OnLitStateChanged()
	{
		if (!_achievementGained && _candle.IsLit())
		{
			for (int i = 0; i < _particles.Length; i++)
			{
				_particles[i].Play();
			}
			for (int j = 0; j < _candleObjects.Length; j++)
			{
				_candleObjects[j].SetActive(value: false);
			}
			_korokObject.SetActive(value: true);
			_musicSource.PlayDelayed(1f);
			Achievements.Earn(Achievements.Type.ONE_NINE);
			_achievementGained = true;
		}
	}
}
