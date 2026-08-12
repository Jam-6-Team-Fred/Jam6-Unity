using UnityEngine;

public class CorruptionAnimator : MonoBehaviour
{
	private Material _material;

	private void Awake()
	{
		_material = GetComponent<Renderer>().materials[1];
	}

	private void Update()
	{
		_material.SetFloat("_Cutoff", Mathf.Clamp01(TimeLoop.GetFractionElapsed()));
	}
}
