using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputAction
{
	InputActionPhase Phase { get; }

	InputControl ActiveControl { get; }

	bool HasActiveMouseInput { get; }

	InputConsts.InputValueType ValueType { get; }

	AxisIdentifier AxisID { get; }

	event Action OnStarted;

	event Action OnPerformed;

	event Action OnCancelled;

	void Enable(bool enable);

	void Update();

	bool HasSameBinding(IInputAction toCompare, bool usingGamepad);

	List<Texture2D> GetUITextures(bool gamepad, bool combineImagesWhenPossible, bool forceRefresh = false);
}
public interface IInputAction<T> : IInputAction where T : struct
{
	T Value { get; }
}
