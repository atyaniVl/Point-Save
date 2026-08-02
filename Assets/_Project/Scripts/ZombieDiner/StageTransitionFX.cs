using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MoreMountains.Feedbacks;
using GenericSceneManagement;
using ZombieDiner.Core;

namespace ZombieDiner.Juice
{
    public class StageTransitionFX : MonoBehaviour
    {
        [Header("MMF Cutscene Player")]
        [SerializeField] private MMF_Player cutsceneMMFPlayer;
        [SerializeField] private float cutsceneDuration = 3.5f;
        [SerializeField] private string stage2SceneName = "Core Game Stage 2";

        [Header("Glitch Flash UI")]
        [SerializeField] private Image redFlashOverlay;

        [Header("Camera Shake Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float shakeDuration = 1.2f;
        [SerializeField] private float shakeStrength = 0.8f;

        private bool transitionTriggered = false;

        private void OnEnable()
        {
            GameManager.OnStageChanged += HandleStageChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStageChanged -= HandleStageChanged;
        }

        private void HandleStageChanged(GameStage newStage)
        {
            if (newStage == GameStage.Cutscene)
            {
                PlayCutsceneAndTransition();
            }
            else if (newStage == GameStage.Stage2_Zombie)
            {
                TriggerZombieTransition();
            }
        }

        public void PlayCutsceneAndTransition()
        {
            if (transitionTriggered) return;
            transitionTriggered = true;

            TriggerZombieTransition();

            if (cutsceneMMFPlayer != null)
            {
                cutsceneMMFPlayer.Events.OnComplete.AddListener(OnCutsceneComplete);
                cutsceneMMFPlayer.PlayFeedbacks();
            }

            // Fallback timer in case MMF_Player has no feedbacks or doesn't fire OnComplete
            DOVirtual.DelayedCall(cutsceneDuration, OnCutsceneComplete).SetId("StageTransitionTimer");
        }

        private void OnCutsceneComplete()
        {
            DOTween.Kill("StageTransitionTimer");
            if (cutsceneMMFPlayer != null)
            {
                cutsceneMMFPlayer.Events.OnComplete.RemoveListener(OnCutsceneComplete);
            }

            Debug.Log($"<color=green>[StageTransitionFX] Cutscene completed. Loading {stage2SceneName}...</color>");
            SceneLoader.Load(stage2SceneName);
        }

        /// <summary>
        /// 🔹 الوميض الأحمر المرعب + اهتزاز الشاشة عند دخول Stage 2
        /// </summary>
        public void TriggerZombieTransition()
        {
            // 1. اهتزاز الكاميرا
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.DOShakePosition(shakeDuration, shakeStrength, vibrato: 20);
            }

            // 2. الوميض الأحمر المتكرر (Glitch Red Flash)
            if (redFlashOverlay != null)
            {
                redFlashOverlay.gameObject.SetActive(true);
                Color c = redFlashOverlay.color;
                c.a = 0f;
                redFlashOverlay.color = c;

                // 3 خفقات سريعة حمراء
                Sequence flashSeq = DOTween.Sequence();
                flashSeq.Append(redFlashOverlay.DOFade(0.75f, 0.1f))
                        .Append(redFlashOverlay.DOFade(0f, 0.1f))
                        .Append(redFlashOverlay.DOFade(0.85f, 0.12f))
                        .Append(redFlashOverlay.DOFade(0f, 0.15f))
                        .Append(redFlashOverlay.DOFade(1f, 0.2f))
                        .Append(redFlashOverlay.DOFade(0f, 0.5f))
                        .OnComplete(() => redFlashOverlay.gameObject.SetActive(false));
            }
        }
    }
}