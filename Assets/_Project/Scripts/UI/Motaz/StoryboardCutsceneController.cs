using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using AudioSystem;
using ZombieDiner.Core;

namespace ZombieDiner.UI
{
    public enum PanelDirection
    {
        FromLeft,
        FromRight,
        FromTop,
        FromBottom,
        ZoomIn
    }

    public class StoryboardCutsceneController : MonoBehaviour
    {
        [Header("UI Comic Panels (st-1 to st-6)")]
        [SerializeField] private List<CanvasGroup> panelCanvasGroups;
        [SerializeField] private List<RectTransform> panelRectTransforms;

        [Header("Custom Animation Directions")]
        [Tooltip("حدد اتجاه الحركة لكل صورة بالترتيب")]
        [SerializeField]
        private List<PanelDirection> panelDirections = new List<PanelDirection>
        {
            PanelDirection.FromLeft,   // st-1: المطعم الهادئ
            PanelDirection.FromTop,    // st-2: ظهور الفيروس
            PanelDirection.FromRight,  // st-3: هجوم الزومبي
            PanelDirection.FromBottom, // st-4: الطباخ الخائف
            PanelDirection.ZoomIn,     // st-5: قائمة الطعام المقززة
            PanelDirection.FromRight   // st-6: المطعم المكتظ
        };

