using System;
using UnityEngine;

public class SignalDishController : MonoBehaviour
{
	[SerializeField]
	private NomaiInterfaceSlot[] _slots;

	[SerializeField]
	private AstroObject.Name[] _astroObjectTargets;

	[SerializeField]
	private SignalDishRingController[] _rings;

	[SerializeField]
	private OWTriggerVolume _signalAudioTrigger;

	[SerializeField]
	private OWAudioSource _signalAudioSource;

	private AstroObject.Name _currentTarget;

	private bool _playerInTrigger;

	private void Awake()
	{
		for (int i = 0; i < _slots.Length; i++)
		{
			_slots[i].OnSlotActivated += OnSlotActivated;
			_slots[i].OnSlotDeactivated += OnSlotDeactivated;
		}
		_signalAudioTrigger.OnEntry += OnEnterAudioTrigger;
		_signalAudioTrigger.OnExit += OnExitAudioTrigger;
		if (_slots.Length != _astroObjectTargets.Length)
		{
			Debug.LogError("Number of slots must match number of astro object targets.", this);
			Debug.Break();
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_signalAudioTrigger.OnEntry -= OnEnterAudioTrigger;
		_signalAudioTrigger.OnExit -= OnExitAudioTrigger;
		for (int i = 0; i < _slots.Length; i++)
		{
			_slots[i].OnSlotActivated -= OnSlotActivated;
			_slots[i].OnSlotDeactivated -= OnSlotDeactivated;
		}
	}

	private void Update()
	{
		bool flag = true;
		for (int i = 0; i < _rings.Length; i++)
		{
			if (!_rings[i].IsAlignedWithTarget())
			{
				flag = false;
				break;
			}
		}
		bool flag2 = _signalAudioSource.isPlaying && !_signalAudioSource.IsFadingOut();
		bool flag3 = flag && _playerInTrigger && _currentTarget != AstroObject.Name.None;
		if (flag3 && !flag2)
		{
			_signalAudioSource.clip = GetSignalClip(_currentTarget);
			_signalAudioSource.FadeIn(1f);
		}
		else if (!flag3 && flag2)
		{
			_signalAudioSource.FadeOut(1f);
		}
		if (!_playerInTrigger)
		{
			base.enabled = false;
		}
	}

	private AudioClip GetSignalClip(AstroObject.Name astroName)
	{
		switch (astroName)
		{
		case AstroObject.Name.Sun:
			return Locator.GetAudioManager().GetSingleAudioClip(AudioType.Sun_Ambience_LP);
		case AstroObject.Name.GiantsDeep:
			return Locator.GetAudioManager().GetSingleAudioClip(AudioType.GD_Tornado_LP);
		case AstroObject.Name.BrittleHollow:
			return Locator.GetAudioManager().GetSingleAudioClip(AudioType.BlackHoleAmbience_LP);
		case AstroObject.Name.Eye:
			return null;
		default:
			return null;
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		int num = Array.IndexOf(_slots, slot);
		_currentTarget = _astroObjectTargets[num];
		for (int i = 0; i < _rings.Length; i++)
		{
			_rings[i].SetAstroObjectTarget(_currentTarget);
		}
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		_currentTarget = AstroObject.Name.None;
		for (int i = 0; i < _rings.Length; i++)
		{
			_rings[i].RemoveTarget();
		}
	}

	private void OnEnterAudioTrigger(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			_playerInTrigger = true;
			base.enabled = true;
		}
	}

	private void OnExitAudioTrigger(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			_playerInTrigger = false;
		}
	}
}
