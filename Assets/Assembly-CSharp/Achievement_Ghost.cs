using UnityEngine.SceneManagement;

public class Achievement_Ghost
{
	private static Achievement_Ghost _achievementGhost;

	public bool gotCaughtThisLoop;

	public bool[] reachedLibrary = new bool[3];

	public Achievement_Ghost()
	{
		ResetAll();
		GlobalMessenger<DeathType>.AddListener("DeathSequenceComplete", OnPlayerDeath);
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	~Achievement_Ghost()
	{
		GlobalMessenger<DeathType>.RemoveListener("DeathSequenceComplete", OnPlayerDeath);
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
	}

	public static void GotCaughtByGhost()
	{
		if (_achievementGhost == null)
		{
			_achievementGhost = new Achievement_Ghost();
		}
		_achievementGhost.gotCaughtThisLoop = true;
	}

	public static void ReachLibrary(int libraryID)
	{
		if (_achievementGhost == null)
		{
			_achievementGhost = new Achievement_Ghost();
		}
		if (libraryID > -1 && libraryID < 3)
		{
			_achievementGhost.reachedLibrary[libraryID] = true;
		}
		_achievementGhost.CheckForAchievementSuccess();
	}

	public void CheckForAchievementSuccess()
	{
		if (!gotCaughtThisLoop && reachedLibrary[0] && reachedLibrary[1] && reachedLibrary[2])
		{
			Achievements.Earn(Achievements.Type.GHOSTS);
		}
	}

	private void ResetAll()
	{
		reachedLibrary[0] = false;
		reachedLibrary[1] = false;
		reachedLibrary[2] = false;
		gotCaughtThisLoop = false;
	}

	private void OnPlayerDeath(DeathType type)
	{
		ResetAll();
	}

	private void OnSceneUnloaded(Scene scene)
	{
		ResetAll();
	}
}
