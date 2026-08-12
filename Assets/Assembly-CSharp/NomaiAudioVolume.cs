using UnityEngine;

[RequireComponent(typeof(SphereShape))]
[RequireComponent(typeof(NomaiText))]
public class NomaiAudioVolume : EffectVolume
{
	private SphereShape _sphereShape;

	private NomaiText _nomaiText;

	protected override void Awake()
	{
		base.Awake();
		_sphereShape = GetComponent<SphereShape>();
		_nomaiText = GetComponent<NomaiText>();
	}

	public NomaiText GetAudioText()
	{
		return _nomaiText;
	}

	public float GetNormalizedDistToOrigin(Vector3 worldPos)
	{
		Vector3 a = base.transform.TransformPoint(_sphereShape.center);
		float num = _sphereShape.radius * Mathf.Max(Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.y), base.transform.lossyScale.z);
		return Mathf.Clamp01(Vector3.Distance(a, worldPos) / num);
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		NomaiAudioDetector component = hitObj.GetComponent<NomaiAudioDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		NomaiAudioDetector component = hitObj.GetComponent<NomaiAudioDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}
}
