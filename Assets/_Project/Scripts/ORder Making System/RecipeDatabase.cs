using System.Collections.Generic;
using UnityEngine;
using ZombieDiner.Orders;
[CreateAssetMenu(menuName = "ZombieDiner/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{ 
    public List<RecipeSO> recipes;
    public bool TryGetRecipe(List<ItemSO> i, out DishSO d)
    {
        foreach (var r in recipes)
        {
            if (r.Matches(i))
            {
                d = r.resultDish;
                return true;
            }
        }
        d = null;
        return false;
    }
}