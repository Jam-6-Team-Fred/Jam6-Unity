using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class DreamDropDistanceVolume : MonoBehaviour
{
	[SerializeField]
	private float _killAfterFallDistance;

	private OWTriggerVolume _trigger;

	private OWRigidbody _parentBody;

	private RaycastHit[] _raycastHits = new RaycastHit[32];

	private float _lastGroundedHeight;

	private bool _killNextGrounded;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private float GetPlayerHeight()
	{
		return base.transform.InverseTransformPoint(Locator.GetPlayerTransform().position).y;
	}

	private void FixedUpdate()
	{
		if (Locator.GetPlayerController().IsGrounded())
		{
			_lastGroundedHeight = GetPlayerHeight();
		}
		else if (_lastGroundedHeight - GetPlayerHeight() > _killAfterFallDistance)
		{
			_killNextGrounded = true;
		}
	}

	private void OnBecomeGrounded()
	{
		MonoBehaviour.print("fall distance: " + (_lastGroundedHeight - GetPlayerHeight()));
		if (!_killNextGrounded)
		{
			return;
		}
		Locator.GetPlayerController().OnBecomeGrounded -= OnBecomeGrounded;
		base.enabled = false;
		if (Locator.GetDreamWorldController().IsExitingDream() || Locator.GetDeathManager().IsPlayerDying() || Locator.GetDeathManager().IsPlayerDead())
		{
			MonoBehaviour.print("Someone beat us to the punch! Abort Operation D-Cubed.");
			return;
		}
		float num = Locator.GetPlayerTransform().GetComponent<PlayerResources>().GetMaxImpactSpeed() + 5f;
		if (Physics.RaycastNonAlloc(Locator.GetPlayerTransform().position, -Vector3.up, _raycastHits, 2f, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore) > 0)
		{
			MonoBehaviour.print("Dream Drop Distance!!!");
			ImpactData impact = new ImpactData(Locator.GetPlayerBody(), Locator.GetPlayerCollider(), _raycastHits[0].rigidbody.GetComponent<OWRigidbody>(), Locator.GetPlayerBody().GetVelocity(), num, _raycastHits[0]);
			Locator.GetPlayerTransform().GetComponent<ImpactSensor>().FireHighSpeedImpactEvent(impact);
		}
		else
		{
			Debug.LogWarning("Somehow the Dream Drop Distance raycast missed???");
			Locator.GetDeathManager().SetImpactDeathSpeed(num);
			Locator.GetDeathManager().KillPlayer(DeathType.Impact);
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			Locator.GetPlayerController().OnBecomeGrounded += OnBecomeGrounded;
			_lastGroundedHeight = GetPlayerHeight();
			_killNextGrounded = false;
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			Locator.GetPlayerController().OnBecomeGrounded -= OnBecomeGrounded;
			_killNextGrounded = false;
			base.enabled = false;
		}
	}
}
