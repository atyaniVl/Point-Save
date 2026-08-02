using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZombieDiner.Orders;
using DG.Tweening;

namespace ZombieDiner.UI
{
    public class CustomerOrderBubbleUI : MonoBehaviour
    {
        [Header("Bubble Components")]
        [SerializeField] private GameObject bubblePanel;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject itemSlotPrefab;

        [Header("Patience Bar Components")]
        [SerializeField] private Image patienceFillImage;
        [SerializeField] private GameObject exclamationMarkIcon;

        [Header("Juice & Tween Settings")]
        [SerializeField] private float popDuration = 0.35f;
        [SerializeField] private float itemStaggerDelay = 0.08f;

        private Dictionary<string, GameObject> activeSlotMap = new Dictionary<string, GameObject>();
        private Sequence warningShakeSequence;
        private bool isWarningActive = false;
        private Vector3 originalExclamationScale = Vector3.one;

        private void Awake()
        {
            if (exclamationMarkIcon != null)
            {
                originalExclamationScale = exclamationMarkIcon.transform.localScale;
            }
            HideBubbleImmediate();
        }

        public void DisplayOrder(OrderData orderData)
        {
            if (orderData == null) return;
            StopWarningShake();
            activeSlotMap.Clear();

            if (bubblePanel != null)
            {
                bubblePanel.SetActive(true);
                bubblePanel.transform.DOKill();
                bubblePanel.transform.localScale = Vector3.zero;
                bubblePanel.transform.DOScale(Vector3.one, popDuration).SetEase(Ease.OutBack);
            }

            // تنظيف الحاوية من العناصر القديمة
            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            // 1️⃣ حالة الـ Requested Dish (إذا كان الطلب طبقاً كاملاً جاهزاً)
            if (orderData.requestedDish != null && itemSlotPrefab != null && itemsContainer != null)
            {
                GameObject slotGO = Instantiate(itemSlotPrefab, itemsContainer);
                slotGO.transform.localScale = Vector3.zero;

                Image itemImage = GetItemImageComponent(slotGO);
                TextMeshProUGUI quantityText = GetQuantityTextComponent(slotGO);

                if (itemImage != null)
                {
                    if (orderData.requestedDish.sprite != null)
                    {
                        itemImage.sprite = orderData.requestedDish.sprite;
                        itemImage.enabled = true;
                        itemImage.color = Color.white; // ضمان عدم شفافية اللون
                    }
                    else
                    {
                        itemImage.enabled = false;
                        Debug.LogWarning($"[CustomerOrderBubbleUI] Missing Sprite for Dish: {orderData.requestedDish.dishName}");
                    }
                }

                if (quantityText != null)
                {
                    quantityText.text = orderData.requestedDish.dishName;
                }

                activeSlotMap[orderData.requestedDish.dishName] = slotGO;
                slotGO.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
                return;
            }

            // 2️⃣ دمج العناصر المتكررة داخل القائمة وتجميع أعدادها
            Dictionary<ItemSO, int> aggregatedItems = new Dictionary<ItemSO, int>();
            if (orderData.items != null)
            {
                foreach (var item in orderData.items)
                {
                    if (item == null || item.itemData == null) continue;

                    ItemSO itemData = item.itemData;
                    int quantity = Mathf.Max(1, item.quantity);

                    if (aggregatedItems.ContainsKey(itemData))
                    {
                        aggregatedItems[itemData] += quantity;
                    }
                    else
                    {
                        aggregatedItems.Add(itemData, quantity);
                    }
                }
            }

            // 3️⃣ إنشاء الـ Slots الفريدة فقط بالعنصر وإجمالي عدده
            int itemIndex = 0;
            foreach (var kvp in aggregatedItems)
            {
                ItemSO itemData = kvp.Key;
                int totalQuantity = kvp.Value;

                GameObject slotGO = Instantiate(itemSlotPrefab, itemsContainer);
                slotGO.transform.localScale = Vector3.zero;

                Image itemImage = GetItemImageComponent(slotGO);
                TextMeshProUGUI quantityText = GetQuantityTextComponent(slotGO);

                // تعيين الصورة والتأكد من تفعيل المكون وتلوينه بالأبيض النقي
                if (itemImage != null)
                {
                    if (itemData.itemIcon != null)
                    {
                        itemImage.sprite = itemData.itemIcon;
                        itemImage.enabled = true;
                        itemImage.color = Color.white; // حماية ضد الألوان المعتمة أو الشفافة
                    }
                    else
                    {
                        itemImage.enabled = false; // إخفاء المكون لتجنب المربع الأبيض الفارغ
                        Debug.LogWarning($"[CustomerOrderBubbleUI] Missing ItemIcon in ScriptableObject for Item: {itemData.name}");
                    }
                }

                if (quantityText != null)
                {
                    quantityText.text = $"x{totalQuantity}";
                }

                // قراءة الـ ID المعياري الشامل
                string key = GetItemKey(itemData);
                activeSlotMap[key] = slotGO;

                float delay = itemIndex * itemStaggerDelay;
                slotGO.transform.DOScale(Vector3.one, 0.25f).SetDelay(delay).SetEase(Ease.OutBack);

                itemIndex++;
            }
        }

