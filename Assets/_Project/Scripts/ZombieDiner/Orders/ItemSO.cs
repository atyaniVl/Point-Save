using UnityEngine;

namespace ZombieDiner.Orders
{
    public enum ItemStageType
    {
        Human,  // Standard item for Stage 1 (Human Diner)
        Zombie  // Horrific item for Stage 2 (Zombie Diner)
    }

    public enum ContainerType
    {
        Plate,   // Food/Burgers
        Cup,     // Drinks/Juice/Cola
        Basket   // Snacks/Fries
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

        [Header("Container Tool Type")]
        [Tooltip("Type of container/tool used to fill and serve this item")]
        public ContainerType containerType = ContainerType.Plate;

        [Header("Classification & Economy")]
        [Tooltip("Determines whether this item belongs to Human or Zombie environment")]
        public ItemStageType stageType;

        [Tooltip("Base price/reward per single unit")]
        public int basePrice = 10;
        public string itemID => name;
    }
}