using UnityEngine;
using UnityEngine.UI;

public class FadeChildren : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 1f)]
	private float _fade;

	private Graphic[] _childGraphics;

	public float fade
	{
		get
		{
			return _fade;
		}
		set
		{
			_fade = Mathf.Clamp01(value);
		}
	}

	private void Update()
	{
		ApplyFade();
	}

	private void ApplyFade()
	{
		if (_childGraphics == null)
		{
			InitChildren();
		}
		Graphic[] childGraphics = _childGraphics;
		foreach (Graphic obj in childGraphics)
		{
			Color color = obj.color;
			obj.color = new Color(color.r, color.g, color.b, _fade);
		}
	}

	public void InitChildren()
	{
		_childGraphics = GetComponentsInChildren<Graphic>();
	}

	public void SetChildGraphics(Graphic[] graphics)
	{
		_childGraphics = graphics;
	}
}
