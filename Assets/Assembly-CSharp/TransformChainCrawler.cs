using UnityEngine;

public class TransformChainCrawler : MonoBehaviour
{
	[SerializeField]
	private float _moveSpeed = 1f;

	private Transform[] _chain;

	private int _cur;

	public float moveSpeed
	{
		get
		{
			return _moveSpeed;
		}
		set
		{
			_moveSpeed = value;
		}
	}

	private void Awake()
	{
		base.enabled = false;
	}

	public void Play()
	{
		base.enabled = true;
	}

	public void Pause()
	{
		base.enabled = false;
	}

	public void Stop()
	{
		base.enabled = false;
		ResetToStart();
	}

	public void ResetToStart()
	{
		_cur = 0;
		base.transform.SetParent(_chain[0]);
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
	}

	public void SetChain(Transform[] chain)
	{
		_chain = chain;
		ResetToStart();
	}

	public void SetChainRoot(Transform root)
	{
		int num = 1;
		Transform transform = root;
		while (transform != null && transform.childCount > 0)
		{
			num++;
			transform = transform.GetChild(0);
		}
		_chain = new Transform[num];
		transform = root;
		for (int i = 0; i < num; i++)
		{
			_chain[i] = transform;
			if (transform.childCount > 0)
			{
				transform = transform.GetChild(0);
			}
		}
		ResetToStart();
	}

	public void Update()
	{
		if (_chain == null || _cur >= _chain.Length - 1)
		{
			base.enabled = false;
			return;
		}
		Transform transform = _chain[_cur + 1];
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, transform.localPosition, _moveSpeed * Time.deltaTime);
		float num = Vector3.Distance(base.transform.localPosition, transform.localPosition);
		float t = 1f - Mathf.Clamp01(num / transform.localPosition.magnitude);
		base.transform.localRotation = Quaternion.Lerp(Quaternion.identity, transform.localRotation, t);
		if (num < 0.001f)
		{
			_cur++;
			base.transform.SetParent(_chain[_cur]);
		}
	}
}
