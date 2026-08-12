using System.Collections.Generic;
using UnityEngine;

public abstract class ShipLogMode : MonoBehaviour
{
	public abstract void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource);

	public abstract void EnterMode(string entryID = "", List<ShipLogFact> revealQueue = null);

	public abstract void ExitMode();

	public abstract void OnEnterComputer();

	public abstract void OnExitComputer();

	public abstract void UpdateMode();

	public abstract bool AllowModeSwap();

	public abstract bool AllowCancelInput();

	public abstract string GetFocusedEntryID();

	public virtual bool IsEditMode()
	{
		return false;
	}
}
