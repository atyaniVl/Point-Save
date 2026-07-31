using System;
using UnityEngine;

namespace ZombieDiner.Orders
{
    public class DifficultyScalingSystem : MonoBehaviour
    {
        public static DifficultyScalingSystem Instance { get; private set; }

        // ========================================================================
        // CONFIGURATION (Editable from the Inspector)
        // ========================================================================

        [Header("Stage 1 - Normal Diner Settings")]
        [Tooltip("Initial delivery time for Stage 1 (seconds)")]
        [SerializeField] private float stage1BaseDeliveryTime = 15f;

        [Tooltip("Minimum allowed delivery time in Stage 1")]
        [SerializeField] private float stage1MinDeliveryTime = 8f;

        [Tooltip("Amount of delivery time reduced per wave in Stage 1")]
        [SerializeField] private float stage1TimeDecreasePerWave = 0.5f;

        [Header("Stage 2 - Zombie Diner Settings")]
        [Tooltip("Initial delivery time for the Zombie stage (seconds)")]
        [SerializeField] private float stage2BaseDeliveryTime = 10f;

        [Tooltip("Minimum delivery time allowed to quickly increase difficulty")]
        [SerializeField] private float stage2MinDeliveryTime = 3f;

        [Tooltip("Amount of delivery time reduced per wave in the Zombie stage")]
        [SerializeField] private float stage2TimeDecreasePerWave = 0.8f;

        [Header("Customer Spawn Rate Settings")]
        [Tooltip("Initial interval between customer spawns (seconds)")]
        [SerializeField] private float baseSpawnInterval = 4f;

        [Tooltip("Fastest allowed customer spawn interval")]
        [SerializeField] private float minSpawnInterval = 1.2f;

        [Tooltip("Amount the spawn interval decreases per wave")]
        [SerializeField] private float spawnIntervalDecreasePerWave = 0.25f;

        // ========================================================================
        // OBSERVER PATTERN EVENTS
        // ========================================================================

        /// <summary>
        /// Fired whenever the difficulty is recalculated for a new wave.
        /// Parameters: (currentAllowedDeliveryTime, currentSpawnInterval)
        /// </summary>
        public static event Action<float, float> OnDifficultyUpdated;

        // ========================================================================
        // PROPERTIES
        // ========================================================================

        public float CurrentAllowedDeliveryTime { get; private set; }
        public float CurrentSpawnInterval { get; private set; }
        public int CurrentWave { get; private set; } = 1;

        private void Awake()
        {
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

        private void OnEnable()
        {
            // Subscribe to the GameManager stage change event
            Core.GameManager.OnStageChanged += HandleStageChanged;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            Core.GameManager.OnStageChanged -= HandleStageChanged;
        }

        private void Start()
        {
            // Calculate the initial difficulty when the game starts
            CalculateDifficulty(1);
        }

        /// <summary>
        /// Responds to stage changes from the GameManager
        /// and updates the difficulty immediately.
        /// </summary>
        private void HandleStageChanged(Core.GameStage stage)
        {
            if (stage == Core.GameStage.Stage2_Zombie)
            {
                // Reset the wave and apply Zombie stage settings
                CurrentWave = 1;
                CalculateDifficulty(CurrentWave);
            }
        }

        /// <summary>
        /// Updates the current wave and recalculates the difficulty.
        /// </summary>
        public void SetWave(int waveNumber)
        {
            CurrentWave = Mathf.Max(1, waveNumber);
            CalculateDifficulty(CurrentWave);
        }

        /// <summary>
        /// Calculates the delivery time and customer spawn rate
        /// based on the current wave and game stage.
        /// </summary>
        private void CalculateDifficulty(int wave)
        {
            bool isZombieStage = Core.GameManager.Instance != null &&
                                 Core.GameManager.Instance.CurrentStage == Core.GameStage.Stage2_Zombie;

            // Select the settings for the current stage
            float baseTime = isZombieStage ? stage2BaseDeliveryTime : stage1BaseDeliveryTime;
            float minTime = isZombieStage ? stage2MinDeliveryTime : stage1MinDeliveryTime;
            float decreaseRate = isZombieStage ? stage2TimeDecreasePerWave : stage1TimeDecreasePerWave;

            // Calculate the maximum allowed delivery time
            CurrentAllowedDeliveryTime = Mathf.Max(
                minTime,
                baseTime - ((wave - 1) * decreaseRate)
            );

            // Calculate the customer spawn interval
            CurrentSpawnInterval = Mathf.Max(
                minSpawnInterval,
                baseSpawnInterval - ((wave - 1) * spawnIntervalDecreasePerWave)
            );

            Debug.Log(
                $"[DifficultyScaling] Stage: {(isZombieStage ? "Zombie" : "Normal")} | " +
                $"Wave: {wave} | " +
                $"Max Delivery Time: {CurrentAllowedDeliveryTime}s | " +
                $"Spawn Interval: {CurrentSpawnInterval}s"
            );

            // Notify all subscribed systems about the updated difficulty
            OnDifficultyUpdated?.Invoke(CurrentAllowedDeliveryTime, CurrentSpawnInterval);
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