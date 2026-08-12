using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SubstanceVolume : MonoBehaviour
{
	public enum SubstanceType
	{
		Oxygen = 0
	}

	[SerializeField]
	private SubstanceType _substanceType;

	private void Awake()
	{
		GetComponent<Collider>().isTrigger = true;
		if (base.gameObject.layer != LayerMask.NameToLayer("BasicEffectVolume"))
		{
			base.gameObject.layer = LayerMask.NameToLayer("AdvancedEffectVolume");
		}
	}

	public SubstanceType GetSubstanceType()
	{
		return _substanceType;
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (hitCollider.tag == "PlayerDetector")
		{
			hitCollider.GetRequiredComponent<SubstanceDetector>().AddSubstanceVolume(this);
		}
	}

	private void OnTriggerExit(Collider hitCollider)
	{
		if (hitCollider.tag == "PlayerDetector")
		{
			hitCollider.GetRequiredComponent<SubstanceDetector>().RemoveSubstanceVolume(this);
		}
	}
}
