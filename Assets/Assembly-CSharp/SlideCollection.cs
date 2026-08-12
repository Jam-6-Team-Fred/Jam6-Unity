using System;
using UnityEngine;

[Serializable]
public class SlideCollection
{
	[SerializeField]
	public string streamingAssetIdentifier;

	[SerializeField]
	public Slide[] slides;

	private StreamingIteratedTextureAssetBundle _textureAssetBundle;

	private bool _isVision;

	public Slide this[int i] => slides[i];

	public bool isVision
	{
		get
		{
			return _isVision;
		}
		set
		{
			_isVision = value;
		}
	}

	public SlideCollection(int startArrSize)
	{
		slides = new Slide[startArrSize];
	}

	public StreamingIteratedTextureAssetBundle GetAssetBundle()
	{
		return _textureAssetBundle;
	}

	public void SetAssetBundle(StreamingIteratedTextureAssetBundle tab)
	{
		Debug.Log("receiving texture bundle");
		_textureAssetBundle = tab;
		SetupStreaming();
	}

	public void DereferenceAssetBundle()
	{
		_textureAssetBundle = null;
	}

	public void SetupStreaming()
	{
		for (int i = 0; i < slides.Length; i++)
		{
			slides[i].SetupStreamingIndex(i);
		}
	}

	public void RequestStreamSlides(params int[] slideIndices)
	{
		int[] array = new int[slideIndices.Length];
		for (int i = 0; i < slideIndices.Length; i++)
		{
			array[i] = slides[slideIndices[i]].GetStreamingIndex();
		}
		_textureAssetBundle.LoadTexturesManual(array);
	}

	public void RequestRelease(params int[] slideIndices)
	{
		int[] array = new int[slideIndices.Length];
		for (int i = 0; i < slideIndices.Length; i++)
		{
			array[i] = slides[slideIndices[i]].GetStreamingIndex();
		}
		_textureAssetBundle.ReleaseStreamsManual(array);
	}

	public bool IsStreamedSlideIndexLoaded(int slideIndex)
	{
		int streamingIndex = slides[slideIndex].GetStreamingIndex();
		if (streamingIndex < 0)
		{
			return true;
		}
		return IsStreamedTextureIndexLoaded(streamingIndex);
	}

	public bool IsStreamedTextureIndexLoaded(int streamIdx)
	{
		if (_textureAssetBundle == null)
		{
			return false;
		}
		return _textureAssetBundle.IsTextureAvailable(streamIdx);
	}

	public Texture GetStreamingTexture(int id)
	{
		return _textureAssetBundle.GetTexture(id);
	}
}
