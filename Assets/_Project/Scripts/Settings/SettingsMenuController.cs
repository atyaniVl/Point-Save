using DG.Tweening;
using GenericSceneManagement;
using UnityEngine;
using UnityEngine.UI;
public class SettingsMenuController : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Buttons")]
    [SerializeField] private Button crtButton;
    [SerializeField] private Button shakeButton;
    [SerializeField] private Button backButton;

    [Header("Labels (optional)")]
    [SerializeField] private TMPro.TMP_Text crtStateText;
    [SerializeField] private TMPro.TMP_Text shakeStateText;

    [SerializeField] private UIAnimationController animatorsController;

    private void Start()
    {
        InitializeUI();
        RegisterListeners();
    }

    private void InitializeUI()
    {
        var settings = SettingsManager.Instance.Current;

        musicSlider.SetValueWithoutNotify(settings.MusicVolume);
        sfxSlider.SetValueWithoutNotify(settings.SfxVolume);

        UpdateCRTUI(settings.CRTEnabled);
        UpdateShakeUI(settings.ScreenShake);
    }

    private void RegisterListeners()
    {
        musicSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMusic);
        sfxSlider.onValueChanged.AddListener(SettingsManager.Instance.SetSfx);

        crtButton.onClick.AddListener(OnCRTClicked);
        shakeButton.onClick.AddListener(OnShakeClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnCRTClicked()
    {
        bool enabled = !SettingsManager.Instance.Current.CRTEnabled;

        SettingsManager.Instance.SetCRT(enabled);
        UpdateCRTUI(enabled);
    }

    private void OnShakeClicked()
    {
        bool enabled = !SettingsManager.Instance.Current.ScreenShake;

        SettingsManager.Instance.SetScreenShake(enabled);
        UpdateShakeUI(enabled);
    }

    private void OnBackClicked()
    {
        // Close the additive settings scene

        animatorsController.RestartAll();

        Debug.Log($"Rewound DOTween animations under '{animatorsController.name}'.");
        SceneLoader.Unload("Settings");
    }

    private void UpdateCRTUI(bool enabled)
    {
        if (crtStateText != null)
            crtStateText.text = enabled ? "ON" : "OFF";
    }

    private void UpdateShakeUI(bool enabled)
    {
        if (shakeStateText != null)
            shakeStateText.text = enabled ? "ON" : "OFF";
    }
}