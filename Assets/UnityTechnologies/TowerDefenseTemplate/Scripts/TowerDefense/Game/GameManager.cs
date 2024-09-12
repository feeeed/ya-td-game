using Core.Data;
using Core.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

namespace TowerDefense.Game
{
	/// <summary>
	/// Game Manager - a persistent single that handles persistence, and level lists, etc.
	/// This should be initialized when the game starts.
	/// </summary>
	public class GameManager : GameManagerBase<GameManager, GameDataStore>
	{
		/// <summary>
		/// Scriptable object for list of levels
		/// </summary>
		public LevelList levelList;

		internal static int CurrLang;
		internal static System.Action<int> OnLanguageChange;
		bool isAppFocused;

		void OnApplicationFocus(bool hasFocus)
		{
			isAppFocused = hasFocus;
			UpdateGameFocus();
		}

		void OnApplicationPause(bool isPaused)
		{
			isAppFocused = !isPaused;
			UpdateGameFocus();
		}


		void Update()
		{
			UpdateGameFocus();
		}

		void UpdateGameFocus()
		{
			var shouldStopApp = !isAppFocused;

			if (YandexGame.nowAdsShow)
				shouldStopApp = true;

			AudioListener.pause = shouldStopApp;
			AudioListener.volume = shouldStopApp ? 0 : 1;
		}

		

		/// <summary>
		/// Set sleep timeout to never sleep
		/// </summary>
		protected override void Awake()
		{
			CurrLang = YandexGame.lang == "en" ? 1 : 0;
			//CurrLang = 1;

			Application.runInBackground = false;
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			base.Awake();
		}
		protected override void Start()
		{
			SetLanguage(CurrLang);
			base.Start();
		}

		[ContextMenu("Switch Language")]
		public void SwitchLanguage()
		{
			if (CurrLang == 0)
				CurrLang = 1;
			else
				CurrLang = 0;
			SetLanguage(CurrLang);
		}
		void SetLanguage(int lang)
		{
			CurrLang = lang;
			OnLanguageChange?.Invoke(lang);
		}

		/// <summary>
		/// Method used for completing the level
		/// </summary>
		/// <param name="levelId">The levelId to mark as complete</param>
		/// <param name="starsEarned"></param>
		public void CompleteLevel(string levelId, int starsEarned)
		{
			if (!levelList.ContainsKey(levelId))
			{
				Debug.LogWarningFormat("[GAME] Cannot complete level with id = {0}. Not in level list", levelId);
				return;
			}

			m_DataStore.CompleteLevel(levelId, starsEarned);
			SaveData();
		}

		/// <summary>
		/// Gets the id for the current level
		/// </summary>
		public LevelItem GetLevelForCurrentScene()
		{
			string sceneName = SceneManager.GetActiveScene().name;

			return levelList.GetLevelByScene(sceneName);
		}

		/// <summary>
		/// Determines if a specific level is completed
		/// </summary>
		/// <param name="levelId">The level ID to check</param>
		/// <returns>true if the level is completed</returns>
		public bool IsLevelCompleted(string levelId)
		{
			if (!levelList.ContainsKey(levelId))
			{
				Debug.LogWarningFormat("[GAME] Cannot check if level with id = {0} is completed. Not in level list", levelId);
				return false;
			}

			return m_DataStore.IsLevelCompleted(levelId);
		}

		/// <summary>
		/// Gets the stars earned on a given level
		/// </summary>
		/// <param name="levelId"></param>
		/// <returns></returns>
		public int GetStarsForLevel(string levelId)
		{
			if (!levelList.ContainsKey(levelId))
			{
				Debug.LogWarningFormat("[GAME] Cannot check if level with id = {0} is completed. Not in level list", levelId);
				return 0;
			}

			return m_DataStore.GetNumberOfStarForLevel(levelId);
		}
	}
}