using UnityEngine;

[AddComponentMenu("Audio/Audio Shell", 300)]
public class AudioShell : SectoredMonoBehaviour
{
	[SerializeField]
	private Transform _audioTransform;

	[SerializeField]
	private float _radius;

	protected override void Awake()
	{
		base.Awake();
		base.enabled = false;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsOccupant(DynamicOccupant.Player);
	}

	private void Update()
	{
		Vector3 vector = Locator.GetPlayerTransform().position - base.transform.position;
		_audioTransform.position = base.transform.position + vector.normalized * _radius;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = (Gizmos.color = Color.yellow);
		Gizmos.DrawWireSphere(base.transform.position, _radius);
	}
}
