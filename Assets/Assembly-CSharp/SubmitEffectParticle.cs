using UnityEngine;

public class SubmitEffectParticle : SubmitEffect
{
	[SerializeField]
	private ParticleSystem _particleSystem;

	protected override void ActivateEffect()
	{
		_particleSystem.Play();
	}
}
