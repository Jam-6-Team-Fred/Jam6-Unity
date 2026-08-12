using UnityEngine;

public class ScaleOverTime : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve _scaleCurve;

	private void Update()
	{
		base.transform.localScale = Vector3.one * _scaleCurve.Evaluate(TimeLoop.GetFractionElapsed());
	}
}
