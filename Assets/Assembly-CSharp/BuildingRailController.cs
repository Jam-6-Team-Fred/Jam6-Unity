using UnityEngine;

public class BuildingRailController : MonoBehaviour
{
	[SerializeField]
	private Sector _sector;

	[Space]
	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private DetachableBuilding _detachableBuilding;

	[SerializeField]
	private Animation _animation;

	[SerializeField]
	private float _delay;

	private float _startMoveTime;

	private bool _movingAlongRail;

	public DetachableBuilding detachableBuilding => _detachableBuilding;

	private void Awake()
	{
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
		}
	}

	public void StartMoveAlongRail()
	{
		if (_sector != null && !_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			_animation.Play();
			_animation[_animation.clip.name].normalizedTime = 1f;
			_animation.Sample();
			_detachableBuilding.transform.position = _animation.transform.position;
			_detachableBuilding.transform.rotation = _animation.transform.rotation;
			ProxyShadowCaster[] proxyShadowCasters = _detachableBuilding.proxyShadowCasters;
			for (int i = 0; i < proxyShadowCasters.Length; i++)
			{
				proxyShadowCasters[i].SetDynamic(dynamic: false);
			}
			_movingAlongRail = false;
			base.enabled = false;
		}
		else
		{
			_detachableBuilding.Detach(kinematicSimulation: true);
			_animation.Play();
			_animation.Sample();
			_movingAlongRail = true;
			base.enabled = true;
		}
	}

	private void FixedUpdate()
	{
		if (!_movingAlongRail)
		{
			if (Time.time >= _startMoveTime)
			{
				StartMoveAlongRail();
			}
			return;
		}
		OWRigidbody buildingBody = _detachableBuilding.buildingBody;
		OWRigidbody origParentBody = buildingBody.GetOrigParentBody();
		Vector3 position = buildingBody.GetPosition();
		Quaternion rotation = buildingBody.GetRotation();
		Vector3 position2 = _animation.transform.position;
		Quaternion rotation2 = _animation.transform.rotation;
		Vector3 pointVelocity = origParentBody.GetPointVelocity(position);
		Vector3 vector = (position2 - position) / Time.fixedDeltaTime;
		buildingBody.SetVelocity(pointVelocity + vector);
		Vector3 angularVelocity = origParentBody.GetAngularVelocity();
		Vector3 vector2 = OWPhysics.FromToAngularVelocity(rotation, rotation2);
		buildingBody.SetAngularVelocity(angularVelocity + vector2);
		if (!_animation.isPlaying)
		{
			_detachableBuilding.Reattach();
			_movingAlongRail = false;
			base.enabled = false;
		}
	}

	private void OnFloodImpact()
	{
		if (_delay > 0f)
		{
			base.enabled = true;
			_startMoveTime = Time.time + _delay;
		}
		else
		{
			StartMoveAlongRail();
		}
	}
}
