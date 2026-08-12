using UnityEngine;

public class ShipLogEntryLocation : MonoBehaviour
{
	[SerializeField]
	private string _entryID = string.Empty;

	[SerializeField]
	private OuterFogWarpVolume _outerFogWarpVolume;

	[SerializeField]
	private bool _isWithinCloakField;

	private ShipLogEntry _entry;

	private Transform _transform;

	private bool _initialized;

	public OWEvent OnLocationDestroyed = new OWEvent(1);

	private void OnValidate()
	{
		if (!_entryID.Equals(string.Empty) && !base.gameObject.name.Equals(_entryID))
		{
			base.gameObject.name = _entryID;
		}
		AstroObject componentInParent = base.transform.GetComponentInParent<AstroObject>();
		if (componentInParent != null)
		{
			if (componentInParent.GetAstroObjectName() == AstroObject.Name.RingWorld && !_isWithinCloakField)
			{
				_isWithinCloakField = true;
			}
			else if (componentInParent.GetAstroObjectName() != AstroObject.Name.RingWorld && _isWithinCloakField)
			{
				_isWithinCloakField = false;
			}
		}
	}

	private void Awake()
	{
		_transform = base.transform;
		Locator.RegisterEntryLocation(this);
	}

	private void Start()
	{
		InitEntry();
	}

	private void OnDisable()
	{
		if (!base.gameObject.activeInHierarchy && !CenterOfTheUniverse.IsUniverseDeactivated())
		{
			OnLocationDestroyed.Invoke();
			Locator.UnregisterEntryLocation(this);
		}
	}

	private void InitEntry()
	{
		if (!_initialized)
		{
			_entry = Locator.GetShipLogManager().GetEntry(_entryID);
			if (_entry == null)
			{
				Debug.LogError("Failed to find ship log entry with ID " + _entryID, this);
			}
			_initialized = true;
		}
	}

	private void OnDestroy()
	{
		Locator.UnregisterEntryLocation(this);
	}

	public string GetEntryID()
	{
		return _entryID;
	}

	public string GetEntryName(bool withLineBreaks)
	{
		InitEntry();
		return _entry.GetName(withLineBreaks).ToUpper();
	}

	public Vector3 GetPosition()
	{
		return _transform.position;
	}

	public Transform GetTransform()
	{
		return _transform;
	}

	public OuterFogWarpVolume GetOuterFogWarpVolume()
	{
		return _outerFogWarpVolume;
	}

	public bool IsWithinCloakField()
	{
		return _isWithinCloakField;
	}

	public void OnSelectLocation()
	{
		if (_isWithinCloakField)
		{
			Locator.GetCloakFieldController().SetReferenceFrameVolumeActive(active: true);
		}
	}

	public void OnDeselectLocation()
	{
		if (_isWithinCloakField)
		{
			Locator.GetCloakFieldController().SetReferenceFrameVolumeActive(active: false);
		}
	}
}
