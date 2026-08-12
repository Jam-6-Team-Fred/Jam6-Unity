using UnityEngine;

public class VoidShadowEffectController : MonoBehaviour
{
	public delegate void VoidShadowEffectEvent();

	[SerializeField]
	private OWRenderer[] _objectRenderers = new OWRenderer[0];

	[SerializeField]
	private OWRenderer[] _objectVoidShadowRenderers = new OWRenderer[0];

	[SerializeField]
	private AnimationCurve _objectDissolveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private Transform _voidCracksParent;

	[SerializeField]
	private Vector3 _voidCracksParentOffset = Vector3.zero;

	private int _propID_Dissolve;

	private bool _isPlaying;

	private bool _isComplete;

	private float _effectLength;

	private float _effectStartTime;

	public bool isPlaying => _isPlaying;

	public bool isComplete => _isComplete;

	public Transform voidCracksParent => _voidCracksParent;

	public Vector3 voidCracksParentOffset => _voidCracksParentOffset;

	public event VoidShadowEffectEvent OnEffectComplete;

	private void Awake()
	{
		_propID_Dissolve = Shader.PropertyToID("_Dissolve");
		float value = _objectDissolveCurve.Evaluate(0f);
		for (int i = 0; i < _objectRenderers.Length; i++)
		{
			_objectRenderers[i].SetMaterialProperty(_propID_Dissolve, value);
		}
		if (_voidCracksParent == null)
		{
			_voidCracksParent = base.transform;
		}
		for (int j = 0; j < _objectVoidShadowRenderers.Length; j++)
		{
			_objectVoidShadowRenderers[j].SetActivation(active: false);
		}
		if (_objectDissolveCurve.length > 0)
		{
			_effectLength = _objectDissolveCurve.keys[_objectDissolveCurve.length - 1].time;
		}
		base.enabled = false;
	}

	private void Update()
	{
		if (!_isPlaying || _isComplete)
		{
			base.enabled = false;
			return;
		}
		float num = Time.timeSinceLevelLoad - _effectStartTime;
		float value = _objectDissolveCurve.Evaluate(num);
		for (int i = 0; i < _objectRenderers.Length; i++)
		{
			_objectRenderers[i].SetMaterialProperty(_propID_Dissolve, value);
		}
		if (num >= _effectLength)
		{
			_isComplete = true;
			if (this.OnEffectComplete != null)
			{
				this.OnEffectComplete();
			}
		}
	}

	public void PlayEffect()
	{
		for (int i = 0; i < _objectVoidShadowRenderers.Length; i++)
		{
			_objectVoidShadowRenderers[i].SetActivation(active: true);
		}
		_isPlaying = true;
		_isComplete = false;
		_effectStartTime = Time.timeSinceLevelLoad;
		base.enabled = true;
	}

	private void OnDrawGizmos()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.red;
			Gizmos.matrix = ((_voidCracksParent != null) ? _voidCracksParent.localToWorldMatrix : base.transform.localToWorldMatrix);
			Gizmos.DrawLine(_voidCracksParentOffset + Vector3.down, _voidCracksParentOffset + Vector3.up);
			Gizmos.DrawLine(_voidCracksParentOffset + Vector3.back, _voidCracksParentOffset + Vector3.forward);
			Gizmos.DrawLine(_voidCracksParentOffset + Vector3.left, _voidCracksParentOffset + Vector3.right);
		}
	}
}
