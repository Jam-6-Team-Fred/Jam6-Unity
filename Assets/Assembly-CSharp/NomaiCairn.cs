using UnityEngine;

public class NomaiCairn : MonoBehaviour
{
	private NomaiCairnRock[] _rocks;

	private OWAudioSource _audioSource;

	private void Awake()
	{
		_rocks = GetComponentsInChildren<NomaiCairnRock>();
		_audioSource = GetComponent<OWAudioSource>();
		base.enabled = false;
	}

	private void KnockOver()
	{
		_audioSource.PlayOneShot(AudioType.KnockOverCairn);
		for (int i = 0; i < _rocks.Length; i++)
		{
			_rocks[i].MakeRigidbody(GetComponentInParent<OWRigidbody>());
			_rocks[i].ReturnAfterSeconds(i + 1 + 3, 3f);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		bool flag = true;
		for (int i = 0; i < _rocks.Length; i++)
		{
			if (!_rocks[i].HasReturnedToCairn())
			{
				flag = false;
			}
		}
		if (flag)
		{
			KnockOver();
		}
	}
}
