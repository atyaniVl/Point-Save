using UnityEngine;

namespace ZombieDiner.Orders
{
    public enum ItemStageType
    {
        Human,  // Standard item for Stage 1 (Human Diner)
        Zombie  // Horrific item for Stage 2 (Zombie Diner)
    }

    [CreateAssetMenu(fileName = "NewItemSO", menuName = "Zombie Diner/Item SO")]
    public class ItemSO : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique identifier for the item (e.g., Burger, Cola)")]
        public string itemId;

        [Tooltip("Readable display name for UI rendering")]
        public string itemName;

        [Header("Visuals")]
        [Tooltip("Art sprite representing the item in orders and UI")]
        public Sprite itemIcon;

        [Header("Classification & Economy")]
        [Tooltip("Determines whether this item belongs to Human or Zombie environment")]
        public ItemStageType stageType;

        [Tooltip("Base price/reward per single unit")]
        public int basePrice = 10;
    }
}