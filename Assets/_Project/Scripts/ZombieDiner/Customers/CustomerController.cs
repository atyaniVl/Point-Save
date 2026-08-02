using System.Collections.Generic;
using UnityEngine;
using ZombieDiner.Orders;
using ZombieDiner.UI;
using ZombieDiner.Core;
using ZombieDiner.Gameplay;
using DG.Tweening;
using AudioSystem;

namespace ZombieDiner.Customers
{
    public enum CustomerState
    {
        WalkingToCounter,
        WaitingForOrder,
        Leaving
    }

    public class CustomerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2.2f;

        [Header("Customer Visuals")]
        [SerializeField] private SpriteRenderer customerSpriteRenderer;
        [SerializeField] private List<Sprite> humanSprites = new List<Sprite>();
        [SerializeField] private List<Sprite> zombieSprites = new List<Sprite>();

        [Header("Stage 1 - Human Patience Settings")]
        [SerializeField] private float humanPatienceDuration = 18f;

        [Header("Stage 2 - Zombie Patience Settings")]
        [SerializeField] private float zombiePatienceDuration = 10f;
        [SerializeField] private float zombiePatienceSpeedMultiplier = 1.35f;

        [Header("Human Walk Settings")]
        [SerializeField] private float humanHopAmount = 0.12f;
        [SerializeField] private float humanHopDuration = 0.22f;
        [SerializeField] private Vector3 humanLandPunch = new Vector3(0.15f, -0.15f, 0f);

        [Header("Zombie Walk Settings")]
        [SerializeField] private float zombieSwayAngle = 8f;
        [SerializeField] private float zombieLimpDrop = 0.05f;
        [SerializeField] private float zombieStepDuration = 0.4f;
        [SerializeField] private Vector3 zombieLandPunch = new Vector3(-0.2f, 0.2f, 0f);

        [Header("Components")]
        [SerializeField] private CustomerOrderBubbleUI bubbleUI;

        private CustomerState currentState = CustomerState.WalkingToCounter;
        private Vector3 targetQueuePosition;
        private OrderData currentOrder;

        private float maxPatience;
        private float remainingPatience;

        // 🔹 متغيّر لمتابعة تشغيل صوت واهتزاز تحذير نفاذ الوقت مرة واحدة فقط
        private bool hasPlayedWarningSound = false;

        private Sequence walkSequence;
        private HashSet<string> deliveredItemIDs = new HashSet<string>();

        private DeliveryZone deliveryZone;
        private CustomerOrderReceiver orderReceiver;
        private BoxCollider2D clickCollider;

        public OrderData CurrentOrder => currentOrder;
        public CustomerState CurrentState => currentState;

        private void Awake()
        {
            if (customerSpriteRenderer == null) customerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (bubbleUI == null) bubbleUI = GetComponentInChildren<CustomerOrderBubbleUI>();
            if (bubbleUI != null) bubbleUI.HideBubbleImmediate();

            if (!TryGetComponent(out orderReceiver)) orderReceiver = gameObject.AddComponent<CustomerOrderReceiver>();
            if (!TryGetComponent(out deliveryZone)) deliveryZone = gameObject.AddComponent<DeliveryZone>();

            if (!TryGetComponent(out clickCollider))
            {
                clickCollider = gameObject.AddComponent<BoxCollider2D>();
                clickCollider.size = new Vector2(1.1f, 1.6f);
                clickCollider.offset = Vector2.zero;
            }
        }

        public void InitializeInQueue(Vector3 targetPos, OrderData order, float defaultPatienceTime)
        {
            targetQueuePosition = targetPos;
            currentOrder = order;

            if(TryGetComponent<DeliveryZone>(out var deliveryZone))
            {
                //deliveryZone.;
            }

            deliveredItemIDs.Clear();
            fulfilledCounts.Clear();
            hasPlayedWarningSound = false;

            bool isZombie = IsZombieStage();
            maxPatience = isZombie ? zombiePatienceDuration : humanPatienceDuration;
            remainingPatience = maxPatience;

            currentState = CustomerState.WalkingToCounter;

            SetupRandomVisual();
            StartWalkingJuice();

            float duration = Vector3.Distance(transform.position, targetQueuePosition) / moveSpeed;
            transform.DOMove(targetQueuePosition, Mathf.Max(0.5f, duration))
                .SetEase(Ease.Linear)
                .OnComplete(OnReachedDestination);
        }

        private readonly Dictionary<string, int> fulfilledCounts = new Dictionary<string, int>();

