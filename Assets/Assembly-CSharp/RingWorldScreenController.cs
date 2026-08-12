using UnityEngine;

public class RingWorldScreenController : MonoBehaviour
{
	private static int s_propID_screenFlicker = -1;

	[SerializeField]
	private OWRenderer[] _screenRenderers;

	[SerializeField]
	private AnimationCurve _flickerCurve;

	private float _flickerStartTime;

	private void Awake()
	{
		if (s_propID_screenFlicker == -1)
		{
			s_propID_screenFlicker = Shader.PropertyToID("_ScreenFlicker");
		}
		base.enabled = false;
	}

	public void BeginFlicker()
	{
		_flickerStartTime = Time.time;
		base.enabled = true;
	}

	private void Update()
	{
		float num = Time.time - _flickerStartTime;
		float value = _flickerCurve.Evaluate(num);
		for (int i = 0; i < _screenRenderers.Length; i++)
		{
			_screenRenderers[i].SetMaterialProperty(s_propID_screenFlicker, value);
		}
		if (num > _flickerCurve.keys[_flickerCurve.length - 1].time)
		{
			base.enabled = false;
		}
	}
}
