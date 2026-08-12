using UnityEngine;

public class DetachableConnector : DetachableFragment
{
	[SerializeField]
	private FragmentIntegrity[] _connectedFragments;

	[SerializeField]
	private int _minSupportCount = 1;

	[SerializeField]
	private float _detachAtIntegrity;

	protected override void AddEventListeners()
	{
		for (int i = 0; i < _connectedFragments.Length; i++)
		{
			_connectedFragments[i].OnTakeDamage += OnTakeDamage;
		}
	}

	protected override void RemoveEventListeners()
	{
		for (int i = 0; i < _connectedFragments.Length; i++)
		{
			_connectedFragments[i].OnTakeDamage -= OnTakeDamage;
		}
	}

	protected override void OnTakeDamage(float integrity)
	{
		int num = 0;
		for (int i = 0; i < _connectedFragments.Length; i++)
		{
			if (_connectedFragments[i].GetIntegrity() <= _detachAtIntegrity)
			{
				num++;
			}
		}
		if (_connectedFragments.Length - num < _minSupportCount)
		{
			Detach();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		if (_connectedFragments == null)
		{
			return;
		}
		for (int i = 0; i < _connectedFragments.Length; i++)
		{
			if (_connectedFragments[i] != null)
			{
				DetachableFragment component = _connectedFragments[i].GetComponent<DetachableFragment>();
				if (component != null)
				{
					Gizmos.DrawLine(base.transform.position, component.GetWorldCenterOfMass());
					Gizmos.DrawSphere(base.transform.position, 0.5f);
					Gizmos.DrawSphere(component.GetWorldCenterOfMass(), 0.5f);
				}
			}
		}
	}
}
