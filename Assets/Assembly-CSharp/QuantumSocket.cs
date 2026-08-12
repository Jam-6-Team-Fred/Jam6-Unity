using UnityEngine;

public class QuantumSocket : MonoBehaviour
{
	public delegate void SocketEvent(QuantumSocket socket);

	[SerializeField]
	private int _probabilityMultiplier = 1;

	[SerializeField]
	private GameObject _emptySocketObject;

	[SerializeField]
	private GameObject _filledSocketObject;

	[SerializeField]
	private QuantumSocket _linkedSocket;

	[SerializeField]
	private Light[] _lightSources;

	[SerializeField]
	private OWTriggerVolume _occupiedByPlayerVolume;

	private VisibilityObject _visibilityObject;

	private QuantumObject _quantumObject;

	private bool _isPlayerInSocket;

	private float _lastOccupiedTime = -100f;

	private bool _active = true;

	public event SocketEvent OnNewlyObscured;

	private void OnValidate()
	{
		if (_linkedSocket != null && _linkedSocket.GetLinkedSocket() == null)
		{
			_linkedSocket.SetLinkedSocket(this);
		}
	}

	private void Awake()
	{
		base.enabled = false;
		_visibilityObject = GetComponent<VisibilityObject>();
		if (_visibilityObject != null && _visibilityObject.GetSector() != null)
		{
			_visibilityObject.GetSector().OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
			_visibilityObject.SetLightSources(_lightSources);
		}
		if ((bool)_emptySocketObject)
		{
			_emptySocketObject.SetActive(value: true);
		}
		if ((bool)_filledSocketObject)
		{
			_filledSocketObject.SetActive(value: false);
		}
		if (_occupiedByPlayerVolume != null)
		{
			_occupiedByPlayerVolume.OnEntry += OnPlayerEnterSocket;
			_occupiedByPlayerVolume.OnExit += OnPlayerExitSocket;
		}
		if (_lightSources.Length != 0 && _visibilityObject == null)
		{
			Debug.LogError("Light sources won't do anything without a visibility object", this);
			Debug.Break();
		}
	}

	private void OnDestroy()
	{
		if (_visibilityObject != null)
		{
			_visibilityObject.GetSector().OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		if (_occupiedByPlayerVolume != null)
		{
			_occupiedByPlayerVolume.OnEntry -= OnPlayerEnterSocket;
			_occupiedByPlayerVolume.OnExit -= OnPlayerExitSocket;
		}
	}

	public bool IsActive()
	{
		return _active;
	}

	public void SetActive(bool active)
	{
		_active = active;
	}

	public virtual bool IsOccupied()
	{
		bool flag = _linkedSocket != null && _linkedSocket.GetOccupant() != null && !_linkedSocket.GetOccupant().IsLocked();
		return _quantumObject != null || _isPlayerInSocket || flag;
	}

	public QuantumSocket GetLinkedSocket()
	{
		return _linkedSocket;
	}

	public VisibilityObject GetVisibilityObject()
	{
		return _visibilityObject;
	}

	public void SetLinkedSocket(QuantumSocket socket)
	{
		_linkedSocket = socket;
	}

	public int GetProbabilityMultiplier()
	{
		return _probabilityMultiplier;
	}

	public QuantumObject GetOccupant()
	{
		return _quantumObject;
	}

	public void CollapseOccupant(bool forceCollapse = false)
	{
		if (_quantumObject != null)
		{
			if (forceCollapse)
			{
				_quantumObject.ForceCollapse();
			}
			else
			{
				_quantumObject.AttemptCollapse();
			}
		}
	}

	public void SetQuantumObject(QuantumObject qObj)
	{
		_quantumObject = qObj;
		if ((bool)_emptySocketObject)
		{
			_emptySocketObject.SetActive(value: false);
		}
		if ((bool)_filledSocketObject)
		{
			_filledSocketObject.SetActive(value: true);
		}
	}

	public void ReleaseQuantumObject()
	{
		_quantumObject = null;
		_lastOccupiedTime = Time.time;
		if ((bool)_emptySocketObject)
		{
			_emptySocketObject.SetActive(value: true);
		}
		if ((bool)_filledSocketObject)
		{
			_filledSocketObject.SetActive(value: false);
		}
	}

	private void OnSectorOccupantsUpdated()
	{
		base.enabled = _visibilityObject.GetSector().ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}

	private void Update()
	{
		if (_visibilityObject.IsNewlyObscured() && this.OnNewlyObscured != null && Time.time > _lastOccupiedTime + 0.5f)
		{
			this.OnNewlyObscured(this);
		}
	}

	private void OnPlayerEnterSocket(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			_isPlayerInSocket = true;
		}
	}

	private void OnPlayerExitSocket(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			_isPlayerInSocket = false;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (_occupiedByPlayerVolume == null && collider.CompareTag("PlayerDetector"))
		{
			_isPlayerInSocket = true;
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (_occupiedByPlayerVolume == null && collider.CompareTag("PlayerDetector"))
		{
			_isPlayerInSocket = false;
		}
	}
}
