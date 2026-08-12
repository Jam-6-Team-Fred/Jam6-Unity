using UnityEngine;

[RequireComponent(typeof(InteractVolume))]
public class HatchController : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _triggerVolume;

	[SerializeField]
	private GameObject _hatchObject;

	[SerializeField]
	private Transform _hatch;

	[SerializeField]
	private float _hatchOpenTime;

	[SerializeField]
	private Vector3 _hatchClosedRotation = new Vector3(180f, 0f, 0f);

	[SerializeField]
	private Vector3 _hatchOpenedRotation = Vector3.zero;

	private bool _hatchOpening;

	private bool _hatchClosing;

	private float _timeSinceRotation;

	private Quaternion _hatchClosedQuaternion;

	private Quaternion _hatchOpenedQuaternion;

	private SingleInteractionVolume _interactVolume;

	private bool _isPlayerInShip;

	private ShipAudioController _shipAudioController;

	private void Awake()
	{
		_interactVolume = this.GetRequiredComponent<SingleInteractionVolume>();
		_interactVolume.OnPressInteract += OnPressInteract;
		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;
		_hatchObject.SetActive(value: false);
		_hatchClosedQuaternion = Quaternion.Euler(_hatchClosedRotation);
		_hatchOpenedQuaternion = Quaternion.Euler(_hatchOpenedRotation);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEntry;
		_triggerVolume.OnExit -= OnExit;
		_interactVolume.OnPressInteract -= OnPressInteract;
	}

	private void FixedUpdate()
	{
		if (_hatchOpening && _hatch.localRotation != _hatchOpenedQuaternion)
		{
			if (_timeSinceRotation == _hatchOpenTime)
			{
				_timeSinceRotation = 0f;
			}
			_hatch.localRotation = Quaternion.Lerp(_hatchClosedQuaternion, _hatchOpenedQuaternion, _timeSinceRotation / _hatchOpenTime);
			_timeSinceRotation += Time.deltaTime;
			if (_timeSinceRotation >= _hatchOpenTime)
			{
				_hatch.localRotation = _hatchOpenedQuaternion;
				_timeSinceRotation = _hatchOpenTime;
				_hatchOpening = false;
				base.enabled = false;
			}
		}
		else if (_hatchClosing && _hatch.localRotation != _hatchClosedQuaternion)
		{
			if (_timeSinceRotation == _hatchOpenTime)
			{
				_timeSinceRotation = 0f;
			}
			_hatch.localRotation = Quaternion.Lerp(_hatchOpenedQuaternion, _hatchClosedQuaternion, _timeSinceRotation / _hatchOpenTime);
			_timeSinceRotation += Time.deltaTime;
			if (_timeSinceRotation >= _hatchOpenTime)
			{
				_hatch.localRotation = _hatchClosedQuaternion;
				_timeSinceRotation = _hatchOpenTime;
				_hatchClosing = false;
				base.enabled = false;
			}
		}
	}

	public bool IsPlayerInShip()
	{
		return _isPlayerInShip;
	}

	private void OnPressInteract()
	{
		OpenHatch();
	}

	private void OpenHatch()
	{
		_hatchObject.SetActive(value: false);
		if (_shipAudioController == null)
		{
			_shipAudioController = Locator.GetShipTransform().GetComponentInChildren<ShipAudioController>();
		}
		_shipAudioController.PlayOpenHatch();
		base.enabled = true;
		_hatchOpening = true;
		_hatchClosing = false;
	}

	private void CloseHatch()
	{
		_hatchObject.SetActive(value: true);
		_interactVolume.ResetInteraction();
		if (_shipAudioController == null)
		{
			_shipAudioController = Locator.GetShipTransform().GetComponentInChildren<ShipAudioController>();
		}
		_shipAudioController.PlayCloseHatch();
		base.enabled = true;
		_hatchClosing = true;
		_hatchOpening = false;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			CloseHatch();
			_isPlayerInShip = true;
			GlobalMessenger.FireEvent("EnterShip");
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_isPlayerInShip = false;
			GlobalMessenger.FireEvent("ExitShip");
		}
	}
}
