public interface ISettingsStorage
{
    void Save(SettingsData data);
    SettingsData Load();
}