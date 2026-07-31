using UnityEngine;
using UnityEngine.UI;
using ZombieDiner.Core;
using ZombieDiner.Gameplay;

namespace ZombieDiner.UI
{
    public class DynamicUITheme : MonoBehaviour
    {
        [Header("Currency Visuals")]
        [SerializeField] private Image currencyIconImage;
        [SerializeField] private Sprite normalCoinSprite;  // 🪙 صورة العملة العادية
        [SerializeField] private Sprite zombieCoinSprite;  // 💀 صورة عملة الزومبي

        [Header("Hearts / Lives Visuals")]
        [SerializeField] private Image[] heartImages;       // المصفوفة الخاصة بالأرواح الـ 3
        [SerializeField] private Sprite normalHeartSprite; // ❤️ القلوب الحمراء
        [SerializeField] private Sprite zombieHeartSprite; // 💚 قلوب الزومبي الخضراء/المكسورة
        [SerializeField] private Color emptyHeartColor = new Color(0.3f, 0.3f, 0.3f, 0.5f); // لون القلب الضائع

        private void OnEnable()
        {
            GameManager.OnStageChanged += HandleStageChanged;
            WaveManager.OnLivesChanged += UpdateLivesUI;
        }

        private void OnDisable()
        {
            GameManager.OnStageChanged -= HandleStageChanged;
            WaveManager.OnLivesChanged -= UpdateLivesUI;
        }

        private void HandleStageChanged(GameStage newStage)
        {
            bool isZombie = (newStage == GameStage.Stage2_Zombie);

            // 1. تغيير شكل أيقونة العملة
            if (currencyIconImage != null)
            {
                currencyIconImage.sprite = isZombie ? zombieCoinSprite : normalCoinSprite;
            }

            // 2. تغيير أشكال الأرواح مع الحفاظ على حالتها المفعلة
            if (heartImages != null)
            {
                foreach (var heart in heartImages)
                {
                    if (heart != null)
                    {
                        heart.sprite = isZombie ? zombieHeartSprite : normalHeartSprite;
                    }
                }
            }
        }

        /// <summary>
        /// تحديث عرض الأرواح المتبقية (إخفاء/بهت القلوب الخاسرة)
        /// </summary>
        private void UpdateLivesUI(int currentLives)
        {
            if (heartImages == null) return;

            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null)
                {
                    if (i < currentLives)
                    {
                        heartImages[i].color = Color.white; // مفعّل
                    }
                    else
                    {
                        heartImages[i].color = emptyHeartColor; // خاسر
                    }
                }
            }
        }
    }
}