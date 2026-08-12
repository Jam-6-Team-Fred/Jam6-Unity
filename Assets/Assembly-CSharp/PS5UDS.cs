using System;
using System.Collections.Generic;
using UnityEngine;

public class PS5UDS : MonoBehaviour
{
	public static event Action<string> OnReceivedLaunchActivityIntent;

	public static void Init()
	{
	}

	public static void EarnTrophy(Achievements.Type type)
	{
	}

	public static void PostActivityStartEvent(string activityId)
	{
	}

	public static void PostActivityEndCompletedEvent(string activityId)
	{
	}

	public static void PostActivityEndAbandonedEvent(string activityId)
	{
	}

	public static void PostActivityResumeEvent(string activityId)
	{
	}

	public static void PostActivityAvailabilityChangeEvent(List<string> availableActivities)
	{
	}

	public static void PostUnavailableActivitiesEvent(List<string> unavailableActivities)
	{
	}

	public static void PostActivityTerminateEvent()
	{
	}

	public static void PostActivityPriorityChangeEvent(List<string> activities, List<int> priorities)
	{
	}

	public void Update()
	{
	}
}
