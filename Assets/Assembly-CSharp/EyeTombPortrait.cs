using UnityEngine;

public class EyeTombPortrait : MonoBehaviour
{
	[SerializeField]
	private DreamCandle _dreamCandle;

	[SerializeField]
	private GameObject _occupiedPortrait;

	[SerializeField]
	private GameObject _emptyPortrait;

	private float _swapTime;

	public OWEvent OnSwapPortrait = new OWEvent(1);

	private void Awake()
	{
		_dreamCandle.OnLitStateChanged += new OWEvent.OWCallback(OnLitStateChanged);
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_dreamCandle.OnLitStateChanged -= new OWEvent.OWCallback(OnLitStateChanged);
	}

	private void OnLitStateChanged()
	{
		if (!_dreamCandle.IsLit())
		{
			base.enabled = true;
			_swapTime = Time.time + 0.5f;
			GlobalMessenger<float, float>.FireEvent("FlickerOffAndOn", 0.5f, 1f);
		}
	}

	private void Update()
	{
		if (Time.time > _swapTime)
		{
			base.enabled = false;
			OnSwapPortrait.Invoke();
			if (_emptyPortrait != null)
			{
				_emptyPortrait.SetActive(value: true);
			}
			if (_occupiedPortrait != null)
			{
				_occupiedPortrait.SetActive(value: false);
			}
		}
	}
}
