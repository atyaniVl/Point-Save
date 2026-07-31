using UnityEngine;

public class PlayerPrefsSettingsStorage : ISettingsStorage
{
    public SettingsData Load()
    {
        return new SettingsData
        {
            MusicVolume = PlayerPrefs.GetFloat("Music", 1f),
            SfxVolume = PlayerPrefs.GetFloat("SFX", 1f),
            ScreenShake = PlayerPrefs.GetInt("Shake", 1) == 1,
            CRTEnabled = PlayerPrefs.GetInt("CRT", 1) == 1
        };
    }

    public void Save(SettingsData data)
    {
        PlayerPrefs.SetFloat("Music", data.MusicVolume);
        PlayerPrefs.SetFloat("SFX", data.SfxVolume);

        PlayerPrefs.SetInt("Shake", data.ScreenShake ? 1 : 0);
        PlayerPrefs.SetInt("CRT", data.CRTEnabled ? 1 : 0);

        PlayerPrefs.Save();
    }
}