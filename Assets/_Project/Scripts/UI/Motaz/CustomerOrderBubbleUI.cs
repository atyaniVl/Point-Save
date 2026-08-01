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

            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            int itemIndex = 0;
            foreach (var item in orderData.items)
            {
                if (item == null || item.itemData == null) continue;

                GameObject slotGO = Instantiate(itemSlotPrefab, itemsContainer);
                slotGO.transform.localScale = Vector3.zero;

                Image itemImage = slotGO.transform.Find("ItemIcon")?.GetComponent<Image>();
                TextMeshProUGUI quantityText = slotGO.transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();

                if (itemImage == null) itemImage = slotGO.GetComponentInChildren<Image>();
                if (quantityText == null) quantityText = slotGO.GetComponentInChildren<TextMeshProUGUI>();

                if (itemImage != null) itemImage.sprite = item.itemData.itemIcon;
                if (quantityText != null) quantityText.text = $"x{item.quantity}";

                // 🔹 حفظ مرجع الـ Slot ربطاً بالـ ItemID لتسليمه جزئياً
                activeSlotMap[item.itemData.itemID] = slotGO;

                float delay = itemIndex * itemStaggerDelay;
                slotGO.transform.DOScale(Vector3.one, 0.25f).SetDelay(delay).SetEase(Ease.OutBack);

                itemIndex++;
            }
        }

        /// <summary>
        /// 🔹 وضع علامة صح فوق نص العدد (QuantityText) عند تسليم العنصر جزئياً
        /// </summary>
        public void MarkItemAsDelivered(string itemID, Transform startWorldPos)
        {
            if (activeSlotMap.TryGetValue(itemID, out GameObject slotGO))
            {
                // 1. إطلاق أنيميشن طيران الأكل
                AnimateFoodFloating(startWorldPos, slotGO.transform);

                // 2. البحث عن نص العدد وإخفاؤه
                Transform quantityTextTransform = slotGO.transform.Find("QuantityText");
                if (quantityTextTransform != null)
                {
                    quantityTextTransform.gameObject.SetActive(false);
                }

                // 3. إظهار علامة الصح ✔️ مكان النص مع أنيميشن Pop ناعم
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
                    targetTransform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
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

        /// <summary>
        /// 🔹 بدء الاهتزاز والتحذير المرئي (يتم استدعاؤها بالتزامن مع الصوت)
        /// </summary>
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

                // أنيميشن اهتزاز متواصل ومناسب لوضع الخطر مع الصوت
                warningShakeSequence = DOTween.Sequence();
                warningShakeSequence.Join(bubblePanel.transform.DOShakePosition(0.5f, strength: new Vector3(4f, 4f, 0f), vibrato: 18, randomness: 90, fadeOut: false))
                   .Join(bubblePanel.transform.DOShakeRotation(0.5f, strength: new Vector3(0, 0, 3f), vibrato: 18, randomness: 90, fadeOut: false))
                                   .SetLoops(-1, LoopType.Restart);
            }
        }

        /// <summary>
        /// 🔹 إيقاف اهتزاز الفقاعة وإعادة تعيين موقعها
        /// </summary>
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

        private void OnDestroy()
        {
            StopWarningShake();
            if (bubblePanel != null) bubblePanel.transform.DOKill();
            if (exclamationMarkIcon != null) exclamationMarkIcon.transform.DOKill();
        }
    }
}