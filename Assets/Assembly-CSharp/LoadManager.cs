using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadManager : MonoBehaviour, IPermanentManagerWorker
{
	public enum FadeType
	{
		None = 0,
		ToBlack = 1,
		ToWhite = 2
	}

	private class LoadSceneJob
	{
		public OWScene sceneToLoad;

		public FadeType fadeType;

		public float fadeLength;

		public bool pauseDuringFade;

		public bool asyncOperation;

		public bool allowAsyncTransition;

		public bool skipPreLoadMemoryDump;

		public bool skipVsyncChange;
	}

	public delegate void SceneLoadEvent(OWScene originalScene, OWScene loadScene);

	private static LoadManager s_instance = null;

	private static OWScene s_previousScene = OWScene.None;

	private static OWScene s_currentScene = OWScene.None;

	private static OWScene s_loadingScene = OWScene.None;

	private static AsyncOperation s_sceneLoadOperation = null;

	private static bool s_allowAsyncTransition = false;

	private static bool s_waitingForAsyncTransition = false;

	private static FadeType s_fadeType = FadeType.None;

	private static float s_fadeStartTime = 0f;

	private static float s_fadeLength = 0f;

	private static bool s_pauseDuringFade = false;

	private static bool s_skipVsyncChange = false;

	private static int s_lastVSyncCount = -1;

	private static LoadSceneJob s_loadSceneJob = null;

	[SerializeField]
	private Canvas _fadeCanvas;

	[SerializeField]
	private Image _fadeImage;

	private Color _fadeColor;

	private static bool s_sceneReloadWaitingForStreamingMgr;

	private static bool s_sceneLoadWaitingForStreamingMgr;

	public static event SceneLoadEvent OnStartSceneLoad;

	public static event SceneLoadEvent OnCompleteSceneLoad;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void InitializeOnSceneLoad()
	{
		s_currentScene = NameToScene(SceneManager.GetActiveScene().name);
		LateInitializerManager.pauseOnInitialization = s_currentScene == OWScene.SolarSystem || s_currentScene == OWScene.EyeOfTheUniverse || s_currentScene == OWScene.Undefined;
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	private static void UnloadUnusedAssets()
	{
		GC.Collect();
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (LoadManager.OnCompleteSceneLoad != null)
		{
			LoadManager.OnCompleteSceneLoad(s_previousScene, s_currentScene);
		}
	}

	private static void OnSceneUnloaded(Scene scene)
	{
	}

	private static void OnActiveSceneChanged(Scene prevScene, Scene newScene)
	{
		s_sceneLoadOperation = null;
		AudioListener.volume = 1f;
		if ((bool)s_instance)
		{
			s_instance._fadeCanvas.enabled = false;
			s_instance._fadeImage.color = Color.clear;
		}
		if (s_lastVSyncCount > -1)
		{
			QualitySettings.vSyncCount = s_lastVSyncCount;
			s_lastVSyncCount = -1;
		}
		OWTime.SetTimeScale(1f);
		OWTime.SetMaxDeltaTime(1f / 15f);
		if (s_pauseDuringFade)
		{
			OWTime.Unpause(OWTime.PauseType.Loading);
		}
		LateInitializerManager.pauseOnInitialization = s_currentScene == OWScene.SolarSystem || s_currentScene == OWScene.EyeOfTheUniverse || s_currentScene == OWScene.Undefined;
		UnloadUnusedAssets();
	}

	private static string SceneToName(OWScene scene)
	{
		switch (scene)
		{
		case OWScene.TitleScreen:
			return "TitleScreen";
		case OWScene.SolarSystem:
			return "SolarSystem";
		case OWScene.EyeOfTheUniverse:
			return "EyeOfTheUniverse";
		case OWScene.Credits_Fast:
			return "Credits_Fast";
		case OWScene.Credits_Final:
			return "Credits_Final";
		case OWScene.PostCreditsScene:
			return "PostCreditScene";
		default:
			return "Undefined";
		}
	}

	private static OWScene NameToScene(string name)
	{
		switch (name)
		{
		case "TitleScreen":
			return OWScene.TitleScreen;
		case "SolarSystem":
			return OWScene.SolarSystem;
		case "EyeOfTheUniverse":
			return OWScene.EyeOfTheUniverse;
		case "Credits_Fast":
			return OWScene.Credits_Fast;
		case "Credits_Final":
			return OWScene.Credits_Final;
		case "PostCreditScene":
			return OWScene.PostCreditsScene;
		default:
			return OWScene.Undefined;
		}
	}

	private static void LoadSceneImmediate(OWScene scene)
	{
		s_previousScene = s_currentScene;
		s_currentScene = scene;
		s_loadingScene = OWScene.None;
		SceneManager.LoadScene(SceneToName(scene));
	}

	private static void ReloadSceneImmediate()
	{
		s_previousScene = s_currentScene;
		s_loadingScene = OWScene.None;
		StreamingManager.ForcePhysicsBakeJobsComplete();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	private static void StartAsyncSceneLoad(OWScene scene)
	{
		s_loadingScene = scene;
		s_sceneLoadOperation = SceneManager.LoadSceneAsync(SceneToName(scene));
		s_sceneLoadOperation.allowSceneActivation = false;
	}

	private static void StartAsyncSceneReload()
	{
		s_loadingScene = s_currentScene;
		s_sceneLoadOperation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
		s_sceneLoadOperation.allowSceneActivation = false;
	}

	private static void FinalizeAsyncSceneLoad()
	{
		s_previousScene = s_currentScene;
		s_currentScene = s_loadingScene;
		s_loadingScene = OWScene.None;
		StreamingManager.ForcePhysicsBakeJobsComplete();
		if (!s_skipVsyncChange && QualitySettings.vSyncCount > 0 && s_lastVSyncCount < 0)
		{
			s_lastVSyncCount = QualitySettings.vSyncCount;
			QualitySettings.vSyncCount = 0;
		}
		s_sceneLoadOperation.allowSceneActivation = true;
	}

	public static void LoadScene(OWScene scene, FadeType fadeType = FadeType.None, float fadeLength = 1f, bool pauseDuringFade = true)
	{
		if (s_loadingScene != 0)
		{
			Debug.LogError(string.Concat("Tried to load scene ", scene, " while scene ", s_loadingScene, " already pending!"));
			return;
		}
		if (s_loadSceneJob != null)
		{
			Debug.LogError(string.Concat("Tried to load scene ", scene, " while scene ", s_loadSceneJob.sceneToLoad, " already pending!"));
			return;
		}
		if (scene == OWScene.None || scene == OWScene.Undefined)
		{
			throw new ArgumentException("Tried to load an undefined scene!");
		}
		s_loadSceneJob = new LoadSceneJob();
		s_loadSceneJob.sceneToLoad = scene;
		s_loadSceneJob.fadeType = fadeType;
		s_loadSceneJob.fadeLength = fadeLength;
		s_loadSceneJob.pauseDuringFade = pauseDuringFade;
		s_loadSceneJob.asyncOperation = false;
		s_loadSceneJob.skipPreLoadMemoryDump = false;
		s_loadSceneJob.skipVsyncChange = false;
	}

	private static void DoLoadScene()
	{
		s_loadingScene = s_loadSceneJob.sceneToLoad;
		s_fadeType = s_loadSceneJob.fadeType;
		s_fadeStartTime = Time.unscaledTime;
		s_fadeLength = s_loadSceneJob.fadeLength;
		s_pauseDuringFade = s_loadSceneJob.pauseDuringFade;
		s_skipVsyncChange = s_loadSceneJob.skipVsyncChange;
		if (LoadManager.OnStartSceneLoad != null)
		{
			LoadManager.OnStartSceneLoad(s_currentScene, s_loadingScene);
		}
		if (s_fadeType == FadeType.None || s_fadeLength <= 0f || s_instance == null)
		{
			LoadSceneImmediate(s_loadingScene);
		}
		else if (s_fadeType != 0)
		{
			if (s_instance != null)
			{
				s_instance._fadeCanvas.enabled = true;
				s_instance._fadeImage.color = Color.clear;
				s_instance._fadeColor = ((s_fadeType == FadeType.ToBlack) ? Color.black : Color.white);
			}
			if (s_pauseDuringFade)
			{
				OWTime.Pause(OWTime.PauseType.Loading);
			}
		}
	}

	public static void ReloadScene()
	{
		if (s_loadingScene != 0)
		{
			Debug.LogError(string.Concat("Tried to reload scene ", SceneManager.GetActiveScene().name, " while scene ", s_loadingScene, " already pending!"));
		}
		else if (s_loadSceneJob != null)
		{
			Debug.LogError(string.Concat("Tried to reload scene ", SceneManager.GetActiveScene().name, " while scene ", s_loadSceneJob.sceneToLoad, " already pending!"));
		}
		else
		{
			s_loadSceneJob = new LoadSceneJob();
			s_loadSceneJob.sceneToLoad = s_currentScene;
			s_loadSceneJob.asyncOperation = false;
			s_loadSceneJob.skipPreLoadMemoryDump = false;
			s_loadSceneJob.skipVsyncChange = false;
		}
	}

	private static void DoReloadScene()
	{
		if (LoadManager.OnStartSceneLoad != null)
		{
			LoadManager.OnStartSceneLoad(s_currentScene, s_currentScene);
		}
		ReloadSceneImmediate();
	}

	public static void LoadSceneAsync(OWScene scene, bool autoTransition, FadeType fadeType = FadeType.None, float fadeLength = 1f, bool pauseDuringFade = true)
	{
		if (s_loadingScene != 0)
		{
			Debug.LogError(string.Concat("Tried to load scene ", scene, " while scene ", s_loadingScene, " already pending!"));
			return;
		}
		if (s_loadSceneJob != null)
		{
			Debug.LogError(string.Concat("Tried to load scene ", scene, " while scene ", s_loadSceneJob.sceneToLoad, " already pending!"));
			return;
		}
		if (scene == OWScene.None || scene == OWScene.Undefined)
		{
			throw new ArgumentException("Tried to load an undefined scene!");
		}
		s_loadSceneJob = new LoadSceneJob();
		s_loadSceneJob.sceneToLoad = scene;
		s_loadSceneJob.fadeType = fadeType;
		s_loadSceneJob.fadeLength = fadeLength;
		s_loadSceneJob.pauseDuringFade = pauseDuringFade;
		s_loadSceneJob.asyncOperation = true;
		s_loadSceneJob.allowAsyncTransition = autoTransition;
		s_loadSceneJob.skipPreLoadMemoryDump = false;
		s_loadSceneJob.skipVsyncChange = false;
	}

	public static void DoLoadSceneAsync()
	{
		s_loadingScene = s_loadSceneJob.sceneToLoad;
		s_allowAsyncTransition = s_loadSceneJob.allowAsyncTransition;
		s_waitingForAsyncTransition = true;
		s_fadeType = s_loadSceneJob.fadeType;
		s_fadeStartTime = Time.unscaledTime;
		s_fadeLength = s_loadSceneJob.fadeLength;
		s_pauseDuringFade = s_loadSceneJob.pauseDuringFade;
		s_skipVsyncChange = s_loadSceneJob.skipVsyncChange;
		if (LoadManager.OnStartSceneLoad != null)
		{
			LoadManager.OnStartSceneLoad(s_currentScene, s_loadingScene);
		}
		if (StreamingManager.isBusy)
		{
			s_sceneLoadWaitingForStreamingMgr = true;
		}
		else
		{
			FinalizeDoLoadSceneAsync();
		}
	}

	private static void FinalizeDoLoadSceneAsync()
	{
		StartAsyncSceneLoad(s_loadingScene);
		if (s_instance == null)
		{
			s_sceneLoadOperation.allowSceneActivation = true;
		}
	}

	public static void ReloadSceneAsync(bool autoTransition, bool isStatueSequence = false)
	{
		if (s_loadingScene != 0)
		{
			Debug.LogError(string.Concat("Tried to reload scene ", SceneManager.GetActiveScene().name, " while scene ", s_loadingScene, " already pending!"));
		}
		else if (s_loadSceneJob != null)
		{
			Debug.LogError(string.Concat("Tried to reload scene ", SceneManager.GetActiveScene().name, " while scene ", s_loadSceneJob.sceneToLoad, " already pending!"));
		}
		else
		{
			s_loadSceneJob = new LoadSceneJob();
			s_loadSceneJob.sceneToLoad = s_currentScene;
			s_loadSceneJob.fadeType = FadeType.None;
			s_loadSceneJob.asyncOperation = true;
			s_loadSceneJob.allowAsyncTransition = autoTransition;
			s_loadSceneJob.skipPreLoadMemoryDump = isStatueSequence;
			s_loadSceneJob.skipVsyncChange = false;
		}
	}

	private static void DoReloadSceneAsync()
	{
		s_loadingScene = s_loadSceneJob.sceneToLoad;
		s_allowAsyncTransition = s_loadSceneJob.allowAsyncTransition;
		s_waitingForAsyncTransition = true;
		s_fadeType = s_loadSceneJob.fadeType;
		s_skipVsyncChange = s_loadSceneJob.skipVsyncChange;
		if (LoadManager.OnStartSceneLoad != null)
		{
			LoadManager.OnStartSceneLoad(s_currentScene, s_loadingScene);
		}
		if (StreamingManager.isBusy)
		{
			s_sceneReloadWaitingForStreamingMgr = true;
		}
		else
		{
			FinalizeDoReloadSceneAsync();
		}
	}

	private static void FinalizeDoReloadSceneAsync()
	{
		StreamingManager.ForcePhysicsBakeJobsComplete();
		StartAsyncSceneReload();
		if (s_instance == null)
		{
			s_sceneLoadOperation.allowSceneActivation = true;
		}
	}

	public static void EnableAsyncLoadTransition()
	{
		if (s_sceneLoadOperation != null)
		{
			s_allowAsyncTransition = true;
		}
	}

	public static float GetAsyncLoadProgress()
	{
		if (s_sceneLoadOperation != null)
		{
			return s_sceneLoadOperation.progress;
		}
		return 0f;
	}

	public static bool IsAsyncLoadComplete()
	{
		if (s_sceneLoadOperation != null)
		{
			return s_sceneLoadOperation.progress >= 0.9f;
		}
		return false;
	}

	public static OWScene GetCurrentScene()
	{
		return s_currentScene;
	}

	public static OWScene GetPreviousScene()
	{
		return s_previousScene;
	}

	public static OWScene GetLoadingScene()
	{
		return s_loadingScene;
	}

	public static bool IsBusy()
	{
		if (s_loadSceneJob == null)
		{
			if (s_loadingScene == OWScene.None)
			{
				return false;
			}
			return true;
		}
		if (s_loadSceneJob.sceneToLoad == OWScene.None || s_loadSceneJob.sceneToLoad == OWScene.Undefined)
		{
			return false;
		}
		return true;
	}

	public void InitializeOnAwake()
	{
		_fadeCanvas.enabled = false;
		if (s_instance != null)
		{
			Debug.LogError("More than one LoadManager in the scene!", this);
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			s_instance = this;
		}
	}

	private void Update()
	{
		if ((s_loadSceneJob == null && s_loadingScene == OWScene.None) || (s_loadSceneJob != null && s_loadSceneJob.sceneToLoad == OWScene.None) || PlayerData.IsBusy())
		{
			return;
		}
		if (s_loadSceneJob != null)
		{
			if (s_loadSceneJob.sceneToLoad == s_currentScene)
			{
				if (s_loadSceneJob.asyncOperation)
				{
					DoReloadSceneAsync();
				}
				else
				{
					DoReloadScene();
				}
			}
			else if (s_loadSceneJob.asyncOperation)
			{
				DoLoadSceneAsync();
			}
			else
			{
				DoLoadScene();
			}
			s_loadSceneJob = null;
		}
		else if (s_sceneLoadWaitingForStreamingMgr)
		{
			if (!StreamingManager.isBusy)
			{
				FinalizeDoLoadSceneAsync();
				s_sceneLoadWaitingForStreamingMgr = false;
			}
		}
		else if (s_sceneReloadWaitingForStreamingMgr)
		{
			if (!StreamingManager.isBusy)
			{
				FinalizeDoReloadSceneAsync();
				s_sceneReloadWaitingForStreamingMgr = false;
			}
		}
		else if (s_sceneLoadOperation == null)
		{
			float num = Mathf.Clamp01((Time.unscaledTime - s_fadeStartTime) / s_fadeLength);
			_fadeImage.color = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, num);
			AudioListener.volume = 1f - num;
			if (num >= 1f)
			{
				LoadSceneImmediate(s_loadingScene);
			}
		}
		else
		{
			if (!(s_sceneLoadOperation.progress >= 0.9f) || s_sceneLoadOperation.allowSceneActivation || !s_allowAsyncTransition)
			{
				return;
			}
			if (s_waitingForAsyncTransition)
			{
				s_fadeStartTime = Time.unscaledTime;
				if (s_fadeType != 0 && s_fadeLength > 0f)
				{
					_fadeCanvas.enabled = true;
					_fadeImage.color = Color.clear;
					_fadeColor = ((s_fadeType == FadeType.ToBlack) ? Color.black : Color.white);
					if (s_pauseDuringFade)
					{
						OWTime.Pause(OWTime.PauseType.Loading);
					}
				}
				s_waitingForAsyncTransition = false;
			}
			if (s_fadeType == FadeType.None || s_fadeLength <= 0f)
			{
				FinalizeAsyncSceneLoad();
				return;
			}
			float num2 = Mathf.Clamp01((Time.unscaledTime - s_fadeStartTime) / s_fadeLength);
			_fadeImage.color = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, num2);
			AudioListener.volume = 1f - num2;
			if (num2 >= 1f)
			{
				FinalizeAsyncSceneLoad();
			}
		}
	}
}
