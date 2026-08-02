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
                EnsureItemPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [Header("Available Dish Pool")]
        [Tooltip("List of all DishSO assets available for customers to order")]
        [SerializeField] private List<DishSO> availableDishSOs = new List<DishSO>();

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
        /// Generates a dynamic order requesting between 2 and 4 items from the available item pool.
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

            // Customer requests 2 to 4 items
            int itemCount = UnityEngine.Random.Range(2, 5);
            int totalReward = 0;

            for (int i = 0; i < itemCount; i++)
            {
                ItemSO selectedSO = validSOs[UnityEngine.Random.Range(0, validSOs.Count)];
                newOrder.items.Add(new OrderItem
                {
                    itemData = selectedSO,
                    quantity = 1
                });

                totalReward += (selectedSO != null ? selectedSO.basePrice : 10);
            }

            newOrder.rewardAmount = totalReward;

            float allowedTime = DifficultyScalingSystem.Instance != null
                ? DifficultyScalingSystem.Instance.CurrentAllowedDeliveryTime
                : 15f;

            Debug.Log($"[OrderGenerator] Multi-Item Order Generated ({itemCount} items) | Reward: {totalReward} | Time: {allowedTime}s");

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