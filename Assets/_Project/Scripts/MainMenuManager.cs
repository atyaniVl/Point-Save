using UnityEngine;
using UnityEngine.UI;
using GenericSceneManagement;
using AudioSystem;

public class MainMenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private Material material;
    private void OnEnable()
    {
        playButton.onClick.AddListener(PlayGame);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(QuitGame);


    }
    private void OnDisable()
    {
        playButton.onClick.RemoveListener(PlayGame);
        settingsButton.onClick.RemoveListener(OpenSettings);
        creditsButton.onClick.RemoveListener(OpenCredits);
        quitButton.onClick.RemoveListener(QuitGame);

        SettingsManager.Instance.OnCRTChanged -= UpdateCRT;
    }
    void Start()
    {
        AudioManager.Instance.PlayMusic("TestingMusicClip");

        UpdateCRT(SettingsManager.Instance.Current.CRTEnabled);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlayGame()
    {
        Debug.Log("Play Game");
        AudioManager.Instance.PlaySfx("TestingSfxClip");
    }
    void UpdateCRT(bool enabled)
    {
        material.SetFloat("_Intensity", enabled ? 1 : 0);
    }

    void OpenSettings()
    {
        SceneLoader.LoadAdditive("Settings");
        SettingsManager.Instance.OnCRTChanged += UpdateCRT;
    }
    void OpenCredits()
    {
        Debug.Log("Credits Opened");
    }
    void QuitGame()
    {
        Debug.Log("Quit Game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
