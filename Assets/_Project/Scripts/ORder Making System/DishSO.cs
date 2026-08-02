using UnityEngine;

namespace ZombieDiner.Orders
{
    [CreateAssetMenu(fileName = "NewDish", menuName = "ZombieDiner/Dish")]
    public class DishSO : ScriptableObject
    {
        public string dishName;
        public Sprite sprite;
    }
}