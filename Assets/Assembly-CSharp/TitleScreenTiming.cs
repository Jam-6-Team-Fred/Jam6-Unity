using System.Collections;
using UnityEngine;

public class TitleScreenTiming : MonoBehaviour
{
	public static TitleScreenTiming Instance;

	public float timeToFadeInLogo;

	public float timeToStartButtonFadeInCascade;

	public float durationOfEachButtonFadeIn;

	public float timeBetweenButtonFadeInCascadeTriggers;

	public Transform owLogo;

	public Transform mainLayoutGroupParent;

	private IEnumerator MainMenuSlowIntroSequence()
	{
		yield return new WaitForSeconds(timeToFadeInLogo);
	}
}
