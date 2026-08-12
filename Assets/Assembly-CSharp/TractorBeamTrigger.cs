using UnityEngine;

public class TractorBeamTrigger : OWTriggerVolume
{
	[SerializeField]
	private TractorBeamController _tractorBeam;

	[SerializeField]
	private bool _activateBeam = true;

	protected override void Reset()
	{
		base.Reset();
		_tractorBeam = GetComponentInParent<TractorBeamController>();
		Collider component = GetComponent<Collider>();
		if (component != null)
		{
			component.isTrigger = true;
		}
		if (!OWLayerMask.IsLayerInMask(base.gameObject.layer, OWLayerMask.effectVolumeMask))
		{
			base.gameObject.layer = LayerMask.NameToLayer("BasicEffectVolume");
		}
	}

	public override void AddObjectToVolume(GameObject obj)
	{
		if (_active)
		{
			base.AddObjectToVolume(obj);
			if (obj.CompareTag("PlayerDetector"))
			{
				_tractorBeam.SetActivation(_activateBeam);
			}
		}
	}
}
