using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.Xbox
{
	public class Gdk : MonoBehaviour
	{
		public delegate void OnGameSaveSucceededHandler(object sender, string blobName);

		public delegate void OnGameSaveFailedHandler(object sender, string blobName);

		public delegate void OnGameSaveLoadedHandler(object sender, string blobName, GameSaveLoadedArgs e);

		public delegate void OnGameSaveLoadFailedHandler(object sender, string blobName);

		public delegate void OnErrorHandler(object sender, ErrorEventArgs e);

		[Header("You can find the value of the scid in your MicrosoftGame.config")]
		public string scid;

		public bool signInOnStart = true;

		private static Gdk _xboxHelpers;

		private static bool _initialized;

		private static Dictionary<int, string> _hresultToFriendlyErrorLookup;

		private const int _100PercentAchievementProgress = 100;

		private const string _GameSaveContainerName = "GameSave";

		private const int _MaxAssociatedProductsToRetrieve = 25;

		private string _cachedGamertag;

		private static Gdk _xboxHelperInstance;

		public string currentGamertag => _cachedGamertag;

		public static Gdk Helpers
		{
			get
			{
				if (_xboxHelperInstance == null)
				{
					_xboxHelperInstance = Object.FindObjectOfType<Gdk>();
					if (_xboxHelperInstance != null)
					{
						_xboxHelperInstance._Initialize();
					}
				}
				return _xboxHelperInstance;
			}
		}

		public event OnGameSaveSucceededHandler OnGameSaveSucceeded;

		public event OnGameSaveFailedHandler OnGameSaveFailed;

		public event OnGameSaveLoadedHandler OnGameSaveLoaded;

		public event OnGameSaveLoadFailedHandler OnGameSaveLoadFailed;

		public event OnErrorHandler OnError;

		private void Awake()
		{
			if (_xboxHelperInstance == null)
			{
				_xboxHelperInstance = this;
			}
			else if (_xboxHelperInstance != this)
			{
				Object.Destroy(base.gameObject);
			}
		}

		private void Start()
		{
			Object.Destroy(base.gameObject);
		}

		private void _Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;
				Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		private void InitializeHresultToFriendlyErrorLookup()
		{
			_hresultToFriendlyErrorLookup.Add(-2143330041, "IAP_UNEXPECTED: Does the player you are signed in as have a license for the game? You can get one by downloading your game from the store and purchasing it first. If you can't find your game in the store, have you published it in Partner Center?");
		}

		public void SignIn()
		{
		}

		public void SignOut()
		{
		}

		public void Save(byte[] data, string blobName)
		{
		}

		public void LoadSaveData(string blobName)
		{
		}

		public void UnlockAchievement(string achievementId)
		{
		}

		private void Update()
		{
		}

		protected static bool Succeeded(int hresult, string operationFriendlyName)
		{
			bool result = false;
			if (HR.SUCCEEDED(hresult))
			{
				result = true;
			}
			else
			{
				string text = hresult.ToString("X8");
				string empty = string.Empty;
				empty = ((!_hresultToFriendlyErrorLookup.ContainsKey(hresult)) ? (operationFriendlyName + " failed.") : _hresultToFriendlyErrorLookup[hresult]);
				_LogError($"{empty} Error code: hr=0x{text}");
				if (Helpers.OnError != null)
				{
					Helpers.OnError(Helpers, new ErrorEventArgs(text, empty));
				}
			}
			return result;
		}

		private static void _LogError(string message)
		{
			Debug.Log(message);
		}
	}
}
