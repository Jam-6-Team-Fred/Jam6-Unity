using Steamworks;
using UnityEngine;

public class Achievements : MonoBehaviour
{
	public enum Type
	{
		TERRIBLE_FATE = 0,
		WHATS_THIS_BUTTON = 1,
		ALPHA_PILOT = 2,
		YOU_TRIED = 3,
		BEGINNERS_LUCK = 4,
		SATELLITE = 5,
		HEARTH_TO_MOON = 6,
		DEEP_IMPACT = 7,
		HARMONIC_CONVERGENCE = 8,
		MUSEUM = 9,
		DIEHARD = 10,
		PCHOOOOOOO = 11,
		GONE_IN_60_SECONDS = 12,
		CARCINOGENS = 13,
		CUTTING_IT_CLOSE = 14,
		MICAS_WRATH = 15,
		STUDIOUS = 16,
		AROUND_THE_WORLD = 17,
		SILENCED_CARTOGRAPHER = 18,
		TUBULAR = 19,
		EARLY_ADOPTER = 20,
		GRATE_FILTER = 21,
		FLAT_HEARTHER = 22,
		CELCIUS = 23,
		GHOSTS = 24,
		SLEEP_WAKE_REPEAT = 25,
		SIMULATION = 26,
		FIRE_ARROWS = 27,
		ONE_NINE = 28,
		TAKEMEALIVE = 29,
		OOFMYBONES = 30,
		FOUND_SIGNAL = 31,
		TOTAL = 32
	}

	public enum HeroStat
	{
		FULL_TIMELOOP = 0,
		PERFECT_MARSHMALLOW = 1,
		TIMELOOP_COUNT = 2,
		TOTAL = 3
	}

	private static readonly string[] s_names = new string[32]
	{
		"TERRIBLE_FATE", "WHATS_THIS_BUTTON", "ALPHA_PILOT", "YOU_TRIED", "BEGINNERS_LUCK", "RIGIDBODY", "HEARTH_TO_MOON", "DEEP_IMPACT", "HARMONIC_CONVERGENCE", "MUSEUM",
		"DIEHARD", "PCHOOOOOOO", "GONE_IN_60_SECONDS", "CARCINOGENS", "CUTTING_IT_CLOSE", "MICAS_WRATH", "STUDIOUS", "ACHIEVEMENT_1", "ACHIEVEMENT_2", "ACHIEVEMENT_3",
		"ACHIEVEMENT_4", "ACHIEVEMENT_5", "ACHIEVEMENT_6", "ACHIEVEMENT_7", "ACHIEVEMENT_8", "ACHIEVEMENT_9", "ACHIEVEMENT_10", "ACHIEVEMENT_11", "ACHIEVEMENT_12", "ACHIEVEMENT_13",
		"ACHIEVEMENT_14", "ACHIEVEMENT_MISSING"
	};

	private static bool[] s_isEarned = new bool[32];

	private void Update()
	{
	}

	public static void Init()
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.RequestCurrentStats();
		}
	}

	public static void Earn(Type type)
	{
		Debug.Log("Earn " + type);
		if (s_isEarned[(int)type])
		{
			return;
		}
		s_isEarned[(int)type] = true;
		if (SteamManager.Initialized)
		{
			if (!SteamUserStats.SetAchievement(s_names[(int)type]))
			{
				Debug.LogError("Unable to grant achievement \"" + s_names[(int)type] + "\"");
			}
			SteamUserStats.StoreStats();
		}
	}

	public static void SetHeroStat(HeroStat stat, uint value)
	{
	}

	public static void ResetAll()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		for (int i = 0; i < 32; i++)
		{
			if (!SteamUserStats.ClearAchievement(s_names[i]))
			{
				Debug.LogError("Unable to clear achievement \"" + s_names[i] + "\"");
			}
		}
		SteamUserStats.StoreStats();
	}

	public static void AchieveAll()
	{
		for (int i = 0; i < 32; i++)
		{
			Earn((Type)i);
		}
	}
}
