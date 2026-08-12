using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class VanishVolume : MonoBehaviour
{
	[SerializeField]
	private GameObject _vanishEffectPrefab;

	[SerializeField]
	private bool _onlyAffectsPlayerAndShip;

	[SerializeField]
	private bool _shrinkBodies = true;

	protected Collider _collider;

	protected OWRigidbody _attachedOWRigidbody;

	private List<OWRigidbody> _shrinkingBodies;

	private List<RelativeLocationData> _shrinkingBodyLocationData;

	private OWRigidbody _playerBody;

	private OWRigidbody _shipBody;

	private OWRigidbody _shipCockpitBody;

	private OWRigidbody _probeBody;

	private OWRigidbody _modelShipBody;

	private OWRigidbody _nomaiShuttleBody;

	private RelativeLocationData _playerLocation;

	private RelativeLocationData _shipLocation;

	private RelativeLocationData _shipCockpitLocation;

	private RelativeLocationData _probeLocation;

	private RelativeLocationData _modelShipLocation;

	private RelativeLocationData _nomaiShuttleLocation;

	private List<VanishVolumeCustomHandler> _vanishVolumeCustomHandlers;

	private ParticleSystemPool _vanishEffectPool;

	protected virtual void Awake()
	{
		_attachedOWRigidbody = base.gameObject.GetAttachedOWRigidbody();
		_shrinkingBodies = new List<OWRigidbody>(8);
		_shrinkingBodyLocationData = new List<RelativeLocationData>(8);
		_vanishVolumeCustomHandlers = new List<VanishVolumeCustomHandler>(8);
		if (_vanishEffectPrefab != null)
		{
			_vanishEffectPool = new ParticleSystemPool(_vanishEffectPrefab, 8, base.transform);
		}
		_collider = GetComponent<Collider>();
		_collider.Assert(OWLayerMask.effectVolumeMask, isTrigger: true);
	}

	protected abstract void Vanish(OWRigidbody bodyToVanish, RelativeLocationData entryLocation);

	protected abstract void VanishPlayer(OWRigidbody playerBody, RelativeLocationData entryLocation);

	protected abstract void VanishShip(OWRigidbody shipBody, RelativeLocationData entryLocation);

	protected abstract void VanishShipCockpit(OWRigidbody shipCockpitBody, RelativeLocationData entryLocation);

	protected abstract void VanishProbe(OWRigidbody probeBody, RelativeLocationData entryLocation);

	protected abstract void VanishModelRocketShip(OWRigidbody modelBody, RelativeLocationData entryLocation);

	protected abstract void VanishNomaiShuttle(OWRigidbody modelBody, RelativeLocationData entryLocation);

	protected void Shrink(OWRigidbody bodyToShrink)
	{
		if (!_shrinkingBodies.Contains(bodyToShrink))
		{
			if (bodyToShrink.CompareTag("DetachedFragment"))
			{
				bodyToShrink.GetComponentInChildren<DetachableFragment>().BeginWarpScaling();
			}
			_shrinkingBodies.Add(bodyToShrink);
			_shrinkingBodyLocationData.Add(new RelativeLocationData(bodyToShrink, base.transform));
		}
	}

	protected virtual void Update()
	{
		if (_vanishEffectPool != null)
		{
			_vanishEffectPool.Update();
		}
	}

	protected virtual void FixedUpdate()
	{
		for (int num = _shrinkingBodies.Count - 1; num >= 0; num--)
		{
			if (_shrinkingBodies[num] == null)
			{
				_shrinkingBodies.RemoveAt(num);
				_shrinkingBodyLocationData.RemoveAt(num);
			}
			else
			{
				_shrinkingBodies[num].SetLocalScale(_shrinkingBodies[num].GetLocalScale() * 0.95f);
				if (_shrinkingBodies[num].GetLocalScale().x < 0.1f)
				{
					_shrinkingBodies[num].SetLocalScale(new Vector3(0.1f, 0.1f, 0.1f));
					Vanish(_shrinkingBodies[num], _shrinkingBodyLocationData[num]);
					_shrinkingBodies.RemoveAt(num);
					_shrinkingBodyLocationData.RemoveAt(num);
				}
			}
		}
		if (_playerBody != null)
		{
			VanishPlayer(_playerBody, _playerLocation);
			_playerBody = null;
		}
		if (_shipBody != null)
		{
			VanishShip(_shipBody, _shipLocation);
			_shipBody = null;
		}
		if (_shipCockpitBody != null)
		{
			VanishShipCockpit(_shipCockpitBody, _shipCockpitLocation);
			_shipCockpitBody = null;
		}
		if (_probeBody != null)
		{
			VanishProbe(_probeBody, _probeLocation);
			_probeBody = null;
		}
		if (_modelShipBody != null)
		{
			VanishModelRocketShip(_modelShipBody, _modelShipLocation);
			_modelShipBody = null;
		}
		if (_nomaiShuttleBody != null)
		{
			VanishNomaiShuttle(_nomaiShuttleBody, _nomaiShuttleLocation);
			_nomaiShuttleBody = null;
		}
		if (_vanishVolumeCustomHandlers.Count > 0)
		{
			for (int i = 0; i < _vanishVolumeCustomHandlers.Count; i++)
			{
				_vanishVolumeCustomHandlers[i].HandleVanish(this);
			}
			_vanishVolumeCustomHandlers.Clear();
		}
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (!(hitCollider.attachedRigidbody != null))
		{
			return;
		}
		VanishVolumeCustomHandler component = hitCollider.attachedRigidbody.GetComponent<VanishVolumeCustomHandler>();
		if (component != null && component.ShouldHandleVanish(this))
		{
			component.CacheVanishData(this, new RelativeLocationData(hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>(), base.transform));
			_vanishVolumeCustomHandlers.Add(component);
		}
		else if (hitCollider.attachedRigidbody.CompareTag("Player"))
		{
			_playerBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
			_playerLocation = new RelativeLocationData(_playerBody, base.transform);
		}
		else if (hitCollider.attachedRigidbody.CompareTag("Ship"))
		{
			_shipBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
			_shipLocation = new RelativeLocationData(_shipBody, base.transform);
		}
		else if (hitCollider.attachedRigidbody.CompareTag("ShipCockpit"))
		{
			_shipCockpitBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
			_shipCockpitLocation = new RelativeLocationData(_shipCockpitBody, base.transform);
		}
		else if (hitCollider.attachedRigidbody.CompareTag("Probe"))
		{
			_probeBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
			_probeLocation = new RelativeLocationData(_probeBody, base.transform);
		}
		else if (hitCollider.attachedRigidbody.CompareTag("ModelRocketShipBody"))
		{
			_modelShipBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
			_modelShipLocation = new RelativeLocationData(_modelShipBody, base.transform);
		}
		else if (hitCollider.attachedRigidbody.CompareTag("NomaiShuttleBody"))
		{
			_nomaiShuttleBody = hitCollider.attachedRigidbody.GetRequiredComponent<OWRigidbody>();
			_nomaiShuttleLocation = new RelativeLocationData(_nomaiShuttleBody, base.transform);
		}
		else
		{
			if (_onlyAffectsPlayerAndShip)
			{
				return;
			}
			OWRigidbody component2 = hitCollider.attachedRigidbody.GetComponent<OWRigidbody>();
			if (component2 != null)
			{
				if (_shrinkBodies)
				{
					Shrink(component2);
				}
				else
				{
					Vanish(component2, new RelativeLocationData(component2, base.transform));
				}
			}
		}
		if (_vanishEffectPool != null)
		{
			Quaternion quaternion = Quaternion.FromToRotation(base.transform.forward, hitCollider.transform.position - base.transform.position);
			_vanishEffectPool.Instantiate(base.transform, hitCollider.transform.position, quaternion * base.transform.rotation);
		}
	}
}
