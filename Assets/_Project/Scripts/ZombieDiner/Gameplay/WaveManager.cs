using System;
using UnityEngine;
using ZombieDiner.Core;

namespace ZombieDiner.Gameplay
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Wave Settings")]
        [Tooltip("عدد الزباين المطلوب خدمتهم في Stage 1 للانتقال للـ Storyboard/Cutscene")]
        [SerializeField] private int stage1MaxCustomers = 5; // 👈 يمكنك تعديلها بحرية من الـ Inspector

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

        public bool CanSpawnMoreCustomers()
        {
            if (GameManager.Instance == null) return false;

            // في Stage 1 يتوقف التوليد إذا وصلنا للعدد المطلوب
            if (GameManager.Instance.CurrentStage == GameStage.Stage1_Normal)
            {
                return spawnedCustomersCount < stage1MaxCustomers;
            }

            // في Stage 2 يكون التوليد مستمراً
            return GameManager.Instance.CurrentStage == GameStage.Stage2_Zombie;
        }

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

            // فحص اكتمال Stage 1 للانتقال السينمائي
            if (GameManager.Instance != null && GameManager.Instance.CurrentStage == GameStage.Stage1_Normal)
            {
                if (processedCustomersCount >= stage1MaxCustomers)
                {
                    Debug.Log("<color=green>[WaveManager] Stage 1 Human Customers Finished! Starting Glitch & Storyboard...</color>");

                    // تحويل اللعبة لـ Cutscene لبدء التشويش والستوري بورد
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
                Debug.LogError("<color=red>[Game Over] All lives lost!</color>");
                GameManager.Instance.ChangeStage(GameStage.GameOver);
            }
        }
    }
}