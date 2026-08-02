using System.Collections.Generic;
using UnityEngine;
using ZombieDiner.Core;
using ZombieDiner.Orders;
using ZombieDiner.Gameplay;

namespace ZombieDiner.Customers
{
    public class CustomerSpawnerManager : MonoBehaviour
    {
        public static CustomerSpawnerManager Instance { get; private set; }

        [Header("Prefab & Multi-Spawn Points")]
        [SerializeField] private GameObject customerPrefab;

        [Tooltip("نقاط التوليد من اليسار (ضع النقطتين هنا)")]
        [SerializeField] private Transform[] spawnPointsLeft;

        [Tooltip("نقاط التوليد من اليمين (ضع النقطتين هنا)")]
        [SerializeField] private Transform[] spawnPointsRight;

        [Header("Queue Center & Spacing")]
        [Tooltip("نقطة المنتصف حيث يقف أول زبون تماماً")]
        [SerializeField] private Transform centerPoint;

        [Tooltip("المسافة الأفقية بين كل زبون والذي يليه لتفادي التداخل")]
        [SerializeField] private float queueSpacing = 3.2f;

        [Tooltip("الحد الأقصى لعدد الزباين في الطابور")]
        [SerializeField] private int maxQueueCapacity = 5;

        private CustomerController[] activeSlots;
        private float spawnTimer;

        private int leftCount = 0;
        private int rightCount = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeSlots();
        }

        private void InitializeSlots()
        {
            if (activeSlots == null || activeSlots.Length != maxQueueCapacity)
            {
                activeSlots = new CustomerController[maxQueueCapacity];
            }
        }

        public bool IsQueueFull()
        {
            InitializeSlots();
            for (int i = 0; i < activeSlots.Length; i++)
            {
                if (activeSlots[i] == null) return false;
            }
            return true;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
               (GameManager.Instance.CurrentStage == GameStage.Cutscene ||
                GameManager.Instance.CurrentStage == GameStage.GameOver))
            {
                return;
            }

            if (WaveManager.Instance != null && !WaveManager.Instance.CanSpawnMoreCustomers())
            {
                return;
            }

            if (IsQueueFull()) return;

            spawnTimer += Time.deltaTime;

            float currentSpawnInterval = DifficultyScalingSystem.Instance != null
                ? DifficultyScalingSystem.Instance.CurrentSpawnInterval
                : 5f;

