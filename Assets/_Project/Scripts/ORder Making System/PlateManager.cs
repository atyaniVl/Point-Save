using System.Collections.Generic;
using UnityEngine;
using ZombieDiner.Orders;

public class PlateManager : MonoBehaviour
{
    public static PlateManager Instance { get; private set; }

    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private PlateVisual plateVisual;

    private readonly List<ItemSO> ingredients = new List<ItemSO>();

    [SerializeField] private int maxCapacity = 6;

    public RecipeDatabase Database => recipeDatabase;
    public IReadOnlyList<ItemSO> Ingredients => ingredients;

    public DishSO CurrentDish { get; private set; }
    public bool IsCompleted => ingredients.Count > 0;

    private void Awake()
    {
        Instance = this;
    }

    public void TryAddIngredient(ItemSO item)
    {
        if (item == null) return;

        // 1. Try to find a container tool matching item.containerType that is empty
        PlateVisual targetContainer = null;
        PlateVisual[] containers = FindObjectsOfType<PlateVisual>();

        foreach (var c in containers)
        {
            if (c != null && c.ContainerType == item.containerType && !c.HasItem)
            {
                targetContainer = c;
                break;
            }
        }

        // Fallback A: Any empty container tool
        if (targetContainer == null)
        {
            foreach (var c in containers)
            {
                if (c != null && !c.HasItem)
                {
                    targetContainer = c;
                    break;
                }
            }
        }

        // Fallback B: Inspector assigned plateVisual
        if (targetContainer == null)
        {
            targetContainer = plateVisual;
        }

        if (targetContainer != null)
        {
            bool filled = targetContainer.TryFillContainer(item);
            if (filled)
            {
                ingredients.Add(item);
                OrderMakingEvents.RaiseIngredientAdded(item);
            }
        }
    }

    public void ClearPlate()
    {
        ingredients.Clear();
        CurrentDish = null;

        if (plateVisual != null)
        {
            plateVisual.Clear();
        }

        OrderMakingEvents.RaisePlateCleared();
    }

    private void CompletePlate(DishSO dish)
    {
        CurrentDish = dish;

        if (plateVisual != null)
        {
            plateVisual.ShowDish(dish);
        }

        OrderMakingEvents.RaiseDishCompleted(dish);
    }

    private DishSO CreateFallbackDish()
    {
        DishSO dish = ScriptableObject.CreateInstance<DishSO>();
        dish.dishName = ingredients[0] != null ? ingredients[0].itemName : "Test Plate";
        dish.sprite = ingredients[0] != null ? ingredients[0].itemIcon : null;
        return dish;
    }
}