        public bool TryFulfillItem(ItemSO item)
        {
            if (currentOrder == null || currentOrder.items == null || item == null) return false;

            foreach (var orderItem in currentOrder.items)
            {
                if (orderItem?.itemData == null) continue;
                ItemSO reqSO = orderItem.itemData;

                bool isMatch = item == reqSO ||
                               string.Equals(item.name, reqSO.name, System.StringComparison.OrdinalIgnoreCase) ||
                               (!string.IsNullOrEmpty(item.itemId) && string.Equals(item.itemId, reqSO.itemId, System.StringComparison.OrdinalIgnoreCase)) ||
                               (!string.IsNullOrEmpty(item.itemName) && string.Equals(item.itemName, reqSO.itemName, System.StringComparison.OrdinalIgnoreCase));

                if (isMatch)
                {
                    string reqKey = reqSO.name;
                    int currentFulfilled = fulfilledCounts.ContainsKey(reqKey) ? fulfilledCounts[reqKey] : 0;
                    if (currentFulfilled < orderItem.quantity)
                    {
                        fulfilledCounts[reqKey] = currentFulfilled + 1;
                        if (bubbleUI != null)
                        {
                            bubbleUI.UpdateFulfilledProgress(fulfilledCounts, currentOrder);
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsOrderFullyFulfilled()
        {
            if (currentOrder == null || currentOrder.items == null || currentOrder.items.Count == 0) return true;

            foreach (var orderItem in currentOrder.items)
            {
                if (orderItem?.itemData == null) continue;
                ItemSO reqSO = orderItem.itemData;
                string reqKey = reqSO.name;
                int currentFulfilled = fulfilledCounts.ContainsKey(reqKey) ? fulfilledCounts[reqKey] : 0;

                if (currentFulfilled < orderItem.quantity)
                {
                    return false;
                }
            }

            return true;
        }

        public void MoveToQueuePosition(Vector3 newPos)
        {
            targetQueuePosition = newPos;
            float duration = Vector3.Distance(transform.position, newPos) / moveSpeed;
            transform.DOMove(newPos, Mathf.Max(0.3f, duration)).SetEase(Ease.OutQuad);
        }

        private void SetupRandomVisual()
        {
            if (customerSpriteRenderer == null) return;

            bool isZombie = IsZombieStage();
            if (isZombie && zombieSprites.Count > 0)
            {
                customerSpriteRenderer.sprite = zombieSprites[Random.Range(0, zombieSprites.Count)];
            }
            else if (!isZombie && humanSprites.Count > 0)
            {
                customerSpriteRenderer.sprite = humanSprites[Random.Range(0, humanSprites.Count)];
            }
        }

        private bool IsZombieStage()
        {
            return GameManager.Instance != null && GameManager.Instance.CurrentStage == GameStage.Stage2_Zombie;
        }

        private void Update()
        {
            UpdateSortingOrder();

            if (currentState == CustomerState.WaitingForOrder)
            {
                HandlePatience();
            }
        }

        private void UpdateSortingOrder()
        {
            if (customerSpriteRenderer != null)
            {
                customerSpriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
            }
        }

        private void OnReachedDestination()
        {
            StopWalkingJuice();

            if (currentState == CustomerState.WalkingToCounter)
            {
                currentState = CustomerState.WaitingForOrder;

                bool isZombie = IsZombieStage();
                Vector3 punchScale = isZombie ? zombieLandPunch : humanLandPunch;
                transform.DOPunchScale(punchScale, 0.35f, isZombie ? 8 : 6, 1f);

                if (bubbleUI != null && currentOrder != null)
                {
                    bubbleUI.DisplayOrder(currentOrder);

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySfxRandomPitch("OrderSpawn", 0.95f, 1.05f);
                    }
                }
            }
        }

        private void HandlePatience()
        {
            bool isZombie = IsZombieStage();

            float speedMultiplier = isZombie ? zombiePatienceSpeedMultiplier : 1.0f;
            remainingPatience -= Time.deltaTime * speedMultiplier;

            float normalizedPatience = Mathf.Clamp01(remainingPatience / maxPatience);

            if (bubbleUI != null)
            {
                bubbleUI.UpdatePatienceBar(normalizedPatience);
            }

            // ⏳ حساب الثواني الحقيقية المتبقية (Real Seconds Remaining)
            float realSecondsRemaining = remainingPatience / speedMultiplier;

            // 🔊🫨 تشغيل الصوت والاهتزاز معاً في نفس اللحظة تماماً عند وصول 3 ثوانٍ
            if (realSecondsRemaining <= 3.0f && !hasPlayedWarningSound)
            {
                hasPlayedWarningSound = true;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySfx("TimerWarning");
                }

                if (bubbleUI != null)
                {
                    bubbleUI.StartWarningShake();
                }
            }

            if (remainingPatience <= 0f)
            {
                OnPatienceExpired();
            }
        }

        private void OnPatienceExpired()
        {
            LeaveUnsatisfied();
        }

        public bool TryDeliverItem(string itemID, Transform playerTransform)
        {
            if (currentState != CustomerState.WaitingForOrder || currentOrder == null) return false;

            bool isNeeded = false;
            foreach (var item in currentOrder.items)
            {
                if (item.itemData != null && item.itemData.itemID == itemID)
                {
                    isNeeded = true;
                    break;
                }
            }

            if (!isNeeded || deliveredItemIDs.Contains(itemID)) return false;

            deliveredItemIDs.Add(itemID);

            if (bubbleUI != null)
            {
                bubbleUI.MarkItemAsDelivered(itemID, playerTransform);
            }

            // مكافأة الوقت: استعادة 25% من الصبر
            float patienceBonus = maxPatience * 0.25f;
            remainingPatience = Mathf.Min(maxPatience, remainingPatience + patienceBonus);

            float speedMultiplier = IsZombieStage() ? zombiePatienceSpeedMultiplier : 1.0f;
            if ((remainingPatience / speedMultiplier) > 3.0f)
            {
                hasPlayedWarningSound = false;
                if (bubbleUI != null) bubbleUI.StopWarningShake();
            }

            if (deliveredItemIDs.Count >= currentOrder.items.Count)
            {
                CompleteOrderSuccessfully();
            }

            return true;
        }

        public void CompleteOrderSuccessfully()
        {
            bool isZombie = IsZombieStage();
            float timeRatio = remainingPatience / maxPatience;

            if (timeRatio >= 0.70f)
            {
                if (isZombie)
                {
                    RestorePatienceToOtherCustomers(2.0f);
                    Debug.Log("<color=green>[Zombie Rage Bonus]</color> Fast zombie delivery restored patience to others!");
                }
                else
                {
                    Debug.Log("<color=yellow>[Speed Tip]</color> Customer gave a big tip for super fast service!");
                }
            }

            LeaveHappy();
        }

        private void RestorePatienceToOtherCustomers(float extraTime)
        {
            CustomerController[] allCustomers = FindObjectsOfType<CustomerController>();
            foreach (var customer in allCustomers)
            {
                if (customer != this && customer.CurrentState == CustomerState.WaitingForOrder)
                {
                    customer.AddBonusPatience(extraTime);
                }
            }
        }

        public void AddBonusPatience(float extraTime)
        {
            remainingPatience = Mathf.Min(maxPatience, remainingPatience + extraTime);

            float speedMultiplier = IsZombieStage() ? zombiePatienceSpeedMultiplier : 1.0f;
            if ((remainingPatience / speedMultiplier) > 3.0f)
            {
                hasPlayedWarningSound = false;
                if (bubbleUI != null) bubbleUI.StopWarningShake();
            }

            transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.2f);
        }

        public void LeaveHappy()
        {
            currentState = CustomerState.Leaving;
            if (bubbleUI != null) bubbleUI.HideBubble();

            if (CustomerSpawnerManager.Instance != null)
            {
                CustomerSpawnerManager.Instance.OnCustomerLeftQueue(this);
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnCustomerFinished(wasServed: true);
            }

            transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.3f);
            StartExitAnimation();
        }

        public void LeaveUnsatisfied()
        {
            currentState = CustomerState.Leaving;
            if (bubbleUI != null) bubbleUI.HideBubble();

            if (CustomerSpawnerManager.Instance != null)
            {
                CustomerSpawnerManager.Instance.OnCustomerLeftQueue(this);
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnCustomerFinished(wasServed: false);
            }

            StartExitAnimation();
        }

        private void StartExitAnimation()
        {
            StartWalkingJuice();

            transform.DOMove(transform.position + Vector3.left * 10f, 2.5f)
                .SetEase(Ease.Linear)
                .OnComplete(() => Destroy(gameObject));
        }

        #region Walk Animations (Juiciness)

        private void StartWalkingJuice()
        {
            StopWalkingJuice();
            bool isZombie = IsZombieStage();

            walkSequence = DOTween.Sequence();

            if (isZombie)
            {
                walkSequence.Append(transform.DORotate(new Vector3(0, 0, zombieSwayAngle), zombieStepDuration).SetEase(Ease.InOutSine))
                            .Join(transform.DOMoveY(transform.position.y - zombieLimpDrop, zombieStepDuration).SetEase(Ease.InOutSine))
                            .Append(transform.DORotate(new Vector3(0, 0, -zombieSwayAngle), zombieStepDuration).SetEase(Ease.InOutSine))
                            .Join(transform.DOMoveY(transform.position.y, zombieStepDuration).SetEase(Ease.InOutSine))
                            .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                walkSequence.Append(transform.DOMoveY(transform.position.y + humanHopAmount, humanHopDuration).SetEase(Ease.OutQuad))
                            .Append(transform.DOMoveY(transform.position.y, humanHopDuration).SetEase(Ease.InQuad))
                            .SetLoops(-1, LoopType.Restart);
            }
        }

        private void StopWalkingJuice()
        {
            if (walkSequence != null && walkSequence.IsActive())
            {
                walkSequence.Kill();
            }
            transform.rotation = Quaternion.identity;
        }

        private void OnDestroy()
        {
            StopWalkingJuice();
        }

        #endregion
    }
}