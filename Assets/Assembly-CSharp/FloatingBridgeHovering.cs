using System.Collections.Generic;
using UnityEngine;

public class FloatingBridgeHovering : MonoBehaviour
{
	private enum BridgeStatus
	{
		DOWN = 0,
		UP = 1,
		RISING = 2,
		LOWERING = 3
	}

	[SerializeField]
	private float _hoverHeight;

	[SerializeField]
	private float _risingSpeed = 1.5f;

	[SerializeField]
	private bool _startsHovering;

	[SerializeField]
	private GameObject _detector;

	[SerializeField]
	private List<OWTriggerVolume> _trigger;

	private BridgeStatus _status;

	private void Awake()
	{
		base.enabled = false;
		if (_startsHovering)
		{
			_detector.transform.SetLocalPositionZ(0f - _hoverHeight);
			_status = BridgeStatus.UP;
		}
		else
		{
			_status = BridgeStatus.DOWN;
		}
		for (int i = 0; i < _trigger.Count; i++)
		{
			_trigger[i].OnEntry += TriggerBridgeUp;
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _trigger.Count; i++)
		{
			_trigger[i].OnEntry -= TriggerBridgeUp;
		}
	}

	private void FixedUpdate()
	{
		switch (_status)
		{
		case BridgeStatus.RISING:
			_detector.transform.SetLocalPositionZ(_detector.transform.localPosition.z - _risingSpeed * Time.deltaTime);
			if (_detector.transform.localPosition.z < 0f - _hoverHeight)
			{
				base.enabled = false;
				_detector.transform.SetLocalPositionZ(0f - _hoverHeight);
				_status = BridgeStatus.UP;
			}
			break;
		case BridgeStatus.LOWERING:
			_detector.transform.SetLocalPositionZ(_detector.transform.localPosition.z + _risingSpeed * Time.deltaTime);
			if (_detector.transform.localPosition.z > 0f)
			{
				base.enabled = false;
				_detector.transform.SetLocalPositionZ(0f);
				_status = BridgeStatus.DOWN;
			}
			break;
		}
	}

	private void TriggerBridgeUp(GameObject input)
	{
		if (input.CompareTag("PlayerDetector") && _status != BridgeStatus.UP)
		{
			base.enabled = true;
			_status = BridgeStatus.RISING;
		}
	}

	private void TriggerBridgeDown(GameObject input)
	{
		if (input.CompareTag("PlayerDetector") && _status != 0)
		{
			base.enabled = true;
			_status = BridgeStatus.LOWERING;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_detector != null && _detector.GetComponent<BoxCollider>() != null) // CHANGED
		{
			Gizmos.color = Color.cyan;
			Gizmos.matrix = Matrix4x4.TRS(_detector.transform.position, _detector.transform.rotation, Vector3.one);
			Gizmos.DrawWireCube(new Vector3(0f, 0f, _hoverHeight), _detector.GetComponent<BoxCollider>().size);
		}
	}
}
