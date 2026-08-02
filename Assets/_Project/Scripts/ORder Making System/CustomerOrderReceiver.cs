using System;
using System.Collections.Generic;
using UnityEngine;
using ZombieDiner.Orders;

namespace ZombieDiner.Customers
{
    public class CustomerOrderReceiver : MonoBehaviour
    {
        [SerializeField] private DishSO expectedDish;

        private CustomerController customerController;

        private void Awake()
        {
            customerController = GetComponent<CustomerController>();
        }

        /// <summary>
        /// يُستدعى عند تسليم طبق جاهز للزبون مباشرة.
        /// </summary>
        public void ReceiveDish(DishSO dish)
        {
            IReadOnlyList<ItemSO> plateItems = PlateManager.Instance != null ? PlateManager.Instance.Ingredients : null;
            ReceivePlateItems(plateItems, dish);
        }

        /// <summary>
        /// يُستدعى عند تسليم عنصر منفرد (ItemSO) للزبون.
        /// </summary>
        public void ReceiveSingleItem(ItemSO item)
        {
            if (item == null || customerController == null || customerController.CurrentOrder == null) return;

            bool isMatched = customerController.TryFulfillItem(item);
            if (isMatched)
            {
                OrderMakingEvents.RaiseOrderDeliveredCorrect();
                if (customerController.IsOrderFullyFulfilled())
                {
                    customerController.CompleteOrderSuccessfully();
                }
            }
            else
            {
                // ❌ الطلب خاطئ: يتم إطلاق صوت الطلب الخاطئ وتجاهل التسليم بدون استدعاء LeaveUnsatisfied
                OrderMakingEvents.RaiseOrderDeliveredWrong();

                // 🛑 تم إزالة customerController.LeaveUnsatisfied() لمنع مغادرة الزبون ونقص الأرواح
            }
        }

        /// <summary>
        /// يُستدعى للتحقق وتمرير مكونات الصحن/الطبق إلى الزبون.
        /// </summary>
        public void ReceivePlateItems(IReadOnlyList<ItemSO> plateItems, DishSO optionalDish = null)
        {
            if (customerController == null || customerController.CurrentOrder == null) return;

            // 1. التحقق أولاً هل ما تم تسليمه يطابق طلب الزبون
            bool isValidDelivery = IsOrderMatching(plateItems, optionalDish);

            if (!isValidDelivery)
            {
                // ❌ إذا كان الطلب خاطئاً:
                OrderMakingEvents.RaiseOrderDeliveredWrong();

                // 🛑 تم إزالة customerController.LeaveUnsatisfied() لمنع الزبون من الغضب ونقص الأرواح
                return;
            }

            // 2. إذا كان الطلب صحيحاً، يتم احتساب المكونات والإنهاء
            if (plateItems != null && plateItems.Count > 0)
            {
                foreach (var item in plateItems)
                {
                    if (item == null) continue;
                    customerController.TryFulfillItem(item);
                }
            }

            OrderMakingEvents.RaiseOrderDeliveredCorrect();

            if (customerController.IsOrderFullyFulfilled())
            {
                customerController.CompleteOrderSuccessfully();
            }
        }

        /// <summary>
        /// فحص داخلي للتحقق من تطابق المكونات/الطبق المقدم مع طلب الزبون الحالي.
        /// </summary>
        private bool IsOrderMatching(IReadOnlyList<ItemSO> plateItems, DishSO optionalDish)
        {
            // Check 1: Inspector override validation
            if (expectedDish != null)
            {
                return optionalDish == expectedDish;
            }

            if (customerController == null || customerController.CurrentOrder == null) return false;

            var order = customerController.CurrentOrder;

            // Check 2: Direct dish comparison if the order specifies a particular DishSO
            if (order.requestedDish != null && optionalDish != null)
            {
                if (order.requestedDish == optionalDish ||
                    string.Equals(order.requestedDish.dishName, optionalDish.dishName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            // Check 3: Multi-item matching validation (compare plate ingredients vs unfulfilled order items)
            if (plateItems != null && plateItems.Count > 0)
            {
                List<ItemSO> orderedItems = new List<ItemSO>();
                foreach (var orderItem in order.items)
                {
                    if (orderItem?.itemData != null)
                    {
                        for (int i = 0; i < Mathf.Max(1, orderItem.quantity); i++)
                        {
                            orderedItems.Add(orderItem.itemData);
                        }
                    }
                }

                if (MatchesItems(plateItems, orderedItems))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// مقارنة عدد العناصر وأنواعها
        /// </summary>
        private bool MatchesItems(IReadOnlyList<ItemSO> plateItems, List<ItemSO> orderItems)
        {
            if (plateItems == null || orderItems == null) return false;
            if (plateItems.Count != orderItems.Count) return false;

            var dict = new Dictionary<string, int>();
            foreach (var item in orderItems)
            {
                if (item == null) continue;
                string key = string.IsNullOrEmpty(item.itemId) ? item.name : item.itemId;
                dict[key] = dict.ContainsKey(key) ? dict[key] + 1 : 1;
            }

            foreach (var item in plateItems)
            {
                if (item == null) return false;
                string key = string.IsNullOrEmpty(item.itemId) ? item.name : item.itemId;

                if (!dict.ContainsKey(key) || dict[key] <= 0)
                {
                    return false;
                }

                dict[key]--;
            }

            return true;
        }
    }
}