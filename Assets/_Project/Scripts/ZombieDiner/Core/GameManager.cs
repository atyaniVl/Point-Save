using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MoreMountains.Feedbacks; // لدعم Feel

namespace ZombieDiner.Core
{
   
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Current State")]
        [SerializeField] private GameStage currentStage = GameStage.Stage1_Normal;
        [SerializeField] private int playerHealth = 100;

        [Header("Juice & Visual Transition Settings")]
        [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
        [SerializeField] private Image backgroundImageUI;

        [Header("Stage Colors")]
        [SerializeField] private Color normalStageColor = new Color(0.8f, 0.9f, 1f, 1f);
        [SerializeField] private Color cutsceneColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        [SerializeField] private Color zombieStageColor = new Color(0.2f, 0.35f, 0.2f, 1f);

        [Header("Storyboard & Cutscene Settings")]
        [SerializeField] private GameObject storyboardPanel; // لوحة الـ UI للقصة/الكتسسين
        [SerializeField] private float glitchDuration = 1.5f; // مدة تأثير التشويش قبل إظهار الستوري بورد

        [Tooltip("إجمالي مدة الـ Storyboard بالثواني التي تتوزع عليها الصور")]
        [SerializeField] private float cutsceneDuration = 12.0f; // 👈 يمكنك التحكم بها وتعديلها من الـ Inspector

        [SerializeField] private MMF_Player glitchFeelFeedback; // اختياري: Feel Feedback للتشويش

        public GameStage CurrentStage => currentStage;
        public float CutsceneDuration => cutsceneDuration;

        public static event Action<GameStage> OnStageChanged;
        public static event Action OnGameOverTriggered;

        private Coroutine cutsceneCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (storyboardPanel != null)
                storyboardPanel.SetActive(false);
        }

        private void Start()
        {
            ApplyStageVisuals(currentStage, isInstant: true);
            OnStageChanged?.Invoke(currentStage);
        }

        public void ChangeStage(GameStage newStage)
        {
            if (currentStage == newStage) return;

            currentStage = newStage;
            Debug.Log($"<color=yellow>[GameManager]</color> Transitioned to Stage: {currentStage}");

            if (newStage == GameStage.Cutscene)
            {
                if (cutsceneCoroutine != null) StopCoroutine(cutsceneCoroutine);
                cutsceneCoroutine = StartCoroutine(GlitchAndShowStoryboardRoutine());
            }
            else
            {
                ApplyStageVisuals(currentStage, isInstant: false);
                OnStageChanged?.Invoke(currentStage);
            }

            if (newStage == GameStage.GameOver)
            {
                OnGameOverTriggered?.Invoke();
            }
        }

        private IEnumerator GlitchAndShowStoryboardRoutine()
        {
            // 1. تشغيل التشويش واهتزاز الشاشة المبدئي
            TriggerCameraShake(glitchDuration, 0.8f);

            if (glitchFeelFeedback != null)
            {
                glitchFeelFeedback.PlayFeedbacks();
            }

            ApplyStageVisuals(GameStage.Cutscene, isInstant: false);

            yield return new WaitForSeconds(glitchDuration);

            // 2. إظهار الستوري بورد بـ Scale ناعم وتأكيد تفعيله
            if (storyboardPanel != null)
            {
                storyboardPanel.SetActive(true);
                storyboardPanel.transform.localScale = Vector3.zero;
                storyboardPanel.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
            }
        }

        /// <summary>
        /// 🔹 يُستدعى للتحول لمرحلة الزومبي عند انتهاء الستوري بورد أو النقر لتخطيها
        /// </summary>
        public void StartZombieStageFromUI()
        {
            if (cutsceneCoroutine != null)
            {
                StopCoroutine(cutsceneCoroutine);
                cutsceneCoroutine = null;
            }

            if (storyboardPanel != null && storyboardPanel.activeSelf)
            {
                storyboardPanel.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    storyboardPanel.SetActive(false);
                    ChangeStage(GameStage.Stage2_Zombie);
                });
            }
            else
            {
                ChangeStage(GameStage.Stage2_Zombie);
            }
        }

        public void ApplyZombieAttackDamage(int damage)
        {
            playerHealth = Mathf.Max(0, playerHealth - damage);
            TriggerCameraShake(0.5f, 0.6f);

            if (playerHealth <= 0)
            {
                ChangeStage(GameStage.GameOver);
            }
        }

        private void ApplyStageVisuals(GameStage stage, bool isInstant)
        {
            Color targetColor = stage switch
            {
                GameStage.Stage1_Normal => normalStageColor,
                GameStage.Cutscene => cutsceneColor,
                GameStage.Stage2_Zombie => zombieStageColor,
                GameStage.GameOver => Color.black,
                _ => normalStageColor
            };

            float fadeDuration = isInstant ? 0f : 1f;

            if (backgroundSpriteRenderer != null)
                backgroundSpriteRenderer.DOColor(targetColor, fadeDuration).SetEase(Ease.InOutQuad);

            if (backgroundImageUI != null)
                backgroundImageUI.DOColor(targetColor, fadeDuration).SetEase(Ease.InOutQuad);
        }

        public void TriggerCameraShake(float duration, float strength)
        {
            if (Camera.main != null)
            {
                Camera.main.transform.DOKill();
                Camera.main.transform.DOShakePosition(duration, strength: strength, vibrato: 20);
            }
        }

        private void OnDestroy()
        {
            if (Camera.main != null)
                Camera.main.transform.DOKill();
        }
    }
}