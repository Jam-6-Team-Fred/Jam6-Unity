using UnityEngine;
using UnityEngine.InputSystem;

public class TrailerRailController : MonoBehaviour
{
	[SerializeField]
	private Animation _animation;

	[SerializeField]
	private PlayerAttachPoint _attachPoint;

	private bool _repositionPlayer;

	private bool _readyToPlay;

	private void Update()
	{
		bool flag = false;
		if (Keyboard.current != null)
		{
			flag = InputTransitionUtil.TryGetKey(KeyCode.J, out var key) && Keyboard.current[key].wasPressedThisFrame;
		}
		if (flag)
		{
			if (_readyToPlay)
			{
				Play();
			}
			else
			{
				_repositionPlayer = true;
			}
		}
	}

	private void FixedUpdate()
	{
		if (_repositionPlayer)
		{
			_repositionPlayer = false;
			_readyToPlay = true;
			GUIMode.SetRenderMode(GUIMode.RenderMode.Hidden);
			Locator.GetPlayerBody().WarpToPositionRotation(_attachPoint.transform.position, _attachPoint.transform.rotation);
			_attachPoint.AttachPlayer();
		}
	}

	private void Play()
	{
		_animation.Play();
		_animation.Sample();
	}
}
