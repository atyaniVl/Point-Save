using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZombieDiner.Core;

namespace ZombieDiner.Testing
{
    public class GrayboxGameManagerTester : MonoBehaviour
    {
        [Header("UI References (Graybox Visuals)")]
        [SerializeField] private Image backgroundPanel;
        [SerializeField] private TextMeshProUGUI stageText;

        [Header("Stage Colors for Graybox")]
        [SerializeField] private Color stage1Color = Color.green;
        [SerializeField] private Color cutsceneColor = Color.black;
        [SerializeField] private Color stage2Color = Color.red;
        [SerializeField] private Color gameOverColor = Color.gray;

        private void OnEnable()
        {
            GameManager.OnStageChanged += HandleStageChanged;
            GameManager.OnGameOverTriggered += HandleGameOver;
        }

        private void OnDisable()
        {
            GameManager.OnStageChanged -= HandleStageChanged;
            GameManager.OnGameOverTriggered -= HandleGameOver;
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GameManager.Instance.ChangeStage(GameStage.Stage1_Normal);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GameManager.Instance.ChangeStage(GameStage.Cutscene);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GameManager.Instance.ChangeStage(GameStage.Stage2_Zombie);
            }

            // تم التغيير إلى KeyCode.G لتجنب التضارب مع زر Space الخاطئ
            if (Input.GetKeyDown(KeyCode.G))
            {
                GameManager.Instance.ChangeStage(GameStage.GameOver);
            }
        }

        private void HandleStageChanged(GameStage newStage)
        {
            Debug.Log($"<color=green>[EVENT RECEIVED] Game Stage Changed To: {newStage}</color>");

            Color targetColor = newStage switch
            {
                GameStage.Stage1_Normal => stage1Color,
                GameStage.Cutscene => cutsceneColor,
                GameStage.Stage2_Zombie => stage2Color,
                GameStage.GameOver => gameOverColor,
                _ => Color.white
            };

            if (backgroundPanel != null)
            {
                backgroundPanel.color = targetColor;
            }

            if (stageText != null)
            {
                stageText.text = $"Current Stage: {newStage}";
            }
        }

        private void HandleGameOver()
        {
            Debug.Log("<color=red>[EVENT RECEIVED] Game Over Event Fired!</color>");
        }
    }
}