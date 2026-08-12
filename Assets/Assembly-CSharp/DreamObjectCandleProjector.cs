using UnityEngine;

public class DreamObjectCandleProjector : MonoBehaviour
{
	[SerializeField]
	private DreamObjectProjection[] _projections = new DreamObjectProjection[0];

	[Space]
	[SerializeField]
	private DreamCandle[] _controllingCandles = new DreamCandle[0];

	[SerializeField]
	private DreamCandle[] _controllingCandlesBack = new DreamCandle[0];

	[SerializeField]
	private OWTriggerVolume _blockingVolume;

	private bool _lit;

	private bool _projected;

	private bool _blocked;

	private bool _candleProjectionStatus;

	protected virtual void Awake()
	{
		_blocked = false;
		_candleProjectionStatus = true;
		if (_blockingVolume != null)
		{
			_blockingVolume.OnEntry += OnEntry;
			_blockingVolume.OnExit += OnExit;
		}
	}

	protected virtual void Start()
	{
		base.enabled = false;
		_lit = false;
		for (int i = 0; i < _controllingCandles.Length; i++)
		{
			if (_controllingCandles[i].StartsLit())
			{
				_lit = true;
			}
		}
		_projected = _lit;
		for (int j = 0; j < _projections.Length; j++)
		{
			_projections[j].SetVisibleImmediate(_lit, forceUpdate: true);
		}
		for (int k = 0; k < _controllingCandles.Length; k++)
		{
			_controllingCandles[k].OnLitStateChanged += new OWEvent.OWCallback(OnCandleStateChanged);
		}
		for (int l = 0; l < _controllingCandlesBack.Length; l++)
		{
			_controllingCandlesBack[l].OnLitStateChanged += new OWEvent.OWCallback(OnBackCandleStateChanged);
		}
	}

	private void OnCandleStateChanged()
	{
		if (_lit)
		{
			for (int i = 0; i < _controllingCandles.Length; i++)
			{
				if (_controllingCandles[i].IsLit())
				{
					return;
				}
			}
			for (int j = 0; j < _controllingCandlesBack.Length; j++)
			{
				_controllingCandlesBack[j].SetLit(lit: false, playAudio: false, instant: true);
			}
			_lit = false;
			OnFade();
			return;
		}
		for (int k = 0; k < _controllingCandles.Length; k++)
		{
			if (_controllingCandles[k].IsLit())
			{
				_lit = true;
				for (int l = 0; l < _controllingCandlesBack.Length; l++)
				{
					_controllingCandlesBack[l].SetLit(lit: true, playAudio: false);
				}
				if (!_blocked)
				{
					OnProject();
				}
				break;
			}
		}
	}

	private void OnBackCandleStateChanged()
	{
		if (_lit)
		{
			for (int i = 0; i < _controllingCandlesBack.Length; i++)
			{
				if (_controllingCandlesBack[i].IsLit())
				{
					return;
				}
			}
			for (int j = 0; j < _controllingCandles.Length; j++)
			{
				_controllingCandles[j].SetLit(lit: false, playAudio: false, instant: true);
			}
			_lit = false;
			OnFade();
			return;
		}
		for (int k = 0; k < _controllingCandlesBack.Length; k++)
		{
			if (_controllingCandlesBack[k].IsLit())
			{
				_lit = true;
				for (int l = 0; l < _controllingCandles.Length; l++)
				{
					_controllingCandles[l].SetLit(lit: true, playAudio: false);
				}
				if (!_blocked)
				{
					OnProject();
				}
				break;
			}
		}
	}

	public void OnCandlesProjectionChange(bool candlesProjected)
	{
		_candleProjectionStatus = candlesProjected;
		if (candlesProjected)
		{
			if (!_blocked && _lit)
			{
				OnProject();
			}
		}
		else if (_lit)
		{
			OnFade();
		}
	}

	private void OnFade()
	{
		_projected = false;
		for (int i = 0; i < _projections.Length; i++)
		{
			_projections[i].SetVisible(visible: false);
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_blocked = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_blocked = false;
			if (_lit && !_projected && _candleProjectionStatus)
			{
				OnProject();
			}
		}
	}

	private void OnProject()
	{
		if (!_lit)
		{
			Debug.LogError("Something went horribly wrong with DreamObjectCandleProjector");
		}
		_projected = true;
		for (int i = 0; i < _projections.Length; i++)
		{
			_projections[i].SetVisible(visible: true);
		}
	}
}
