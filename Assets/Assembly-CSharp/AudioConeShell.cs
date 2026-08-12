using UnityEngine;

[AddComponentMenu("Audio/Audio Cone Shell", 300)]
public class AudioConeShell : SectoredMonoBehaviour
{
	[SerializeField]
	private Transform _audioTransform;

	[SerializeField]
	private float _bottomRadius;

	[SerializeField]
	private float _topRadius;

	[SerializeField]
	private float _height;

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
		float num = _height * 0.5f;
		Vector3 vector = base.transform.InverseTransformPoint(Locator.GetPlayerTransform().position);
		float num2 = Mathf.Clamp(vector.y, 0f - num, num);
		float num3 = Mathf.Lerp(_bottomRadius, _topRadius, (num2 + num) / _height);
		vector.y = 0f;
		_audioTransform.localPosition = vector.normalized * num3 + Vector3.up * num2;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = (Gizmos.color = Color.yellow);
		Vector3 vector = base.transform.position + base.transform.up * _height * 0.5f;
		Vector3 vector2 = base.transform.position - base.transform.up * _height * 0.5f;
		OWGizmos.DrawWireCircle(vector, base.transform.up, _topRadius);
		OWGizmos.DrawWireCircle(vector2, base.transform.up, _bottomRadius);
		Gizmos.DrawLine(vector2, vector);
	}
}