        [Header("Cutscene Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float slideOffset = 400f; // مسافة البداية من خارج الشاشة

        [Header("Controls & Skip")]
        [SerializeField] private Button skipButton;
        [SerializeField] private GameObject clickToContinuePrompt;

        private int currentPanelIndex = 0;
        private bool isCutsceneFinished = false;
        private List<Vector2> originalAnchoredPositions = new List<Vector2>();

        private void Awake()
        {
            // حفظ المواقع الأصلية
            originalAnchoredPositions.Clear();
            for (int i = 0; i < panelCanvasGroups.Count; i++)
            {
                RectTransform rect = GetRectTransform(i);
                if (rect != null)
                {
                    originalAnchoredPositions.Add(rect.anchoredPosition);
                }
                else
                {
                    originalAnchoredPositions.Add(Vector2.zero);
                }
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(SkipCutscene);
            }

            if (clickToContinuePrompt != null)
            {
                clickToContinuePrompt.SetActive(false);
            }
        }

        private void OnEnable()
        {
            ResetPanels();
            StartCoroutine(PlayStoryboardSequence());
        }

        private void ResetPanels()
        {
            currentPanelIndex = 0;
            isCutsceneFinished = false;

            for (int i = 0; i < panelCanvasGroups.Count; i++)
            {
                if (panelCanvasGroups[i] != null)
                {
                    panelCanvasGroups[i].alpha = 0f;
                    RectTransform rect = GetRectTransform(i);
                    if (rect != null && i < originalAnchoredPositions.Count)
                    {
                        rect.DOKill();
                        SetupPanelStartPosition(rect, i);
                    }
                }
            }
        }

        /// <summary>
        /// تعيين نقطة بداية الصورة حسب الاتجاه المخصص لها
        /// </summary>
        private void SetupPanelStartPosition(RectTransform rect, int index)
        {
            PanelDirection dir = (index < panelDirections.Count) ? panelDirections[index] : PanelDirection.FromLeft;
            Vector2 basePos = originalAnchoredPositions[index];

            switch (dir)
            {
                case PanelDirection.FromLeft:
                    rect.anchoredPosition = basePos + new Vector2(-slideOffset, 0f);
                    rect.localScale = Vector3.one;
                    break;
                case PanelDirection.FromRight:
                    rect.anchoredPosition = basePos + new Vector2(slideOffset, 0f);
                    rect.localScale = Vector3.one;
                    break;
                case PanelDirection.FromTop:
                    rect.anchoredPosition = basePos + new Vector2(0f, slideOffset);
                    rect.localScale = Vector3.one;
                    break;
                case PanelDirection.FromBottom:
                    rect.anchoredPosition = basePos + new Vector2(0f, -slideOffset);
                    rect.localScale = Vector3.one;
                    break;
                case PanelDirection.ZoomIn:
                    rect.anchoredPosition = basePos;
                    rect.localScale = Vector3.one * 0.6f;
                    break;
            }
        }

        private IEnumerator PlayStoryboardSequence()
        {
            // توزيع الوقت الزمني الإجمالي المحدد من الـ GameManager على عدد اللوحات
            float totalDuration = (GameManager.Instance != null) ? GameManager.Instance.CutsceneDuration : 12.0f;
            int panelCount = panelCanvasGroups.Count;
            float autoAdvanceDelay = panelCount > 0 ? (totalDuration / panelCount) : 2.0f;

            while (currentPanelIndex < panelCanvasGroups.Count)
            {
                yield return StartCoroutine(ShowPanel(currentPanelIndex));
                currentPanelIndex++;

                yield return new WaitForSeconds(autoAdvanceDelay);
            }

            OnCutsceneComplete();
        }

        private IEnumerator ShowPanel(int index)
        {
            if (index < 0 || index >= panelCanvasGroups.Count || panelCanvasGroups[index] == null)
                yield break;

            CanvasGroup cg = panelCanvasGroups[index];
            RectTransform rect = GetRectTransform(index);
            PanelDirection dir = (index < panelDirections.Count) ? panelDirections[index] : PanelDirection.FromLeft;

            PlayAudioForPanel(index);

            Sequence seq = DOTween.Sequence();

            // 1. أنيميشن الـ Fade
            seq.Join(cg.DOFade(1f, fadeDuration));

            // 2. أنيميشن الحركة أو التكبير حسب الاتجاه
            if (rect != null && index < originalAnchoredPositions.Count)
            {
                if (dir == PanelDirection.ZoomIn)
                {
                    seq.Join(rect.DOScale(Vector3.one, fadeDuration).SetEase(Ease.OutBack));
                }
                else
                {
                    seq.Join(rect.DOAnchorPos(originalAnchoredPositions[index], fadeDuration).SetEase(Ease.OutCubic));
                }
            }

            yield return seq.WaitForCompletion();
        }

        private RectTransform GetRectTransform(int index)
        {
            if (index < panelRectTransforms.Count && panelRectTransforms[index] != null)
                return panelRectTransforms[index];

            if (index < panelCanvasGroups.Count && panelCanvasGroups[index] != null)
                return panelCanvasGroups[index].GetComponent<RectTransform>();

            return null;
        }

        private void PlayAudioForPanel(int index)
        {
            if (AudioManager.Instance == null) return;

            switch (index)
            {
                case 0:
                    AudioManager.Instance.PlaySfx("RestaurantAmbience");
                    break;
                case 1:
                    AudioManager.Instance.PlaySfx("VirusAlert");
                    break;
                case 2:
                    AudioManager.Instance.PlaySfx("ZombieRoarFar");
                    break;
                case 3:
                    AudioManager.Instance.PlaySfx("ChefGasp");
                    break;
                case 4:
                    AudioManager.Instance.PlaySfx("GrossSlime");
                    break;
                case 5:
                    AudioManager.Instance.PlaySfx("ZombieDinerCrowd");
                    break;
            }
        }

        public void SkipCutscene()
        {
            if (isCutsceneFinished) return;
            StopAllCoroutines();

            for (int i = 0; i < panelCanvasGroups.Count; i++)
            {
                if (panelCanvasGroups[i] != null)
                {
                    panelCanvasGroups[i].alpha = 1f;
                    RectTransform rect = GetRectTransform(i);
                    if (rect != null && i < originalAnchoredPositions.Count)
                    {
                        rect.DOKill();
                        rect.anchoredPosition = originalAnchoredPositions[i];
                        rect.localScale = Vector3.one;
                    }
                }
            }

            OnCutsceneComplete();
        }

        private void OnCutsceneComplete()
        {
            if (isCutsceneFinished) return;
            isCutsceneFinished = true;

            if (clickToContinuePrompt != null)
            {
                clickToContinuePrompt.SetActive(true);
            }

            Debug.Log("<color=green>[Cutscene]</color> Storyboard completed. Transitioning to Stage 2!");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartZombieStageFromUI();
            }
        }
    }
}