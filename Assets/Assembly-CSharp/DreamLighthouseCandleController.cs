using UnityEngine;

public class DreamLighthouseCandleController : MonoBehaviour
{
	[SerializeField]
	private DreamCandle[] _candles;

	[SerializeField]
	private GameObject _doorLightShaft;

	private LighthouseController _lighthouseController;

	private void Awake()
	{
		for (int i = 0; i < _candles.Length; i++)
		{
			_candles[i].OnLitStateChanged += new OWEvent.OWCallback(OnCandleLitStateChanged);
		}
	}

	private void Start()
	{
		if (Locator.GetRingWorldController() != null && Locator.GetRingWorldController().GetLighthouseController() != null)
		{
			_lighthouseController = Locator.GetRingWorldController().GetLighthouseController();
			for (int i = 0; i < _candles.Length; i++)
			{
				_lighthouseController.SetMuralRoomLightState(i, _candles[i].StartsLit());
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _candles.Length; i++)
		{
			_candles[i].OnLitStateChanged -= new OWEvent.OWCallback(OnCandleLitStateChanged);
		}
	}

	private void OnCandleLitStateChanged()
	{
		if (_lighthouseController == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < _candles.Length; i++)
		{
			_lighthouseController.SetMuralRoomLightState(i, _candles[i].IsLit());
			if (_candles[i].IsLit())
			{
				num++;
			}
		}
		_lighthouseController.CheckMuralRoomSecretDoor();
		bool flag = num > 3;
		_doorLightShaft.SetActive(flag);
		_lighthouseController.SetDoorLightShaftActive(flag);
	}
}
