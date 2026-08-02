using UnityEngine;

namespace ZombieDiner.Orders
{
    public enum ItemStageType
    {
        Human,
        Zombie
    }

    public enum ContainerType
    {
        Plate,
        Cup,
        Basket
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

        [Header("Paired Stage Counterpart (Optional)")]
        [Tooltip("ضع هنا المكون المقابل في الطور الآخر (مثلاً: البورجر البشري يوضع فيه البورجر الزومبي)")]
        public ItemSO counterpartItem;

        // 🟢 خاصية موحدة تجنبك أي خطأ في حالة الأحرف (Capital / Small)
        public string ItemID => string.IsNullOrEmpty(itemId) ? name : itemId;
    }
}