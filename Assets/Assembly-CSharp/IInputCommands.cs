using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputCommands
{
	InputConsts.InputCommandType CommandType { get; }

	bool IsRebindable { get; }

	InputConsts.InputValueType ValueType { get; }

	AxisIdentifier AxisID { get; }

	float PressedThreshold { get; set; }

	float Sensitivity { get; set; }

	int InversionFactor { get; set; }

	float PressDuration { get; }

	event Action OnStarted;

	event Action OnPerformed;

	event Action OnCancelled;

	bool IsNewlyPressed();

	bool IsPressed(float minPressDuration = 0f);

	bool IsNewlyReleased();

	InputControl GetActiveDevice();

	float GetValue();

	Vector2 GetAxisValue(bool useSensitivity = true);

	void BlockNextRelease();

	void ConsumeInput();

	void EnableAllActions(bool enable);

	bool HasSameBinding(IInputCommands toCompare, bool usingGamepad);

	List<Texture2D> GetUITextures(bool gamepad, bool forceRefresh = false);

	void Update();
}
