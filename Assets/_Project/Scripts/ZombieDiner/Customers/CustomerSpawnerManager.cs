using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ZombieDiner.Core;
using ZombieDiner.Orders;
using ZombieDiner.Gameplay;

namespace ZombieDiner.Customers
{
    public class CustomerSpawnerManager : MonoBehaviour
    {
        public static CustomerSpawnerManager Instance { get; private set; }

        [Header("Prefab & Spawn Point")]
        [Tooltip("Customer Prefab containing CustomerController and World Space Canvas UI")]
        [SerializeField] private GameObject customerPrefab;

        [Tooltip("Point where customers appear initially before moving to counter")]
        [SerializeField] private Transform spawnPoint;

        [Header("Dynamic Queue Settings")]
        [Tooltip("نقطة بداية الطابور (الزبون الأول الملافي للخدمة)")]
        [SerializeField] private Transform queueStartPoint;

        [Tooltip("المسافة الفاصلة بين كل زبون والذي يليه في الصف")]
        [SerializeField] private float queueSpacing = 1.5f;

        [Tooltip("اتجاه امتداد الطابور (مثال: Vector2.right أو Vector2.left)")]
        [SerializeField] private Vector2 queueDirection = Vector2.right;

        [Tooltip("الحد الأقصى لعدد الزباين في الطابور بنفس الوقت")]
        [SerializeField] private int maxQueueCapacity = 5;

        // قائمة تتبع الزباين في الصف حالياً حسب الترتيب
        private List<CustomerController> activeQueue = new List<CustomerController>();

        private float spawnTimer;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            // إيقاف التوليد في حالة الـ Cutscene أو GameOver
            if (GameManager.Instance != null &&
               (GameManager.Instance.CurrentStage == GameStage.Cutscene ||
                GameManager.Instance.CurrentStage == GameStage.GameOver))
            {
                return;
            }

            // فحص حد التوليد لـ Stage 1 (الـ 10 زباين) من الـ WaveManager
            if (WaveManager.Instance != null && !WaveManager.Instance.CanSpawnMoreCustomers())
            {
                return;
            }

            spawnTimer += Time.deltaTime;

            // جلب معدل التوليد حسب الصعوبة
            float currentSpawnInterval = DifficultyScalingSystem.Instance != null
                ? DifficultyScalingSystem.Instance.CurrentSpawnInterval
                : 5f;

            if (spawnTimer >= currentSpawnInterval)
            {
                spawnTimer = 0f;
                TrySpawnCustomer();
            }
        }

        /// <summary>
        /// توليد زبون جديد وإضافته إلى نهاية الطابور إذا كان هناك متسع.
        /// </summary>
        public void TrySpawnCustomer()
        {
            if (activeQueue.Count >= maxQueueCapacity)
            {
                // الطابور ممتلئ، انتظار الدورة القادمة
                return;
            }

            // 1. حساب موضع الوقوف المستهدف في الطابور بناءً على ترتيب الزبون (Index)
            Vector3 targetQueuePos = GetQueuePosition(activeQueue.Count);
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : targetQueuePos;

            GameObject newCustomerGO = Instantiate(customerPrefab, spawnPos, Quaternion.identity);
            CustomerController customer = newCustomerGO.GetComponent<CustomerController>();

            if (customer != null)
            {
                // إضافة الزبون للقائمة فوراً لحجز موقعه في الصف
                activeQueue.Add(customer);

                // إبلاغ الـ WaveManager بزيادة العداد
                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.RegisterSpawnedCustomer();
                }

                // توليد بيانات الطلب والصبر
                OrderData generatedOrder = OrderGenerator.Instance != null
                    ? OrderGenerator.Instance.GenerateRandomOrder()
                    : null;

                float patienceTime = DifficultyScalingSystem.Instance != null
                    ? DifficultyScalingSystem.Instance.CurrentAllowedDeliveryTime
                    : 15f;

                // تهيئة الزبون والبدء بالتحرك لنقطته في الطابور
                customer.InitializeInQueue(targetQueuePos, generatedOrder, patienceTime);
            }
        }

        /// <summary>
        /// 🔹 حساب الموقع المطلوب في الصف بناءً على الـ Index والـ Spacing
        /// </summary>
        public Vector3 GetQueuePosition(int index)
        {
            Vector3 startPos = queueStartPoint != null ? queueStartPoint.position : transform.position;
            Vector3 offset = (Vector3)(queueDirection.normalized * (index * queueSpacing));
            return startPos + offset;
        }

        /// <summary>
        /// 🔹 يُستدعى بواسطة CustomerController عند مغادرة الزبون لتحريك بقية الصف للأمام
        /// </summary>
        public void OnCustomerLeftQueue(CustomerController customer)
        {
            if (activeQueue.Contains(customer))
            {
                activeQueue.Remove(customer);
                ShiftQueueForward();
            }
        }

        /// <summary>
        /// 🔹 إزاحة باقي الزباين في الطابور للأمام بنعومة (Smooth Queue Shift)
        /// </summary>
        private void ShiftQueueForward()
        {
            for (int i = 0; i < activeQueue.Count; i++)
            {
                if (activeQueue[i] != null)
                {
                    Vector3 newPosition = GetQueuePosition(i);
                    activeQueue[i].MoveToQueuePosition(newPosition);
                }
            }
        }
    }
}