        public void UpdateFulfilledProgress(Dictionary<string, int> fulfilledCounts, OrderData orderData)
        {
            if (orderData == null || fulfilledCounts == null) return;

            // حساب الإجمالي المطلوب لكل عنصر بعد الدمج
            Dictionary<string, int> totalRequiredMap = new Dictionary<string, int>();
            foreach (var item in orderData.items)
            {
                if (item == null || item.itemData == null) continue;
                string key = GetItemKey(item.itemData);
                int q = Mathf.Max(1, item.quantity);

                if (totalRequiredMap.ContainsKey(key))
                    totalRequiredMap[key] += q;
                else
                    totalRequiredMap.Add(key, q);
            }

            // تحديث العرض بناءً على المسلم إزاء الإجمالي المطلوب
            foreach (var kvp in totalRequiredMap)
            {
                string key = kvp.Key;
                int totalRequired = kvp.Value;
                int fulfilled = fulfilledCounts.ContainsKey(key) ? fulfilledCounts[key] : 0;

                if (activeSlotMap.TryGetValue(key, out GameObject slotGO))
                {
                    TextMeshProUGUI quantityText = GetQuantityTextComponent(slotGO);

                    int remaining = totalRequired - fulfilled;
                    if (remaining <= 0)
                    {
                        if (quantityText != null) quantityText.gameObject.SetActive(false);

                        Transform checkmarkTransform = slotGO.transform.Find("Checkmark");
                        if (checkmarkTransform != null)
                        {
                            checkmarkTransform.gameObject.SetActive(true);
                            checkmarkTransform.localScale = Vector3.zero;
                            checkmarkTransform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
                        }
                    }
                    else
                    {
                        if (quantityText != null)
                        {
                            quantityText.gameObject.SetActive(true);
                            quantityText.text = $"x{remaining}";
                        }
                        slotGO.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
                    }
                }
            }
        }

        public void MarkItemAsDelivered(string itemID, Transform startWorldPos)
        {
            if (activeSlotMap.TryGetValue(itemID, out GameObject slotGO))
            {
                AnimateFoodFloating(startWorldPos, slotGO.transform);

                TextMeshProUGUI quantityText = GetQuantityTextComponent(slotGO);
                if (quantityText != null)
                {
                    quantityText.gameObject.SetActive(false);
                }

                Transform checkmarkTransform = slotGO.transform.Find("Checkmark");
                if (checkmarkTransform != null)
                {
                    checkmarkTransform.gameObject.SetActive(true);
                    checkmarkTransform.localScale = Vector3.zero;
                    checkmarkTransform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
                }
            }
        }

        private void AnimateFoodFloating(Transform fromTransform, Transform targetTransform)
        {
            if (fromTransform == null || targetTransform == null) return;

            GameObject floatingObj = new GameObject("FloatingFoodItem");
            floatingObj.transform.position = fromTransform.position;

            floatingObj.transform.DOJump(targetTransform.position, jumpPower: 1.5f, numJumps: 1, duration: 0.45f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    if (targetTransform != null)
                    {
                        targetTransform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
                    }
                    Destroy(floatingObj);
                });
        }

