using UnityEngine;

public abstract class RetractableHUDElement : HUDElement
{
	[SerializeField]
	private Vector3 _retractedOffset;

	private Vector3 _extendedPos;

	private Vector3 _retractedPos;

	private Vector3 _startPos;

	private Vector3 _finalPos;

	private bool _retracting;

	private float _startTransitionTime;

	protected override void Awake()
	{
		_extendedPos = base.transform.localPosition;
		_retractedPos = _extendedPos + _retractedOffset;
		base.Awake();
	}

	protected override void ShowHUD()
	{
		_startPos = base.transform.localPosition;
		_finalPos = _extendedPos;
		_startTransitionTime = Time.time;
		_retracting = false;
		base.enabled = true;
		base.ShowHUD();
	}

	protected override void HideHUD()
	{
		_startPos = base.transform.localPosition;
		_finalPos = _retractedPos;
		_startTransitionTime = Time.time;
		_retracting = true;
		base.enabled = true;
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.time - _startTransitionTime) / 0.5f);
		base.transform.localPosition = Vector3.Lerp(_startPos, _finalPos, num);
		if (num >= 1f)
		{
			base.enabled = false;
			if (_retracting)
			{
				DisableRenderers();
			}
		}
	}
}
