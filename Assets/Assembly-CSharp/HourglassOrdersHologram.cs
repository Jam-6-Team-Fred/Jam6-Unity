using UnityEngine;

public class HourglassOrdersHologram : Hologram
{
	[SerializeField]
	private Transform _probeLauncherTransform;

	[SerializeField]
	private GameObject _dataStream;

	private float _startTime;

	private Quaternion _origLocalRotation;

	private Vector3 _origFacing;

	private Vector3 _targetFacing;

	private void OnDestroy()
	{
	}

	protected override void OnFinishActivation()
	{
		_origLocalRotation = _probeLauncherTransform.localRotation;
		_origFacing = base.transform.InverseTransformDirection(_probeLauncherTransform.forward);
		_targetFacing = Locator.GetAstroObject(AstroObject.Name.ProbeCannon).transform.forward;
		_startTime = Time.time;
		_dataStream.SetActive(value: true);
	}

	protected override void OnDeactivation()
	{
	}

	protected override void UpdateHologram()
	{
		float num = Mathf.InverseLerp(_startTime + 2f, _startTime + 6f, Time.time);
		_probeLauncherTransform.localRotation = Quaternion.Slerp(_origLocalRotation, Quaternion.FromToRotation(_origFacing, _targetFacing), num);
		if (num >= 1f)
		{
			CompleteHologram();
		}
	}
}
