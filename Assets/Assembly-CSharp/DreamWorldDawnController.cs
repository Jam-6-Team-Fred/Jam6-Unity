using UnityEngine;

public class DreamWorldDawnController : SectoredMonoBehaviour
{
	[SerializeField]
	private OWRenderer _renderer;

	[SerializeField]
	private AnimationCurve _brightnessCurve = AnimationCurve.EaseInOut(0f, 0f, 25f, 1f);

	private Color _baseColor;

	protected override void Awake()
	{
		base.Awake();
		_baseColor = _renderer.GetOriginalColor();
		float num = _brightnessCurve.Evaluate(0f);
		_renderer.SetColor(new Color(_baseColor.r * num, _baseColor.g * num, _baseColor.b * num, _baseColor.a));
		base.enabled = false;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsOccupant(DynamicOccupant.Player);
	}

	private void Update()
	{
		float num = _brightnessCurve.Evaluate(TimeLoop.GetMinutesElapsed());
		_renderer.SetColor(new Color(_baseColor.r * num, _baseColor.g * num, _baseColor.b * num, _baseColor.a));
	}
}
