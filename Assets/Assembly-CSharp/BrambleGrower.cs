using UnityEngine;

public class BrambleGrower : MonoBehaviour
{
	private float _initScale = 0.5f;

	private float _finalScale = 2f;

	private float _growDuration = 5f;

	private float _initGrowTime;

	private OWCollider _owCollider;

	private void Awake()
	{
		_owCollider = base.gameObject.GetComponent<OWCollider>();
		if (_owCollider == null)
		{
			_owCollider = base.gameObject.AddComponent<OWCollider>();
		}
		base.transform.localScale = new Vector3(1f, _initScale, 1f);
	}

	private void Start()
	{
		base.enabled = false;
	}

	public void Grow()
	{
		_owCollider.BeginScaling();
		_initGrowTime = Time.time;
		base.enabled = true;
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.time - _initGrowTime) / _growDuration);
		base.transform.localScale = new Vector3(1f, _initScale + (_finalScale - _initScale) * num, 1f);
		if (num >= 1f)
		{
			base.transform.localScale = new Vector3(1f, _finalScale, 1f);
			base.enabled = false;
			_owCollider.EndScaling();
		}
	}
}
