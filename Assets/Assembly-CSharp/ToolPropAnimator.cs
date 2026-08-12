using UnityEngine;

public class ToolPropAnimator : MonoBehaviour
{
	private Vector3 _equippedPos;

	private Vector3 _unequippedPos;

	private float _startAnimTime;

	private bool _equipped;

	private Vector3 _startAnimPos;

	private void Awake()
	{
		base.enabled = false;
		_equippedPos = base.transform.localPosition;
		_unequippedPos = _equippedPos - new Vector3(-1.5f, 1f, 0f);
		base.transform.localPosition = _unequippedPos;
	}

	public void StartAnimation(bool equipped)
	{
		_equipped = equipped;
		_startAnimTime = Time.time;
		base.enabled = true;
		_startAnimPos = base.transform.localPosition;
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.time - _startAnimTime) / 0.5f);
		Vector3 b = (_equipped ? _equippedPos : _unequippedPos);
		base.transform.localPosition = Vector3.Lerp(_startAnimPos, b, Mathf.SmoothStep(0f, 1f, num));
		if (num >= 1f)
		{
			base.enabled = false;
		}
	}
}
