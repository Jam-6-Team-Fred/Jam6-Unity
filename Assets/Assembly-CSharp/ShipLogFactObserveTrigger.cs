using UnityEngine;

public class ShipLogFactObserveTrigger : MonoBehaviour, IObservable
{
	[HideInInspector]
	[SerializeField]
	private string _factID = string.Empty;

	[SerializeField]
	private string[] _factIDs;

	[SerializeField]
	private float _maxViewDistance = 2f;

	[SerializeField]
	private float _maxViewAngle = 180f;

	[SerializeField]
	private bool _disableColliderOnRevealFact;

	private OWCollider _owCollider;

	private bool _factsRevealed;

	private void Reset()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Interactible");
	}

	private void Awake()
	{
		if (_disableColliderOnRevealFact)
		{
			_owCollider = base.gameObject.GetAddComponent<OWCollider>();
		}
		if (_factID.Length > 0)
		{
			Debug.LogError("SHIP LOG FACT TRIGGER NEEDS TO BE CHANGED TO LIST (PLEASE TELL ALEX ABOUT THIS)", this);
			Debug.Break();
		}
	}

	public void Observe(RaycastHit raycastHit, Vector3 raycastOrigin)
	{
		float num = Vector3.Angle(raycastHit.point - raycastOrigin, -base.transform.forward);
		if (!_factsRevealed && raycastHit.distance < _maxViewDistance && num < _maxViewAngle)
		{
			if (_disableColliderOnRevealFact)
			{
				_owCollider.SetActivation(active: false);
			}
			for (int i = 0; i < _factIDs.Length; i++)
			{
				Locator.GetShipLogManager().RevealFact(_factIDs[i]);
			}
			_factsRevealed = true;
		}
	}

	public void LoseFocus()
	{
	}

	private void OnDrawGizmosSelected()
	{
		Quaternion quaternion = Quaternion.AngleAxis(_maxViewAngle, base.transform.up);
		Vector3 vector = quaternion * (base.transform.forward * _maxViewDistance);
		Vector3 vector2 = Quaternion.Inverse(quaternion) * (base.transform.forward * _maxViewDistance);
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(base.transform.position, base.transform.position + vector);
		Gizmos.DrawLine(base.transform.position, base.transform.position + vector2);
		Gizmos.color = Color.blue;
		OWGizmos.DrawWireCircle(base.transform.position, base.transform.up, _maxViewDistance);
	}
}
