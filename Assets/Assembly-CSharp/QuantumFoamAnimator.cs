using UnityEngine;

public class QuantumFoamAnimator : MonoBehaviour
{
	private float _scale;

	private float _targetScale;

	private float _origScale;

	private void Start()
	{
		_origScale = base.transform.localScale.x;
		ResetScale();
	}

	private void Update()
	{
		_scale = Mathf.MoveTowards(_scale, _targetScale, Time.deltaTime * 1f);
		base.transform.localScale = _scale * _origScale * Vector3.one;
		if (_scale >= _targetScale)
		{
			ResetScale();
		}
	}

	private void ResetScale()
	{
		_scale = 0f;
		_targetScale = Random.Range(1f, 2f);
		base.transform.localScale = Vector3.zero;
	}
}
