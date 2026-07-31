using System;
using UnityEngine;

namespace ZombieDiner.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ========================================================================
        // OBSERVER PATTERN EVENTS
        // ========================================================================

        /// <summary>
        /// Fired whenever the game stage changes
        /// (Stage1, Cutscene, Stage2, GameOver).
        /// </summary>
        public static event Action<GameStage> OnStageChanged;

        /// <summary>
        /// Fired when the game enters the Game Over state.
        /// </summary>
        public static event Action OnGameOverTriggered;

        // ========================================================================
        // PROPERTIES
        // ========================================================================

        public GameStage CurrentStage { get; private set; } = GameStage.Stage1_Normal;

        /// <summary>
        /// Returns true while the player is actively playing
        /// (either the Human stage or the Zombie stage).
        /// </summary>
        public bool IsPlaying =>
            CurrentStage == GameStage.Stage1_Normal ||
            CurrentStage == GameStage.Stage2_Zombie;

        private void Awake()
        {
            // Safe Singleton initialization
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Start the game in Stage 1 when the scene loads
            ChangeStage(GameStage.Stage1_Normal);
        }

        /// <summary>
        /// Changes the current game stage.
        /// For example, this can be called after the cutscene
        /// to transition to Stage 2.
        /// </summary>
        public void ChangeStage(GameStage newStage)
        {
            CurrentStage = newStage;
            Debug.Log($"[GameManager] Game Stage Transitioned To: {newStage}");

            // Notify all subscribed systems about the stage change
            OnStageChanged?.Invoke(CurrentStage);
        }

        /// <summary>
        /// Triggers the Game Over state.
        /// For example, this can be called when an order timer expires.
        /// </summary>
        public void TriggerGameOver()
        {
            if (CurrentStage == GameStage.GameOver)
                return;

            Debug.Log("[GameManager] Game Over Condition Met!");
            CurrentStage = GameStage.GameOver;

            OnGameOverTriggered?.Invoke();
            OnStageChanged?.Invoke(GameStage.GameOver);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}