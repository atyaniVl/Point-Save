using System;
using UnityEngine;
using AudioSystem;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public SettingsData Current { get; private set; }

    public event Action<bool> OnCRTChanged;
    public event Action<bool> OnCameraShakeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Current = new SettingsData();
        Load();
    }

    private void Start()
    {
        // تطبيق القيم المحفوظة عند بداية اللعبة
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(Current.MusicVolume);
            AudioManager.Instance.SetSfxVolume(Current.SfxVolume);
        }
    }

    public void SetMusic(float value)
    {
        Current.MusicVolume = value;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        Save();
    }

    public void SetSfx(float value)
    {
        Current.SfxVolume = value;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
        Save();
    }

    public void SetScreenShake(bool value)
    {
        Current.ScreenShake = value;
        OnCameraShakeChanged?.Invoke(value);
        Save();
    }

    public void SetCRT(bool value)
    {
        Current.CRTEnabled = value;
        OnCRTChanged?.Invoke(value);
        Save();
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("Music", Current.MusicVolume);
        PlayerPrefs.SetFloat("SFX", Current.SfxVolume);
        PlayerPrefs.SetInt("Shake", Current.ScreenShake ? 1 : 0);
        PlayerPrefs.SetInt("CRT", Current.CRTEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        Current.MusicVolume = PlayerPrefs.GetFloat("Music", 1);
        Current.SfxVolume = PlayerPrefs.GetFloat("SFX", 1);
        Current.ScreenShake = PlayerPrefs.GetInt("Shake", 1) == 1;
        Current.CRTEnabled = PlayerPrefs.GetInt("CRT", 1) == 1;
    }
}