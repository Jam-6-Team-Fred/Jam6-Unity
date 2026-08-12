using UnityEngine;

public class DestructibleFragment : MonoBehaviour
{
	[SerializeField]
	private GameObject _debrisShardPrefab;

	[SerializeField]
	private GameObject _shatterEffectPrefab;

	private OWRigidbody _attachedOWRigidbody;

	private void Awake()
	{
		_attachedOWRigidbody = base.gameObject.GetAttachedOWRigidbody();
		if (_shatterEffectPrefab == null)
		{
			_shatterEffectPrefab = (GameObject)Resources.Load("Prefabs/Particles/Explosion_Debris_Med");
		}
		if (GetComponentInParent<FragmentIntegrity>() != null)
		{
			GetComponentInParent<FragmentIntegrity>().OnTakeDamage += OnTakeDamage;
		}
	}

	private void OnDestroy()
	{
		if (GetComponentInParent<FragmentIntegrity>() != null)
		{
			GetComponentInParent<FragmentIntegrity>().OnTakeDamage -= OnTakeDamage;
		}
	}

	private void OnTakeDamage(float integrity)
	{
		if (!(integrity > 0f))
		{
			_ = _debrisShardPrefab != null;
			if (_shatterEffectPrefab != null)
			{
				GameObject obj = Object.Instantiate(_shatterEffectPrefab, base.transform.position, base.transform.rotation);
				obj.transform.parent = base.transform.root;
				obj.GetRequiredComponent<OWRigidbody>().SetVelocity(_attachedOWRigidbody.GetPointVelocity(base.transform.position));
			}
			Object.Destroy(base.gameObject);
		}
	}
}
