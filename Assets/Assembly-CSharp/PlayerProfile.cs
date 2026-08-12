using System;

[Serializable]
public class PlayerProfile
{
	private int _id;

	private string _displayName;

	private bool _debugProfile;

	public int ID => _id;

	public string DisplayName => _displayName;

	public PlayerProfile(bool debugProfile = false)
	{
		_id = -1;
		_displayName = "NOT_INITIALIZED";
		_debugProfile = debugProfile;
	}

	public void Initialize(int id, string displayName)
	{
		_id = id;
		_displayName = displayName;
	}

	public bool IsDebugProfile()
	{
		return _debugProfile;
	}
}
