using UnityEngine;

public class IceMeltController : MonoBehaviour
{
	[SerializeField]
	private float _startMeltDistance = 1000f;

	[SerializeField]
	private float _endMeltDistance = 500f;

	[Space]
	[SerializeField]
	private Transform _surfaceIce;

	[SerializeField]
	private Transform _surfaceIceProxy;

	[SerializeField]
	private float _surfaceIceMeltedScale = 0.9f;

	[Space]
	[SerializeField]
	private SkinnedMeshRenderer _crackIce;

	[SerializeField]
	private AnimationCurve[] _blendWeightCurves = new AnimationCurve[0];

	[SerializeField]
	private Transform[] _crackIceColliders;

	[SerializeField]
	private AnimationCurve _colliderScaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	[Space]
	[SerializeField]
	private AudioVolume _audioVolume;

	private Transform _sunTransform;

	private float _prevSunDist;

	private void Start()
	{
		_sunTransform = Locator.GetAstroObject(AstroObject.Name.Sun).transform;
		_audioVolume.SetVolumeActivation(active: false);
	}

	private void FixedUpdate()
	{
		float num = Vector3.Distance(base.transform.position, _sunTransform.position);
		float num2 = Mathf.InverseLerp(_startMeltDistance, _endMeltDistance, num);
		float num3 = Mathf.Lerp(1f, _surfaceIceMeltedScale, num2);
		_surfaceIce.localScale = new Vector3(num3, num3, num3);
		_surfaceIceProxy.localScale = new Vector3(num3, num3, num3);
		if (_crackIce.enabled && _crackIce.sharedMesh != null)
		{
			for (int i = 0; i < _blendWeightCurves.Length; i++)
			{
				_crackIce.SetBlendShapeWeight(i, _blendWeightCurves[i].Evaluate(num2));
			}
		}
		float num4 = _colliderScaleCurve.Evaluate(num2);
		for (int j = 0; j < _crackIceColliders.Length; j++)
		{
			_crackIceColliders[j].localScale = new Vector3(num4, num4, num4);
		}
		if (num2 > 0f && num2 < 1f && num < _prevSunDist && !_audioVolume.IsVolumeActive())
		{
			_audioVolume.SetVolumeActivation(active: true);
		}
		else if (_audioVolume.IsVolumeActive() && num2 >= 1f)
		{
			_audioVolume.SetVolumeActivation(active: false);
		}
		_prevSunDist = num;
	}
}
