using UnityEngine;

[RequireComponent(typeof(SlideCollectionContainer))]
public class MindSlideCollection : MonoBehaviour
{
	[SerializeField]
	private float _defaultSlideDuration = 0.7f;

	private SlideCollectionContainer _slideCollectionContainer;

	public float defaultSlideDuration => _defaultSlideDuration;

	public SlideCollectionContainer slideCollectionContainer
	{
		get
		{
			if (_slideCollectionContainer == null)
			{
				_slideCollectionContainer = GetComponent<SlideCollectionContainer>();
			}
			return _slideCollectionContainer;
		}
	}
}
