using UnityEngine;

public class FlashbackScreenGrabImageEffect : MonoBehaviour
{
	public Shader _downsampleShader;

	private Material _downsampleMaterial;

	private RenderTexture _targetRT;

	private void Awake()
	{
		_downsampleMaterial = new Material(_downsampleShader);
		base.enabled = false;
	}

	public void QueueScreenGrab(RenderTexture target)
	{
		base.enabled = true;
		_targetRT = target;
	}

	public void CancelScreenGrab()
	{
		base.enabled = false;
		_targetRT = null;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_targetRT != null)
		{
			int num = Mathf.RoundToInt(Mathf.Log(Mathf.ClosestPowerOfTwo(source.height / _targetRT.height), 2f));
			if (num < 1)
			{
				Graphics.Blit(source, _targetRT);
			}
			else
			{
				RenderTexture renderTexture = source;
				RenderTexture renderTexture2 = null;
				for (int i = 0; i < num - 1; i++)
				{
					renderTexture2 = RenderTexture.GetTemporary(renderTexture.width / 2, renderTexture.height / 2, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
					Graphics.Blit(renderTexture, renderTexture2, _downsampleMaterial);
					if (i != 0)
					{
						RenderTexture.ReleaseTemporary(renderTexture);
					}
					renderTexture = renderTexture2;
				}
				Graphics.Blit(renderTexture, _targetRT, _downsampleMaterial);
				if (num > 1)
				{
					RenderTexture.ReleaseTemporary(renderTexture);
				}
			}
		}
		Graphics.Blit(source, destination);
		base.enabled = false;
		_targetRT = null;
	}
}
