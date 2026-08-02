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

        [Header("Available Dish Pool")]
        [Tooltip("List of all DishSO assets available for customers to order")]
        [SerializeField] private List<DishSO> availableDishSOs = new List<DishSO>();

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
                EnsureItemPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void EnsureItemPool()
        {
            availableItemSOs.RemoveAll(x => x == null);
            if (availableItemSOs.Count == 0)
            {
                var loaded = Resources.FindObjectsOfTypeAll<ItemSO>();
                if (loaded != null && loaded.Length > 0)
                {
                    availableItemSOs.AddRange(loaded);
                }
            }

            availableDishSOs.RemoveAll(x => x == null);
            if (availableDishSOs.Count == 0)
            {
                var loadedDishes = Resources.FindObjectsOfTypeAll<DishSO>();
                if (loadedDishes != null && loadedDishes.Length > 0)
                {
                    availableDishSOs.AddRange(loadedDishes);
                }
            }
        }

        /// <summary>
        /// Generates a dynamic order scaled by current wave progression and difficulty settings.
        /// Wave 1: 1 single item.
        /// Wave 2: 2 distinct items (quantity 1 each).
        /// Wave 3+: Multiple distinct items with scaling quantities.
        /// </summary>
        public OrderData GenerateRandomOrder()
        {
            EnsureItemPool();

            bool isZombieStage = Core.GameManager.Instance != null &&
                                 Core.GameManager.Instance.CurrentStage == Core.GameStage.Stage2_Zombie;

            int currentWave = DifficultyScalingSystem.Instance != null
                ? DifficultyScalingSystem.Instance.CurrentWave
                : 1;

            OrderData newOrder = ScriptableObject.CreateInstance<OrderData>();
            newOrder.orderId = Guid.NewGuid().ToString();
            newOrder.orderTitle = $"Order (Wave {currentWave})";

            ItemStageType targetType = isZombieStage ? ItemStageType.Zombie : ItemStageType.Human;
            List<ItemSO> validSOs = availableItemSOs.FindAll(x => x != null && x.stageType == targetType);
            if (validSOs.Count == 0) validSOs = new List<ItemSO>(availableItemSOs);

            if (validSOs.Count == 0)
            {
                Debug.LogWarning("[OrderGenerator] No ItemSO found in available pool.");
                return null;
            }

            // 🎯 Wave-based difficulty scaling logic
            int distinctTypesCount;
            int maxQuantityPerItem;

            if (currentWave == 1)
            {
                // Wave 1: Only 1 item, quantity 1
                distinctTypesCount = 1;
                maxQuantityPerItem = 1;
            }
            else if (currentWave == 2)
            {
                // Wave 2: 2 distinct item types, quantity 1 each
                distinctTypesCount = Mathf.Min(2, validSOs.Count);
                maxQuantityPerItem = 1;
            }
            else if (currentWave == 3)
            {
                // Wave 3: Up to 2 distinct items, quantity up to 2
                distinctTypesCount = Mathf.Min(2, validSOs.Count);
                maxQuantityPerItem = 2;
            }
            else
            {
                // Wave 4+: 2 to 3 distinct items, quantity up to 3
                int maxAllowedTypes = isZombieStage ? maxDistinctItemsStage2 : maxDistinctItemsStage1;
                distinctTypesCount = Mathf.Min(UnityEngine.Random.Range(2, maxAllowedTypes + 1), validSOs.Count);
                maxQuantityPerItem = UnityEngine.Random.Range(2, 4);
            }

            // Shuffle valid items to get distinct random choices
            List<ItemSO> poolCopy = new List<ItemSO>(validSOs);
            Shuffle(poolCopy);

            int totalReward = 0;

            for (int i = 0; i < distinctTypesCount; i++)
            {
                ItemSO selectedSO = poolCopy[i];
                int quantity = UnityEngine.Random.Range(1, maxQuantityPerItem + 1);

                newOrder.items.Add(new OrderItem
                {
                    itemData = selectedSO,
                    quantity = quantity
                });

                totalReward += (selectedSO != null ? selectedSO.basePrice : 10) * quantity;
            }

            newOrder.rewardAmount = totalReward;

            float allowedTime = DifficultyScalingSystem.Instance != null
                ? DifficultyScalingSystem.Instance.CurrentAllowedDeliveryTime
                : 15f;

            Debug.Log($"[OrderGenerator] Order Generated for Wave {currentWave} ({distinctTypesCount} types) | Reward: {totalReward} | Time: {allowedTime}s");

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