            if (spawnTimer >= currentSpawnInterval)
            {
                spawnTimer = 0f;
                TrySpawnCustomer();
            }
        }

        public void TrySpawnCustomer()
        {
            if (IsQueueFull()) return;

            // 1. تحديد رقم الخانة في الطابور
            int assignedSlotIndex = DecideNextSlotIndex();
            if (assignedSlotIndex == int.MinValue) return;

            Vector3 targetQueuePos = GetQueuePositionFromSlot(assignedSlotIndex);

            // 2. تحديد الجهة (يسار أم يمين)
            bool isLeftCustomer = (assignedSlotIndex < 0);
            if (assignedSlotIndex == 0)
            {
                isLeftCustomer = Random.value > 0.5f;
            }

            // 3. اختيار نقطة التوليد المناسبة من القائمة (حسب عمق الخانة)
            Transform selectedSpawnPoint = GetSpawnPointForSlot(assignedSlotIndex, isLeftCustomer);

            Vector3 spawnPos = selectedSpawnPoint != null ? selectedSpawnPoint.position : targetQueuePos;

            // 4. التوليد والإسناد
            GameObject newCustomerGO = Instantiate(customerPrefab, spawnPos, Quaternion.identity);
            CustomerController customer = newCustomerGO.GetComponent<CustomerController>();

            if (customer != null)
            {
                int arrayIndex = SlotToArrayIndex(assignedSlotIndex);
                activeSlots[arrayIndex] = customer;

                customer.SetExitPoint(selectedSpawnPoint);

                SpriteRenderer sr = newCustomerGO.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 100 - Mathf.Abs(assignedSlotIndex);
                }

                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.RegisterSpawnedCustomer();
                }

                OrderData generatedOrder = OrderGenerator.Instance != null
                    ? OrderGenerator.Instance.GenerateRandomOrder()
                    : null;

                float patienceTime = DifficultyScalingSystem.Instance != null
                    ? DifficultyScalingSystem.Instance.CurrentAllowedDeliveryTime
                    : 15f;

                customer.InitializeInQueue(targetQueuePos, generatedOrder, patienceTime);
            }
        }

        /// <summary>
        /// اختيار نقطة الـ Spawn بناءً على تسلسل الخانة (الزبون الأول يأخذ النقطة 0، والزبون الثاني يأخذ النقطة 1)
        /// </summary>
        private Transform GetSpawnPointForSlot(int slotIndex, bool isLeft)
        {
            Transform[] points = isLeft ? spawnPointsLeft : spawnPointsRight;
            if (points == null || points.Length == 0) return null;

            // حساب عمق الخانة (1 للزبون الأول في الجهة، 2 للزبون الثاني...)
            int depthIndex = Mathf.Abs(slotIndex);
            if (slotIndex == 0) depthIndex = 0;
            else depthIndex = depthIndex - 1; // تحويله لـ Index يبدأ من 0

            int pointIndex = Mathf.Clamp(depthIndex, 0, points.Length - 1);
            return points[pointIndex];
        }

        private int DecideNextSlotIndex()
        {
            if (activeSlots[0] == null) return 0;

            List<int> availableOptions = new List<int>();

            int nextLeft = -(leftCount + 1);
            if (CanOccupySlot(nextLeft)) availableOptions.Add(nextLeft);

            int nextRight = (rightCount + 1);
            if (CanOccupySlot(nextRight)) availableOptions.Add(nextRight);

            if (availableOptions.Count == 0)
            {
                for (int i = 1; i < maxQueueCapacity; i++)
                {
                    if (CanOccupySlot(-i)) { availableOptions.Add(-i); break; }
                }
                for (int i = 1; i < maxQueueCapacity; i++)
                {
                    if (CanOccupySlot(i)) { availableOptions.Add(i); break; }
                }
            }

            if (availableOptions.Count == 0) return int.MinValue;

            int chosenSlot = availableOptions[Random.Range(0, availableOptions.Count)];

            if (chosenSlot < 0 && Mathf.Abs(chosenSlot) > leftCount) leftCount = Mathf.Abs(chosenSlot);
            else if (chosenSlot > 0 && chosenSlot > rightCount) rightCount = chosenSlot;

            return chosenSlot;
        }

        private bool CanOccupySlot(int slotIndex)
        {
            int arrayIndex = SlotToArrayIndex(slotIndex);
            return arrayIndex >= 0 && arrayIndex < maxQueueCapacity && activeSlots[arrayIndex] == null;
        }

        public Vector3 GetQueuePositionFromSlot(int slotIndex)
        {
            Vector3 baseCenter = centerPoint != null ? centerPoint.position : transform.position;

            float xOffset = slotIndex * queueSpacing;
            float zOffset = Mathf.Abs(slotIndex) * 0.05f;

            return baseCenter + new Vector3(xOffset, 0f, zOffset);
        }

        private int SlotToArrayIndex(int slotIndex)
        {
            if (slotIndex == 0) return 0;
            if (slotIndex < 0) return (Mathf.Abs(slotIndex) * 2) - 1;
            return slotIndex * 2;
        }

        public void OnCustomerLeftQueue(CustomerController customer)
        {
            InitializeSlots();
            for (int i = 0; i < activeSlots.Length; i++)
            {
                if (activeSlots[i] == customer)
                {
                    activeSlots[i] = null;
                    break;
                }
            }

            RecalculateSideCounts();
        }

        private void RecalculateSideCounts()
        {
            leftCount = 0;
            rightCount = 0;

            for (int i = 1; i < maxQueueCapacity; i++)
            {
                if (!CanOccupySlot(-i)) leftCount = i;
                if (!CanOccupySlot(i)) rightCount = i;
            }
        }
    }
}