using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZombieDiner.Orders
{
    public class OrderGenerator : MonoBehaviour
    {
        public static OrderGenerator Instance { get; private set; }

        [Header("Available Item Pool")]
        [Tooltip("List of all individual ItemSO assets available in the project")]
        [SerializeField] private List<ItemSO> availableItemSOs = new List<ItemSO>();

        [Header("Difficulty Limits")]
        [Tooltip("Maximum number of distinct item types allowed in Stage 1")]
        [SerializeField] private int maxDistinctItemsStage1 = 2;

        [Tooltip("Maximum number of distinct item types allowed in Stage 2")]
        [SerializeField] private int maxDistinctItemsStage2 = 3;

        // ========================================================================
        // OBSERVER PATTERN EVENTS
        // ========================================================================

        /// <summary>
        /// Fired when a dynamic order is successfully generated.
        /// Passes (OrderData generatedOrder, float allowedDeliveryTime).
        /// </summary>
        public static event Action<OrderData, float> OnOrderGenerated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Generates a dynamic order procedurally based on current GameStage, Wave, and Difficulty limits.
        /// </summary>
        public OrderData GenerateRandomOrder()
        {
            bool isZombieStage = Core.GameManager.Instance != null &&
                                 Core.GameManager.Instance.CurrentStage == Core.GameStage.Stage2_Zombie;

            int currentWave = DifficultyScalingSystem.Instance != null
                ? DifficultyScalingSystem.Instance.CurrentWave
                : 1;

            // 1. Filter available ItemSOs based on current stage environment
            ItemStageType targetType = isZombieStage ? ItemStageType.Zombie : ItemStageType.Human;
            List<ItemSO> validSOs = availableItemSOs.FindAll(x => x != null && x.stageType == targetType);

            if (validSOs.Count == 0)
            {
                Debug.LogWarning($"[OrderGenerator] No ItemSO found for stage environment: {targetType}");
                return null;
            }

            // 2. Calculate distinct item count and maximum quantity based on difficulty scaling
            int maxDistinct = isZombieStage ? maxDistinctItemsStage2 : maxDistinctItemsStage1;
            int distinctCount = Mathf.Clamp(1 + (currentWave / 2), 1, Mathf.Min(maxDistinct, validSOs.Count));
            int maxQuantity = Mathf.Clamp(1 + (currentWave / 3), 1, 3);

            // 3. Create dynamic OrderData instance in memory
            OrderData newOrder = ScriptableObject.CreateInstance<OrderData>();
            newOrder.orderId = Guid.NewGuid().ToString();
            newOrder.orderTitle = isZombieStage ? $"Zombie Order (Wave {currentWave})" : $"Order (Wave {currentWave})";

            // Shuffle pool to guarantee random item combinations
            List<ItemSO> shuffledList = new List<ItemSO>(validSOs);
            Shuffle(shuffledList);

            int totalReward = 0;

            for (int i = 0; i < distinctCount; i++)
            {
                ItemSO selectedSO = shuffledList[i];
                int qty = UnityEngine.Random.Range(1, maxQuantity + 1);

                newOrder.items.Add(new OrderItem
                {
                    itemData = selectedSO,
                    quantity = qty
                });

                totalReward += selectedSO.basePrice * qty;
            }

            newOrder.rewardAmount = totalReward;

            // 4. Retrieve allowed delivery time from difficulty system
            float allowedTime = DifficultyScalingSystem.Instance != null
                ? DifficultyScalingSystem.Instance.CurrentAllowedDeliveryTime
                : 10f;

            Debug.Log($"[OrderGenerator] Dynamic Order Generated: {newOrder.orderTitle} | Items: {newOrder.items.Count} | Reward: {totalReward} | Time: {allowedTime}s");

            // Notify observer systems
            OnOrderGenerated?.Invoke(newOrder, allowedTime);

            return newOrder;
        }

        /// <summary>
        /// Fisher-Yates shuffle implementation to randomize item selection.
        /// </summary>
        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int rnd = UnityEngine.Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[rnd];
                list[rnd] = temp;
            }
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