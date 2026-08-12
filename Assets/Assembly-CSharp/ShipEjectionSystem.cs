using UnityEngine;

[RequireComponent(typeof(InteractVolume))]
public class ShipEjectionSystem : MonoBehaviour
{
	private SingleInteractionVolume _interactVolume;

	[SerializeField]
	private ShipDetachableModule _cockpitModule;

	[SerializeField]
	private float _ejectImpulse = 5f;

	[SerializeField]
	private Transform _cover;

	[SerializeField]
	private float _coverMoveTime = 1f;

	[SerializeField]
	private float _secondPressDelay = 0.25f;

	private ShipAudioController _audioController;

	private OWRigidbody _shipBody;

	private float _pressTime;

	private float _coverT;

	private bool _raising;

	private bool _ejectPrimed;

	private bool _ejectPressed;

	private void Awake()
	{
		_interactVolume = GetComponent<SingleInteractionVolume>();
		_shipBody = this.GetAttachedOWRigidbody();
		_interactVolume.OnPressInteract += OnPressInteract;
		_interactVolume.OnLoseFocus += OnLoseFocus;
	}

	private void Start()
	{
		_audioController = Locator.GetShipTransform().GetComponentInChildren<ShipAudioController>();
		_interactVolume.ChangePrompt(UITextType.ShipEjectPrompt);
	}

	private void OnDestroy()
	{
		_interactVolume.OnPressInteract -= OnPressInteract;
		_interactVolume.OnLoseFocus -= OnLoseFocus;
	}

	private void Update()
	{
		if (Time.time >= _pressTime + _secondPressDelay && _raising)
		{
			_interactVolume.ChangePrompt(UITextType.ShipEjectFinalPrompt);
			_ejectPrimed = true;
		}
		if (_raising)
		{
			_coverT = Mathf.Clamp01(_coverT + Time.deltaTime / _coverMoveTime);
		}
		else
		{
			_coverT = Mathf.Clamp01(_coverT - Time.deltaTime / _coverMoveTime);
		}
		_cover.localEulerAngles = new Vector3(0f, Mathf.SmoothStep(0f, -160f, _coverT), 0f);
		if (_coverT <= 0f && !_raising && !_ejectPressed)
		{
			base.enabled = false;
		}
	}

	private void FixedUpdate()
	{
		if (_ejectPressed)
		{
			OWRigidbody oWRigidbody = _cockpitModule.Detach();
			_shipBody.transform.position -= _shipBody.transform.forward * 2f;
			float num = _ejectImpulse;
			if (Locator.GetShipDetector().GetComponent<ShipFluidDetector>().InOceanBarrierZone())
			{
				MonoBehaviour.print("Ship in ocean barrier zone, reducing eject impulse.");
				num = 1f;
			}
			_shipBody.AddLocalImpulse(Vector3.back * num);
			oWRigidbody.AddLocalImpulse(Vector3.forward * num);
			_audioController.PlayEject();
			RumbleManager.PulseEject();
			base.enabled = false;
		}
	}

	private void OnPressInteract()
	{
		if (!_cockpitModule.isDetached)
		{
			if (_ejectPrimed)
			{
				_ejectPressed = true;
				Achievements.Earn(Achievements.Type.WHATS_THIS_BUTTON);
			}
			else
			{
				_raising = true;
				_audioController.PlayRaiseEjectCover();
			}
			_pressTime = Time.time;
			base.enabled = true;
		}
	}

	private void OnLoseFocus()
	{
		_raising = false;
		_ejectPrimed = false;
		_interactVolume.ChangePrompt(UITextType.ShipEjectPrompt);
	}
}
