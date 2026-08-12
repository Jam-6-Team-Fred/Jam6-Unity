using UnityEngine;

public class DamFragmentController : MonoBehaviour
{
	[SerializeField]
	private Sector _sector;

	[Space]
	[SerializeField]
	private Transform _snapTarget;

	[SerializeField]
	private float _attachDelay = 20f;

	[SerializeField]
	private OWCollider[] _lowResColliders = new OWCollider[0];

	[SerializeField]
	private OWCollider _highResCollider;

	[SerializeField]
	private GameObject _turningOffInFlood;

	private OWRigidbody _damFragmentBody;

	private bool _attachedToRingworld;

	private bool _lowResCollidersOn = true;

	private float _attachTime;

	private bool _floodTurnOff;

	public OWCollider[] dynamicColliders => _lowResColliders;

	public OWCollider highResCollider => _highResCollider;

	private void Awake()
	{
		_damFragmentBody = this.GetRequiredComponent<OWRigidbody>();
		if (_highResCollider != null)
		{
			_highResCollider.SetActivation(active: false);
		}
	}

	private void Start()
	{
		_sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
		_attachTime = Time.time + _attachDelay + Random.value;
		if (!_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			if ((bool)_snapTarget)
			{
				_damFragmentBody.transform.position = _snapTarget.position;
				_damFragmentBody.transform.rotation = _snapTarget.rotation;
			}
			AttachToRingworld();
			if (_turningOffInFlood != null)
			{
				_turningOffInFlood.SetActive(value: false);
				_floodTurnOff = true;
			}
		}
	}

	private void OnDestroy()
	{
		_sector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
	}

	private void OnOccupantExitSector(SectorDetector sectorDetector)
	{
		if (!_attachedToRingworld && sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			if ((bool)_snapTarget)
			{
				_damFragmentBody.transform.position = _snapTarget.position;
				_damFragmentBody.transform.rotation = _snapTarget.rotation;
			}
			AttachToRingworld();
			base.enabled = true;
		}
	}

	private void FixedUpdate()
	{
		if (_attachedToRingworld && !_lowResCollidersOn)
		{
			if (_highResCollider != null)
			{
				_highResCollider.SetActivation(active: true);
			}
			base.enabled = false;
			return;
		}
		if (!_floodTurnOff && Time.time > 5f && _turningOffInFlood != null)
		{
			_turningOffInFlood.SetActive(value: false);
			_floodTurnOff = true;
		}
		if (Time.time > _attachTime)
		{
			AttachToRingworld();
		}
	}

	private void AttachToRingworld()
	{
		if (!_attachedToRingworld)
		{
			_damFragmentBody.transform.parent = _damFragmentBody.GetOrigParent();
			Rigidbody rigidbody = _damFragmentBody.GetRigidbody();
			Object.Destroy(_damFragmentBody.GetComponent<CenterOfTheUniverseOffsetApplier>());
			Object.Destroy(_damFragmentBody.GetComponent<ForceApplier>());
			Object.Destroy(_damFragmentBody.GetComponent<ForceDetector>());
			Object.Destroy(_damFragmentBody);
			Object.Destroy(rigidbody);
			_damFragmentBody = null;
			for (int i = 0; i < _lowResColliders.Length; i++)
			{
				_lowResColliders[i].SetActivation(active: false);
			}
			_lowResCollidersOn = false;
			_attachedToRingworld = true;
		}
	}
}
