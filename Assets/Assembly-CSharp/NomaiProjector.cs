using UnityEngine;

public class NomaiProjector : MonoBehaviour
{
	[SerializeField]
	private NomaiInterfaceSlot _controllingSlot;

	[SerializeField]
	private MeshRenderer[] _textRenderers = new MeshRenderer[0];

	[SerializeField]
	private float _fadeLength = 1f;

	[SerializeField]
	private float _startDelay = 1f;

	[SerializeField]
	private float _staggerDelay = 0.5f;

	[SerializeField]
	private bool _startVisible;

	private float[] _textRendererDelays;

	private bool _visible;

	private void Awake()
	{
		_visible = _startVisible;
		_textRendererDelays = new float[_textRenderers.Length];
		for (int i = 0; i < _textRenderers.Length; i++)
		{
			_textRenderers[i].material.SetFloat("_Blend", _visible ? 1f : 0f);
		}
		_controllingSlot.OnSlotActivated += OnSlotActivated;
		_controllingSlot.OnSlotDeactivated += OnSlotDeactivated;
		base.enabled = _visible;
	}

	private void OnDestroy()
	{
		_controllingSlot.OnSlotActivated -= OnSlotActivated;
		_controllingSlot.OnSlotDeactivated -= OnSlotDeactivated;
	}

	private void Update()
	{
		if (_visible)
		{
			for (int i = 0; i < _textRenderers.Length; i++)
			{
				if (_textRendererDelays[i] > 0f)
				{
					_textRendererDelays[i] -= Time.deltaTime;
					break;
				}
				float @float = _textRenderers[i].material.GetFloat("_Blend");
				@float = Mathf.Clamp01(@float + Time.deltaTime / _fadeLength);
				_textRenderers[i].material.SetFloat("_Blend", @float);
			}
			return;
		}
		bool flag = true;
		for (int j = 0; j < _textRenderers.Length; j++)
		{
			float float2 = _textRenderers[j].material.GetFloat("_Blend");
			float2 = Mathf.Clamp01(float2 - Time.deltaTime / _fadeLength);
			if (float2 > 0f)
			{
				flag = false;
			}
			_textRenderers[j].material.SetFloat("_Blend", float2);
		}
		if (flag)
		{
			base.enabled = false;
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		FadeIn();
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		FadeOut();
	}

	public void FadeIn()
	{
		_visible = true;
		for (int i = 0; i < _textRenderers.Length; i++)
		{
			_textRendererDelays[i] = ((i == 0) ? _startDelay : _staggerDelay);
		}
		base.enabled = true;
	}

	public void FadeOut()
	{
		_visible = false;
	}
}
