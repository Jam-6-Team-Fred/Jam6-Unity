using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadSceneOnTriggerSupernova : MonoBehaviour
{
	private void Awake()
	{
		GlobalMessenger.AddListener("TriggerSupernova", OnTriggerSupernova);
	}

	private void OnTriggerSupernova()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
