using System;
using UnityEngine;

public abstract class CreditsSection : MonoBehaviour
{
	protected bool _isPlaying;

	protected RectTransform[] _childElements;

	public bool isPlaying => _isPlaying;

	public abstract float GetTotalTime();

	public abstract void Play();

	public abstract bool SimulateTime(float time);

	public abstract void ResetSimulate();

	public void AddChildElement(RectTransform element)
	{
		Array.Resize(ref _childElements, _childElements.Length + 1);
		_childElements[_childElements.Length - 1] = element;
	}
}
