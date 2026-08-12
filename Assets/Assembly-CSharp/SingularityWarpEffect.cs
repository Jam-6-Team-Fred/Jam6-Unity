using UnityEngine;

public class SingularityWarpEffect : MonoBehaviour
{
	public delegate void WarpCompleteEvent();

	[SerializeField]
	private SingularityController _singularity;

	[SerializeField]
	private GameObject _warpedObjectGeometry;

	private float _singularityCreationLength;

	private float _singularityCollapseLength;

	private float _warpLength;

	private float _timer;

	private bool _warpingIn;

	private Vector3 _origScale;

	public SingularityController singularityController => _singularity;

	public event WarpCompleteEvent OnWarpComplete;

	private void Awake()
	{
		_singularityCreationLength = _singularity.GetCreationLength();
		_singularityCollapseLength = _singularity.GetCollapseLength();
		_origScale = _warpedObjectGeometry.transform.localScale;
		base.enabled = false;
	}

	private void LateUpdate()
	{
		_timer += Time.deltaTime;
		float num = Mathf.Clamp01(_timer / _warpLength);
		num *= num;
		if (!_warpingIn)
		{
			num = 1f - num;
		}
		_warpedObjectGeometry.transform.localScale = _origScale * num;
	}

	public void WarpObjectIn(float length)
	{
		float num = Mathf.Max(length - (_singularityCreationLength + _singularityCollapseLength), 0f);
		_timer = 0f;
		_warpLength = _singularityCreationLength + _singularityCollapseLength + num;
		_warpingIn = true;
		base.enabled = true;
		_warpedObjectGeometry.transform.localScale = Vector3.zero;
		_singularity.OnCollapse += OnSingularityCollapse;
		_singularity.CreateWithLifetime(num);
	}

	public void WarpObjectOut(float length)
	{
		float num = Mathf.Max(length - (_singularityCreationLength + _singularityCollapseLength), 0f);
		_timer = 0f;
		_warpLength = _singularityCreationLength + _singularityCollapseLength + num;
		_warpingIn = false;
		base.enabled = true;
		_warpedObjectGeometry.transform.localScale = _origScale;
		_singularity.OnCollapse += OnSingularityCollapse;
		_singularity.CreateWithLifetime(num);
	}

	private void OnSingularityCollapse()
	{
		_singularity.OnCollapse -= OnSingularityCollapse;
		base.enabled = false;
		_warpedObjectGeometry.transform.localScale = _origScale;
		if (this.OnWarpComplete != null)
		{
			this.OnWarpComplete();
		}
	}
}
