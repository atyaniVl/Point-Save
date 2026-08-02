using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GenericSceneManagement;
using ZombieDiner.Core;

namespace ZombieDiner.UI
{
    public class ResultScreenManager : MonoBehaviour
    {
        [Header("Stat Text Components")]
        [SerializeField] private TextMeshProUGUI servedPeopleText;
        [SerializeField] private TextMeshProUGUI waveReachedText;
        [SerializeField] private TextMeshProUGUI collectedCoinsText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "Main menu";
        [SerializeField] private string gameplaySceneName = "Core Game";

        private void Start()
        {
            DisplayResults();
            SetupButtons();
            _ = QwacksLeaderboardManager.EnsureAuthenticatedAsync();
        }

        private void DisplayResults()
        {
            if (servedPeopleText != null)
            {
                servedPeopleText.text = $"Served People: {SessionStats.ServedPeopleCount}";
            }

            if (waveReachedText != null)
            {
                waveReachedText.text = $"Wave Reached: {SessionStats.WaveReached}";
            }

            if (collectedCoinsText != null)
            {
                collectedCoinsText.text = $"Collected Coins: ${SessionStats.CollectedCoins}";
            }
        }

        private void SetupButtons()
        {
            if (playAgainButton != null)
            {
                playAgainButton.onClick.RemoveAllListeners();
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        public void OnPlayAgainClicked()
        {
            Time.timeScale = 1f;
            SessionStats.ResetStats();
            Debug.Log($"<color=green>[ResultScreenManager] Reloading gameplay scene: {gameplaySceneName}</color>");
            SceneLoader.Load(gameplaySceneName);
        }

        public void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            SessionStats.ResetStats();
            Debug.Log($"<color=yellow>[ResultScreenManager] Loading main menu scene: {mainMenuSceneName}</color>");
            SceneLoader.Load(mainMenuSceneName);
        }
    }
}
