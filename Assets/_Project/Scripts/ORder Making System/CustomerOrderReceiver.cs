using System.Collections.Generic;
using UnityEngine;
using ZombieDiner.Customers;
using ZombieDiner.Orders;

public class CustomerOrderReceiver : MonoBehaviour
{
    [SerializeField] private DishSO expectedDish;

    private CustomerController customerController;

    private void Awake()
    {
        customerController = GetComponent<CustomerController>();
    }

    public void ReceiveDish(DishSO dish)
    {
        IReadOnlyList<ItemSO> plateItems = PlateManager.Instance != null ? PlateManager.Instance.Ingredients : null;
        ReceivePlateItems(plateItems, dish);
    }

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
            OrderMakingEvents.RaiseOrderDeliveredWrong();
            customerController.LeaveUnsatisfied();
        }
    }

    public void ReceivePlateItems(IReadOnlyList<ItemSO> plateItems, DishSO optionalDish = null)
    {
        if (plateItems == null || plateItems.Count == 0) return;
        if (customerController == null || customerController.CurrentOrder == null) return;

        bool allMatched = true;
        foreach (var item in plateItems)
        {
            if (item == null) continue;
            bool matched = customerController.TryFulfillItem(item);
            if (!matched)
            {
                allMatched = false;
            }
        }

        if (allMatched)
        {
            OrderMakingEvents.RaiseOrderDeliveredCorrect();
            if (customerController.IsOrderFullyFulfilled())
            {
                customerController.CompleteOrderSuccessfully();
            }
        }
        else
        {
            OrderMakingEvents.RaiseOrderDeliveredWrong();
            customerController.LeaveUnsatisfied();
        }
    }

    private bool IsOrderMatching(IReadOnlyList<ItemSO> plateItems, DishSO optionalDish)
    {
        // 1. Explicit inspector override if set
        if (expectedDish != null)
        {
            return optionalDish == expectedDish;
        }

        // 2. Dynamic order validation via CustomerController
        if (customerController != null && customerController.CurrentOrder != null)
        {
            var order = customerController.CurrentOrder;

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

            // Check A: Direct item list match (plate items vs ordered items)
            if (plateItems != null && plateItems.Count > 0)
            {
                if (MatchesItems(plateItems, orderedItems))
                {
                    return true;
                }
            }

            // Check B: Direct dish match if requestedDish was set
            if (order.requestedDish != null && optionalDish != null)
            {
                if (order.requestedDish == optionalDish || string.Equals(order.requestedDish.dishName, optionalDish.dishName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        return plateItems != null && plateItems.Count > 0;
    }

    private bool MatchesItems(IReadOnlyList<ItemSO> plateItems, List<ItemSO> orderItems)
    {
        if (plateItems == null || orderItems == null) return false;
        if (plateItems.Count != orderItems.Count) return false;

        var dict = new Dictionary<string, int>();
        foreach (var item in orderItems)
        {
            if (item == null) continue;
            string key = string.IsNullOrEmpty(item.itemID) ? item.name : item.itemID;
            dict[key] = dict.ContainsKey(key) ? dict[key] + 1 : 1;
        }

        foreach (var item in plateItems)
        {
            if (item == null) return false;
            string key = string.IsNullOrEmpty(item.itemID) ? item.name : item.itemID;
            if (!dict.ContainsKey(key)) return false;
            dict[key]--;
            if (dict[key] < 0) return false;
        }

        return true;
    }
}
