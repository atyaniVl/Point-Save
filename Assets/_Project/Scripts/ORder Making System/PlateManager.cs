using System.Collections.Generic;
using UnityEngine;
using ZombieDiner.Orders;
using ZombieDiner.Visuals;

namespace ZombieDiner.Orders
{
    public class PlateManager : MonoBehaviour
    {
        public static PlateManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private RecipeDatabase recipeDatabase;
        [SerializeField] private PlateVisual plateVisual;

        [Header("Plate Capacity")]
        [SerializeField] private int maxCapacity = 6;

        private readonly List<ItemSO> ingredients = new List<ItemSO>();

        public RecipeDatabase Database => recipeDatabase;
        public IReadOnlyList<ItemSO> Ingredients => ingredients;
        public DishSO CurrentDish { get; private set; }
        public bool IsCompleted => ingredients.Count > 0;

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

            // إيجاد الصحن البصري مع استخدام الدالة الحديثة في يونيتي
            if (plateVisual == null)
            {
                plateVisual = FindFirstObjectByType<PlateVisual>();
            }
        }

        /// <summary>
        /// 🍔 إضافة مكون جديد للصحن المتاح
        /// </summary>
        public void TryAddIngredient(ItemSO item)
        {
            if (item == null) return;

            // التحقق من أن الصحن لم يصل للحد الأقصى للمكونات
            if (ingredients.Count >= maxCapacity)
            {
                Debug.LogWarning("[PlateManager] Plate capacity limit reached!");
                return;
            }

            if (plateVisual == null)
            {
                plateVisual = FindFirstObjectByType<PlateVisual>();
            }

            if (plateVisual != null)
            {
                // إضافة المكون للصحن بصرياً
                plateVisual.OnIngredientAdded(item);

                // تسجيل المكون في بيانات الصحن
                ingredients.Add(item);

                // إرسال حدث إضافة المكون للأنظمة
                OrderMakingEvents.RaiseIngredientAdded(item);
            }
            else
            {
                Debug.LogError("[PlateManager] No PlateVisual assigned or found in scene!");
            }
        }

        /// <summary>
        /// 🧹 تفريغ بيانات الصحن واستعداده للرسبون الجديد
        /// </summary>
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

        /// <summary>
        /// 🍽️ إكمال طبق معين بـ Recipe خاصة
        /// </summary>
        public void CompletePlate(DishSO dish)
        {
            CurrentDish = dish;

            if (plateVisual != null)
            {
                plateVisual.ShowDish(dish);
            }

            OrderMakingEvents.RaiseDishCompleted(dish);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}