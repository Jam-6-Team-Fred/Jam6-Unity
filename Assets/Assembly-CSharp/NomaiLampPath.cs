using UnityEngine;

public class NomaiLampPath : MonoBehaviour
{
	[SerializeField]
	private NomaiLamp[] _lamps = new NomaiLamp[0];

	[SerializeField]
	private float _triggerDistance = 6f;

	private int _activeIndex;

	private void FixedUpdate()
	{
		Transform playerTransform = Locator.GetPlayerTransform();
		if (Vector3.Distance(b: _lamps[_activeIndex].transform.position + _lamps[_activeIndex].transform.up, a: playerTransform.position) < _triggerDistance)
		{
			_lamps[_activeIndex].FadeTo(0f, 0.5f);
			_activeIndex++;
			_lamps[_activeIndex].FadeTo(1f);
			if (_activeIndex == _lamps.Length - 1)
			{
				base.enabled = false;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		for (int i = 0; i < _lamps.Length; i++)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(_lamps[i].transform.position + _lamps[i].transform.up, _triggerDistance);
		}
	}
}
