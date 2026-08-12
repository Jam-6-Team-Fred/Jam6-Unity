using UnityEngine;

public class PlayerCrushedController : MonoBehaviour
{
	private PlayerCharacterController _playerController;

	private bool _isPlayerCrushed;

	private float _crushedTime;

	private GameObject _crushedPivot;

	private Vector3 _playerCrushedLocalPos;

	private void Awake()
	{
		_playerController = GetComponent<PlayerCharacterController>();
	}

	private void FixedUpdate()
	{
		if (!_isPlayerCrushed && _playerController.IsGroundedOnRisingSand() && !PlayerState.IsAttached())
		{
			if (!Physics.Raycast(base.transform.position + base.transform.up * 0.5f, base.transform.up, out var hitInfo, 1f, OWLayerMask.physicalMask))
			{
				return;
			}
			float num = hitInfo.distance - 0.5f;
			float num2 = Vector3.Angle(-base.transform.up, hitInfo.normal);
			MonoBehaviour.print("dist to ceiling: " + num + "   slope degrees: " + num2);
			if ((num < 0f && num2 < 20f) || num < 0.05f)
			{
				OWRigidbody component = hitInfo.rigidbody.GetComponent<OWRigidbody>();
				if (component != null)
				{
					CrushPlayer(component);
					return;
				}
				Debug.LogError("Crushed by Rigidbody with no attached OWRigidbody");
				Debug.Break();
			}
		}
		else if (_isPlayerCrushed)
		{
			Locator.GetPlayerTransform().localPosition = _playerCrushedLocalPos;
			Locator.GetPlayerTransform().localRotation = Quaternion.identity;
			float num3 = Mathf.InverseLerp(_crushedTime, _crushedTime + 5f, Time.time);
			num3 *= num3;
			_crushedPivot.transform.localEulerAngles = new Vector3(0f, 0f, num3 * 10f);
		}
	}

	private void CrushPlayer(OWRigidbody crushedByBody)
	{
		Locator.GetPlayerController().SetColliderActivation(active: false);
		Locator.GetPlayerBody().MakeKinematic();
		MonoBehaviour.print("Crushed! Attaching player to " + crushedByBody.gameObject.name);
		GameObject gameObject = new GameObject("CrushedToDeath_Root");
		gameObject.transform.parent = crushedByBody.transform;
		gameObject.transform.position = Locator.GetPlayerCamera().transform.position;
		gameObject.transform.rotation = Locator.GetPlayerTransform().rotation;
		_crushedPivot = new GameObject("CrushedToDeath_Pivot");
		_crushedPivot.transform.parent = gameObject.transform;
		_crushedPivot.transform.localPosition = Vector3.zero;
		_crushedPivot.transform.localRotation = Quaternion.identity;
		Locator.GetPlayerTransform().parent = _crushedPivot.transform;
		GlobalMessenger<OWRigidbody>.FireEvent("AttachPlayerToPoint", crushedByBody);
		Locator.GetDeathManager().KillPlayer(DeathType.Crushed);
		_isPlayerCrushed = true;
		_crushedTime = Time.time;
		_playerCrushedLocalPos = Locator.GetPlayerTransform().localPosition;
	}
}
