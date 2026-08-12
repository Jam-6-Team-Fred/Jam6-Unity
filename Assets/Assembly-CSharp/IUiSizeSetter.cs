using UnityEngine;

public interface IUiSizeSetter
{
	GameObject userFriendlyParentIdObj { get; set; }

	bool readyForResize { get; }

	event ReadyForResizeEvent OnReadyForResize;

	void DoResizeAction(UITextSize textSizeSetting);

	void MarkReadyForInitialization();
}
