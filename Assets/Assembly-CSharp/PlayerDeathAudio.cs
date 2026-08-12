using UnityEngine;

[RequireComponent(typeof(OWAudioSource))]
public class PlayerDeathAudio : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _deathSource;

	[SerializeField]
	private OWAudioSource _helmetCrackSource;

	[SerializeField]
	private OWAudioSource _shipGroanSource;

	public void PlayCrushedByElevator()
	{
		_deathSource.PlayOneShot(AudioType.Death_CrushedByElevator);
	}

	public void PlayDeathAudio(DeathType deathType, float deathDuration)
	{
		switch (deathType)
		{
		case DeathType.Energy:
		case DeathType.Supernova:
		case DeathType.DreamExplosion:
			_deathSource.PlayOneShot(AudioType.Death_Energy);
			break;
		case DeathType.BigBang:
			_deathSource.PlayOneShot(AudioType.Death_BigBang);
			break;
		case DeathType.Digestion:
			_deathSource.PlayOneShot(AudioType.Death_Digestion);
			if (PlayerState.IsInsideShip())
			{
				_shipGroanSource.PlayDelayed(1f);
			}
			break;
		case DeathType.Crushed:
			_deathSource.PlayOneShot(AudioType.Death_Crushed);
			break;
		case DeathType.CrushedByElevator:
			_deathSource.PlayOneShot(AudioType.Death_CrushedByElevator);
			break;
		case DeathType.Meditation:
			_deathSource.PlayOneShot(AudioType.Death_Self);
			break;
		case DeathType.TimeLoop:
			_deathSource.PlayOneShot(AudioType.Death_TimeLoop);
			break;
		case DeathType.BlackHole:
			_deathSource.PlayOneShot(AudioType.SingularityOnPlayerEnterExit);
			break;
		case DeathType.Lava:
			_deathSource.PlayOneShot(AudioType.Death_Lava);
			break;
		default:
			_deathSource.PlayOneShot(AudioType.Death_Instant);
			break;
		case DeathType.Asphyxiation:
		case DeathType.Dream:
			break;
		}
		if (Locator.GetDeathManager().GetCrackHelmetOnDeath())
		{
			_helmetCrackSource.pitch = Random.Range(0.9f, 1.1f);
			_helmetCrackSource.PlayOneShot(AudioType.PlayerSuitHelmetCrack);
		}
		Locator.GetAudioMixer().MixDeath(deathDuration);
	}
}
