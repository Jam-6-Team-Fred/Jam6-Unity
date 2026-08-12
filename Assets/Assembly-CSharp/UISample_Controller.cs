using System.Collections.Generic;
using UnityEngine;

public class UISample_Controller : MonoBehaviour
{
	private Animator uiManager_Animator;

	public List<Transform> listOfAllMenuItems;

	public static UISample_Controller Instance;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		uiManager_Animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			uiManager_Animator.SetTrigger("RevealMenu");
		}
	}
}
