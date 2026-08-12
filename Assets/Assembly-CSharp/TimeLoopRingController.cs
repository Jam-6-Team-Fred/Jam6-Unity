using UnityEngine;

public class TimeLoopRingController : MonoBehaviour
{
	[SerializeField]
	private NomaiInterfaceSlot _onSlot;

	[SerializeField]
	private NomaiInterfaceSlot _offSlot;

	[SerializeField]
	private OWRigidbody _ringBody;

	[SerializeField]
	private ForceVolume[] _alignmentVolumes;

	[SerializeField]
	private OWAudioSource _audioSource;

	private bool _cacheOrigVelocity = true;

	private Vector3 _origAngularVelocity;

	private bool _alignment = true;

	private bool _running = true;

	private Vector3 _targetAngularVelocity;

	private void Awake()
	{
		_onSlot.OnSlotActivated += OnSlotActivated;
		_onSlot.OnSlotDeactivated += OnSlotDeactivated;
		_offSlot.OnSlotActivated += OnSlotActivated;
		_offSlot.OnSlotDeactivated += OnSlotDeactivated;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_onSlot.OnSlotActivated -= OnSlotActivated;
		_onSlot.OnSlotDeactivated -= OnSlotDeactivated;
		_offSlot.OnSlotActivated -= OnSlotActivated;
		_offSlot.OnSlotDeactivated -= OnSlotDeactivated;
	}

	private void FixedUpdate()
	{
		Vector3 vector = Vector3.MoveTowards(_ringBody.GetAngularVelocity(), _targetAngularVelocity, Time.deltaTime * 0.1f);
		_ringBody.AddAngularVelocityChange(vector - _ringBody.GetAngularVelocity());
		float magnitude = (_targetAngularVelocity - _ringBody.GetAngularVelocity()).magnitude;
		if (!_running && _alignment && magnitude < 0.05f)
		{
			SetAlignment(align: false);
		}
		else if (_running && !_alignment && _ringBody.GetAngularVelocity().magnitude > 0.3f)
		{
			SetAlignment(align: true);
		}
		if (magnitude < 0.0001f)
		{
			base.enabled = false;
		}
	}

	private void SetAlignment(bool align)
	{
		_alignment = align;
		for (int i = 0; i < _alignmentVolumes.Length; i++)
		{
			_alignmentVolumes[i].SetVolumeActivation(_alignment);
		}
	}

	private void SetRunning(bool running)
	{
		if (_running != running)
		{
			if (_running && _cacheOrigVelocity)
			{
				_cacheOrigVelocity = false;
				_origAngularVelocity = _ringBody.GetAngularVelocity();
			}
			_running = running;
			_targetAngularVelocity = (_running ? _origAngularVelocity : Vector3.zero);
			_audioSource.PlayOneShot(_running ? AudioType.NomaiPowerOn : AudioType.NomaiPowerOff);
			base.enabled = true;
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		if (slot == _onSlot)
		{
			SetRunning(running: true);
		}
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		if (slot == _onSlot)
		{
			SetRunning(running: false);
		}
	}
}
