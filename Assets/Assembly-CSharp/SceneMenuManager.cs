using UnityEngine;

public class SceneMenuManager : MonoBehaviour
{
	private PauseMenuManager _pauseMenu;

	private SuitMenuManager _suitMenu;

	public PauseMenuManager pauseMenu => _pauseMenu;

	public SuitMenuManager suitMenu => _suitMenu;

	public void RegisterPauseMenu(PauseMenuManager menuMgr)
	{
		_pauseMenu = menuMgr;
	}

	public void RegisterSuitMenu(SuitMenuManager menuMgr)
	{
		Debug.Log("Registered Suit Menu");
		_suitMenu = menuMgr;
	}
}
