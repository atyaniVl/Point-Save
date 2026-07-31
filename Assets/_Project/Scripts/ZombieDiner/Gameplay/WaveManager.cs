using System;
using UnityEngine;
using ZombieDiner.Core;

namespace ZombieDiner.Gameplay
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Wave Settings")]
        [Tooltip("عدد الزباين المطلوب خدمتهم في Stage 1 للانتقال للـ Cutscene")]
        [SerializeField] private int stage1MaxCustomers = 10;

        [Header("Player Lives Settings")]
        [SerializeField] private int maxLives = 3;
        private int currentLives;

        private int spawnedCustomersCount = 0;
        private int processedCustomersCount = 0;

        public static event Action<int> OnLivesChanged;
        public static event Action<int> OnCustomerCountUpdated;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            ResetLives();
        }

        public void ResetLives()
        {
            currentLives = maxLives;
            OnLivesChanged?.Invoke(currentLives);
        }

        /// <summary>
        /// 🔹 الدالة المطلوبة: فحص هل يسمح بتوليد زباين إضافيين حسب المرحلة
        /// </summary>
        public bool CanSpawnMoreCustomers()
        {
            if (GameManager.Instance == null) return false;

            if (GameManager.Instance.CurrentStage == GameStage.Stage1_Normal)
            {
                return spawnedCustomersCount < stage1MaxCustomers;
            }

            return true; // في Stage 2 أو غيرها يكون التوليد مستمراً
        }

        /// <summary>
        /// 🔹 الدالة المطلوبة: تسجيل زبون جديد تم توليده
        /// </summary>
        public void RegisterSpawnedCustomer()
        {
            spawnedCustomersCount++;
        }

        public void OnCustomerFinished(bool wasServed)
        {
            processedCustomersCount++;
            OnCustomerCountUpdated?.Invoke(processedCustomersCount);

            if (!wasServed)
            {
                LoseLife();
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentStage == GameStage.Stage1_Normal)
            {
                if (processedCustomersCount >= stage1MaxCustomers)
                {
                    Debug.Log("<color=green>[WaveManager] Stage 1 Completed! Transitioning to Cutscene...</color>");
                    GameManager.Instance.ChangeStage(GameStage.Cutscene);
                }
            }
        }

        private void LoseLife()
        {
            currentLives = Mathf.Max(0, currentLives - 1);
            OnLivesChanged?.Invoke(currentLives);

            if (currentLives <= 0)
            {
                Debug.LogError("<color=red>[Game Over] All 3 lives lost!</color>");
                GameManager.Instance.ChangeStage(GameStage.GameOver);
            }
        }
    }
}