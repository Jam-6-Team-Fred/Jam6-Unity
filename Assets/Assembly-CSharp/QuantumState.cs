using UnityEngine;

public class QuantumState : MonoBehaviour
{
	[SerializeField]
	private int _probability;

	[SerializeField]
	private bool _checkPlayerDistance;

	[SerializeField]
	private float _maxPlayerDistance;

	[Space]
	[SerializeField]
	private Light _blockingLight;

	public int GetProbability()
	{
		if (_blockingLight != null && _blockingLight.intensity > 0.001f)
		{
			return 0;
		}
		if (_checkPlayerDistance)
		{
			if (!(Vector3.Distance(Locator.GetPlayerTransform().position, base.transform.position) < _maxPlayerDistance))
			{
				return 0;
			}
			return _probability;
		}
		return _probability;
	}

	public void SetVisible(bool visible)
	{
		base.gameObject.SetActive(visible);
	}
}
