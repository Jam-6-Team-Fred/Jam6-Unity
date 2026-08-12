using UnityEngine;

public class FragmentDragAnimator : MonoBehaviour
{
	private DynamicFluidDetector _fluidDetector;

	private float _startDrag = 10000f;

	private float _endDrag = 10f;

	private float _duration;

	private float _startTime;

	private void Awake()
	{
		base.enabled = false;
	}

	public void StartAnimation(DynamicFluidDetector fluidDetector)
	{
		_fluidDetector = fluidDetector;
		_fluidDetector.SetDragFactor(_startDrag);
		_duration = ((fluidDetector.GetAttachedOWRigidbody().GetMass() > 10f) ? 5f : 2f);
		_startTime = Time.time;
		base.enabled = true;
	}

	private void FixedUpdate()
	{
		float num = Mathf.Clamp01((Time.time - _startTime) / _duration);
		num *= num;
		_fluidDetector.SetDragFactor(Mathf.Lerp(_startDrag, _endDrag, num));
		if (num >= 1f)
		{
			base.enabled = false;
		}
	}
}
