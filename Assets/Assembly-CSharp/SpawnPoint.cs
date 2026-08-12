using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	[SerializeField]
	private SpawnLocation _spawnLocation;

	[SerializeField]
	private bool _isShipSpawn;

	[SerializeField]
	private OWTriggerVolume[] _triggerVolumes;

	private OWRigidbody _attachedBody;

	private void Awake()
	{
		_attachedBody = base.gameObject.GetAttachedOWRigidbody();
	}

	public void SetSpawnLocation(SpawnLocation location)
	{
		_spawnLocation = location;
	}

	public SpawnLocation GetSpawnLocation()
	{
		return _spawnLocation;
	}

	public virtual void OnSpawnPlayer()
	{
	}

	public void AddObjectToTriggerVolumes(GameObject detectorObj)
	{
		for (int i = 0; i < _triggerVolumes.Length; i++)
		{
			_triggerVolumes[i].AddObjectToVolume(detectorObj);
		}
	}

	public Vector3 GetPointVelocity()
	{
		return _attachedBody.GetPointVelocity(base.transform.position);
	}

	public bool IsShipSpawn()
	{
		return _isShipSpawn;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		if (!_isShipSpawn)
		{
			Gizmos.DrawWireSphere(base.transform.position - base.transform.up * 0.5f, 0.5f);
			Gizmos.DrawWireSphere(base.transform.position + base.transform.up * 0.5f, 0.5f);
		}
		else
		{
			Gizmos.DrawWireSphere(base.transform.position, 5f);
		}
	}
}
