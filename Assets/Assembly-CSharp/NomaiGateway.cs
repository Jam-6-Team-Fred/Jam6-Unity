using UnityEngine;

public class NomaiGateway : SectoredMonoBehaviour
{
	[SerializeField]
	private bool _startOpen;

	[SerializeField]
	private bool _openFromBothEnds = true;

	[SerializeField]
	private float _slabInterval = 2f;

	[SerializeField]
	private NomaiInterfaceSlot _openSlot;

	[SerializeField]
	private NomaiInterfaceSlot _closeSlot;

	[SerializeField]
	private NomaiInterfaceOrb _orb;

	[SerializeField]
	private NomaiGatewaySlab[] _slabs;

	[SerializeField]
	private OWAudioSource _audioSource;

	private bool _open;

	private float _slotActivatedTime;

	private int _iSlabsInMotion;

	protected override void Awake()
	{
		base.Awake();
		_audioSource = GetComponentInChildren<OWAudioSource>();
		_openSlot.OnSlotActivated += OpenGate;
		_closeSlot.OnSlotActivated += CloseGate;
	}

	private void Start()
	{
		if (_startOpen)
		{
			_open = true;
			_orb.transform.position = _openSlot.transform.position;
			for (int i = 0; i < _slabs.Length; i++)
			{
				_slabs[i].OpenImmediate();
			}
		}
		for (int j = 0; j < _slabs.Length; j++)
		{
			_slabs[j].OnGatewaySlabStart += OnGatewaySlabStart;
			_slabs[j].OnGatewaySlabStop += OnGatewaySlabStop;
		}
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_openSlot.OnSlotActivated -= OpenGate;
		_closeSlot.OnSlotActivated -= CloseGate;
		for (int i = 0; i < _slabs.Length; i++)
		{
			if (_slabs[i] != null)
			{
				_slabs[i].OnGatewaySlabStart -= OnGatewaySlabStart;
				_slabs[i].OnGatewaySlabStop -= OnGatewaySlabStop;
			}
		}
	}

	public float GetOpenFraction()
	{
		float num = 0f;
		for (int i = 0; i < _slabs.Length; i++)
		{
			num += _slabs[i].GetOpenFraction();
		}
		return num / (float)_slabs.Length;
	}

	private void FixedUpdate()
	{
		int num = (int)((Time.time - _slotActivatedTime) / _slabInterval);
		if (num >= _slabs.Length || _slabs[num].IsOpen() == _open)
		{
			return;
		}
		_slabs[num].SetOpen(_open);
		if (_openFromBothEnds)
		{
			int num2 = _slabs.Length - 1 - num;
			_slabs[num2].SetOpen(_open);
			if (Mathf.Abs(num - num2) <= 1)
			{
				base.enabled = false;
			}
		}
		else if (num == _slabs.Length - 1)
		{
			base.enabled = false;
		}
	}

	private void OpenGate(NomaiInterfaceSlot slot)
	{
		if (!_open)
		{
			_open = true;
			_slotActivatedTime = Time.time;
			base.enabled = true;
		}
	}

	private void CloseGate(NomaiInterfaceSlot slot)
	{
		if (_open)
		{
			_open = false;
			_slotActivatedTime = Time.time;
			base.enabled = true;
		}
	}

	private void OnGatewaySlabStart()
	{
		if (_audioSource != null)
		{
			AudioClip audioClip = _audioSource.PlayOneShot(AudioType.NomaiDoorStartBig);
			if (_iSlabsInMotion == 0)
			{
				_audioSource.PlayDelayed(audioClip.length);
			}
		}
		_iSlabsInMotion++;
	}

	private void OnGatewaySlabStop()
	{
		if (_audioSource != null)
		{
			if (_iSlabsInMotion == 1)
			{
				_audioSource.Stop();
			}
			_audioSource.PlayOneShot(AudioType.NomaiDoorStopBig);
		}
		_iSlabsInMotion--;
	}
}
