using System.Collections;
using UnityEngine;

public class TransformAnimator : MonoBehaviour
{
	public delegate void AnimationEvent();

	private Transform _transform;

	private Vector3 _origLocalPosition;

	private bool _translating;

	private Vector3 _startLocalPosition;

	private Vector3 _targetLocalPosition;

	private float _translateDuration;

	private float _translateStartTime;

	private bool _rotating;

	private Quaternion _origLocalRotation;

	private Quaternion _startLocalRotation;

	private Quaternion _targetLocalRotation;

	private float _rotateDuration;

	private float _rotateStartTime;

	public event AnimationEvent OnTranslationComplete;

	private void Awake()
	{
		_transform = base.transform;
		_origLocalPosition = _transform.localPosition;
		_origLocalRotation = _transform.localRotation;
		base.enabled = false;
	}

	public bool IsAnimating()
	{
		if (!_translating)
		{
			return _rotating;
		}
		return true;
	}

	public void ResetToOriginalPositionRotation()
	{
		_transform.localPosition = _origLocalPosition;
		_transform.localRotation = _origLocalRotation;
	}

	public void RotateToOriginalLocalRotation(float duration)
	{
		RotateToLocalRotation(_origLocalRotation, duration);
	}

	public void RotateToLocalEulerAngles(Vector3 eulerAngle, float duration)
	{
		RotateToLocalRotation(Quaternion.Euler(eulerAngle), duration);
	}

	public void RotateAroundLocalAxis(float degrees, Vector3 localAxis, float duration)
	{
		RotateToLocalRotation(_startLocalRotation * Quaternion.AngleAxis(degrees, localAxis), duration);
	}

	public void RotateToLocalRotation(Quaternion localRotation, float duration)
	{
		_rotating = true;
		_rotateStartTime = Time.time;
		_rotateDuration = duration;
		_startLocalRotation = _transform.localRotation;
		_targetLocalRotation = localRotation;
		base.enabled = true;
	}

	public void TranslateToOriginalLocalPosition(float duration)
	{
		TranslateToLocalPosition(_origLocalPosition, duration);
	}

	public void TranslateInDirection(Vector3 direction, float duration)
	{
		TranslateToLocalPosition(base.transform.parent.InverseTransformPoint(base.transform.position + direction), duration);
	}

	public void TranslateToLocalPosition(Vector3 localPosition, float duration)
	{
		_translating = true;
		_translateStartTime = Time.time;
		_startLocalPosition = _transform.localPosition;
		_targetLocalPosition = localPosition;
		_translateDuration = duration;
		base.enabled = true;
	}

	public void ScaleTo(Vector3 targetScale, float duration)
	{
		StartCoroutine(RunScaleTo(targetScale, duration));
	}

	public void TurnTowardPosition(Vector3 targetPos, float duration, bool constrainToYAxis = true)
	{
		StartCoroutine(RunTurnTowardPosition(targetPos, duration, constrainToYAxis));
	}

	public void TurnTowardTransform(Transform targetTransform, float duration, bool constrainToYAxis = true)
	{
		StartCoroutine(RunTurnTowardTransform(targetTransform, duration, constrainToYAxis));
	}

	private void FixedUpdate()
	{
		if (_translating)
		{
			float t = Mathf.InverseLerp(_translateStartTime, _translateStartTime + _translateDuration, Time.time);
			t = Mathf.SmoothStep(0f, 1f, t);
			_transform.localPosition = Vector3.Lerp(_startLocalPosition, _targetLocalPosition, t);
			_translating = t < 1f;
			if (!_translating && this.OnTranslationComplete != null)
			{
				this.OnTranslationComplete();
			}
		}
		if (_rotating)
		{
			float t2 = Mathf.InverseLerp(_rotateStartTime, _rotateStartTime + _rotateDuration, Time.time);
			t2 = Mathf.SmoothStep(0f, 1f, t2);
			_transform.localRotation = Quaternion.Slerp(_startLocalRotation, _targetLocalRotation, t2);
			_rotating = t2 < 1f;
		}
		if (!_translating && !_rotating)
		{
			base.enabled = false;
		}
	}

	private IEnumerator RunScaleTo(Vector3 targetScale, float duration)
	{
		float startTime = Time.time;
		Vector3 startScale = base.transform.localScale;
		while (true)
		{
			float t = (Time.time - startTime) / duration;
			t = Mathf.SmoothStep(0f, 1f, t);
			_transform.localScale = Vector3.Lerp(startScale, targetScale, t);
			if (!(t >= 1f))
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator RunTurnTowardPosition(Vector3 targetPos, float duration, bool constrainToYAxis)
	{
		float startTime = Time.time;
		Quaternion startRotation = _transform.localRotation;
		Vector3 vector = targetPos - _transform.position;
		Vector3 vector2 = (constrainToYAxis ? (vector - Vector3.Project(vector, _transform.up)) : vector);
		float angle = Vector3.Angle(_transform.forward, vector2) * Mathf.Sign(Vector3.Dot(vector2, _transform.right));
		Quaternion finalRotation = Quaternion.AngleAxis(angle, Vector3.up) * _transform.localRotation;
		while (true)
		{
			float t = (Time.time - startTime) / duration;
			t = Mathf.SmoothStep(0f, 1f, t);
			_transform.localRotation = Quaternion.Slerp(startRotation, finalRotation, t);
			if (!(t >= 1f))
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator RunTurnTowardTransform(Transform targetTransform, float duration, bool constrainToYAxis)
	{
		float startTime = Time.time;
		Quaternion startRotation = _transform.localRotation;
		while (true)
		{
			float t = (Time.time - startTime) / duration;
			t = Mathf.SmoothStep(0f, 1f, t);
			Vector3 vector = targetTransform.position - _transform.position;
			Vector3 vector2 = (constrainToYAxis ? (vector - Vector3.Project(vector, _transform.up)) : vector);
			Quaternion b = Quaternion.AngleAxis(Vector3.Angle(_transform.forward, vector2) * Mathf.Sign(Vector3.Dot(vector2, _transform.right)), Vector3.up) * _transform.localRotation;
			_transform.localRotation = Quaternion.Slerp(startRotation, b, t);
			if (!(t >= 1f))
			{
				yield return null;
				continue;
			}
			break;
		}
	}
}
