using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

        public GameStage CurrentStage => currentStage;

        public static event Action<GameStage> OnStageChanged;
        public static event Action OnGameOverTriggered;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            ApplyStageVisuals(currentStage, isInstant: true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeStage(GameStage.Stage1_Normal);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeStage(GameStage.Cutscene);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeStage(GameStage.Stage2_Zombie);
            if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeStage(GameStage.GameOver);
        }

        public void ChangeStage(GameStage newStage)
        {
            if (currentStage == newStage) return;

            currentStage = newStage;
            Debug.Log($"<color=yellow>[GameManager]</color> Transitioned to Stage: {currentStage}");

            ApplyStageVisuals(currentStage, isInstant: false);
            OnStageChanged?.Invoke(currentStage);

            if (newStage == GameStage.GameOver)
            {
                OnGameOverTriggered?.Invoke();
            }
        }

        /// <summary>
        /// خصم صحة اللاعب عند هجوم الزومبي
        /// </summary>
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
            Color targetColor = normalStageColor;

            switch (stage)
            {
                case GameStage.Stage1_Normal:
                    targetColor = normalStageColor;
                    break;
                case GameStage.Cutscene:
                    targetColor = cutsceneColor;
                    TriggerCameraShake(0.3f, 0.2f);
                    break;
                case GameStage.Stage2_Zombie:
                    targetColor = zombieStageColor;
                    TriggerCameraShake(0.6f, 0.5f);
                    break;
                case GameStage.GameOver:
                    targetColor = Color.black;
                    break;
            }

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
                Camera.main.transform.DOShakePosition(duration, strength: strength, vibrato: 12);
            }
        }

        private void OnDestroy()
        {
            if (Camera.main != null) Camera.main.transform.DOKill();
        }
    }
}