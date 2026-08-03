using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZombieDiner.Orders
{
    /// <summary>
    /// Represents a single item entry inside a dynamic order along with its required quantity.
    /// </summary>
    [Serializable]
    public class OrderItem
    {
        [Tooltip("Direct reference to the Item ScriptableObject asset")]
        public ItemSO itemData;

        [Tooltip("Required quantity of this specific item")]
        public int quantity = 1;
    }

    /// <summary>
    /// Data container created dynamically in memory to hold generated order details.
    /// </summary>
    public class OrderData : ScriptableObject
    {
        [Header("Basic Order Info")]
        public string orderId;
        public string orderTitle;

        [Header("Dish Order")]
        public DishSO requestedDish;

        [Header("Order Contents")]
        public List<OrderItem> items = new List<OrderItem>();

        [Header("Currency / Reward")]
        public int rewardAmount = 10;
        public int totalReward => rewardAmount;

        /// <summary>
        /// Calculates and returns the total sum of all individual items required in this order.
        /// </summary>
        public int GetTotalItemCount()
        {
            int total = 0;
            foreach (var item in items)
            {
                if (item != null)
                {
                    total += item.quantity;
                }
            }
            return total;
        }
    }
}