        public void UpdatePatienceBar(float normalizedTime)
        {
            float clampedTime = Mathf.Clamp01(normalizedTime);

            if (patienceFillImage != null)
            {
                patienceFillImage.fillAmount = clampedTime;
                patienceFillImage.color = Color.Lerp(Color.red, Color.green, clampedTime);
            }
        }

        public void StartWarningShake()
        {
            if (isWarningActive) return;
            isWarningActive = true;

            if (exclamationMarkIcon != null)
            {
                exclamationMarkIcon.SetActive(true);
                exclamationMarkIcon.transform.DOKill();
                exclamationMarkIcon.transform.localScale = Vector3.zero;
                exclamationMarkIcon.transform.DOScale(originalExclamationScale, 0.25f).SetEase(Ease.OutBack);
            }

            if (bubblePanel != null)
            {
                bubblePanel.transform.DOKill();

                warningShakeSequence = DOTween.Sequence();
                warningShakeSequence.Join(bubblePanel.transform.DOShakePosition(0.5f, strength: new Vector3(4f, 4f, 0f), vibrato: 18, randomness: 90, fadeOut: false))
                   .Join(bubblePanel.transform.DOShakeRotation(0.5f, strength: new Vector3(0, 0, 3f), vibrato: 18, randomness: 90, fadeOut: false))
                   .SetLoops(-1, LoopType.Restart);
            }
        }

        public void StopWarningShake()
        {
            isWarningActive = false;

            if (warningShakeSequence != null && warningShakeSequence.IsActive())
            {
                warningShakeSequence.Kill();
            }

            if (bubblePanel != null)
            {
                bubblePanel.transform.DOKill();
                bubblePanel.transform.localPosition = Vector3.zero;
                bubblePanel.transform.localRotation = Quaternion.identity;
            }

            if (exclamationMarkIcon != null)
            {
                exclamationMarkIcon.transform.DOKill();
                exclamationMarkIcon.SetActive(false);
            }
        }

        public void HideBubble()
        {
            StopWarningShake();
            if (bubblePanel != null && bubblePanel.activeSelf)
            {
                bubblePanel.transform.DOKill();
                bubblePanel.transform.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => bubblePanel.SetActive(false));
            }
        }

        public void HideBubbleImmediate()
        {
            StopWarningShake();
            if (bubblePanel != null)
            {
                bubblePanel.transform.DOKill();
                bubblePanel.transform.localScale = Vector3.zero;
                bubblePanel.SetActive(false);
            }
        }

        #region Helpers

        /// <summary>
        /// جلب مكون الـ Image المخصص للأيقونة بدقة حتى وإن كان الـ Slot يمتلك Image رئيسي كخلفية
        /// </summary>
        private Image GetItemImageComponent(GameObject slotGO)
        {
            Transform iconTransform = slotGO.transform.Find("ItemIcon");
            if (iconTransform != null)
            {
                return iconTransform.GetComponent<Image>();
            }

            // في حال عدم وجود ItemIcon كـ Child أعد جلب مكون الابن المتاح
            Image[] images = slotGO.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject != slotGO) return img; // إرجاع الصورة الابن وليس خلفية الأب
            }

            return slotGO.GetComponent<Image>();
        }

        /// <summary>
        /// جلب مكون النص بشكل مضمون
        /// </summary>
        private TextMeshProUGUI GetQuantityTextComponent(GameObject slotGO)
        {
            Transform textTransform = slotGO.transform.Find("QuantityText");
            if (textTransform != null)
            {
                return textTransform.GetComponent<TextMeshProUGUI>();
            }
            return slotGO.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        /// <summary>
        /// دالة مساعدة لاستخراج المفتاح التعريفي الصحيح للعنصر
        /// </summary>
        private string GetItemKey(ItemSO itemSO)
        {
            if (itemSO == null) return string.Empty;
            return !string.IsNullOrEmpty(itemSO.itemId) ? itemSO.itemId : itemSO.name;
        }

        #endregion

        private void OnDestroy()
        {
            StopWarningShake();
            if (bubblePanel != null) bubblePanel.transform.DOKill();
            if (exclamationMarkIcon != null) exclamationMarkIcon.transform.DOKill();
        }
    }
}