using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using ZombieDiner.Core;

namespace AudioSystem
{
    public class AudioManager : MonoBehaviour
    {
        public const string MasterVolumeKey = "Audio.Master.Volume";
        public const string SfxVolumeKey = "Audio.Sfx.Volume";
        public const string MusicVolumeKey = "Audio.Music.Volume";
        public const string MasterEnabledKey = "Audio.Master.Enabled";
        public const string SfxEnabledKey = "Audio.Sfx.Enabled";
        public const string MusicEnabledKey = "Audio.Music.Enabled";

        [Header("Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParameter = "MasterVolume";
        [SerializeField] private string sfxVolumeParameter = "SfxVolume";
        [SerializeField] private string musicVolumeParameter = "MusicVolume";

        [Header("Stage Music IDs")]
        [SerializeField] private string normalStageMusicID = "NormalBGM";
        [SerializeField] private string zombieStageMusicID = "ZombieBGM";

        [Header("Data")]
        [SerializeField] private List<SfxClipDataSO> sfxClips = new List<SfxClipDataSO>();
        [SerializeField] private List<MusicClipDataSO> musicClips = new List<MusicClipDataSO>();

        [Header("Sources")]
        [SerializeField] private Transform sfxSourceRoot;
        [SerializeField] private Transform musicSourceRoot;

        private readonly Dictionary<string, SfxClipDataSO> sfxLookup = new Dictionary<string, SfxClipDataSO>();
        private readonly Dictionary<string, MusicClipDataSO> musicLookup = new Dictionary<string, MusicClipDataSO>();
        private readonly Dictionary<string, AudioSource> musicSources = new Dictionary<string, AudioSource>();
        private readonly List<AudioSource> sfxSources = new List<AudioSource>();

        private bool masterEnabled = true;
        private bool sfxEnabled = true;
        private bool musicEnabled = true;

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            EnsureRoots();
            BuildLookup();
            InitializeSources();
            LoadSettings();
        }

        private void OnEnable()
        {
            GameManager.OnStageChanged += HandleStageMusic;
        }

        private void OnDisable()
        {
            GameManager.OnStageChanged -= HandleStageMusic;
        }

        private void Start()
        {
            // تشغيل موسيقى المرحلة الابتدائية بعد التأكد من تجهيز كل الـ AudioSources
            if (GameManager.Instance != null)
            {
                HandleStageMusic(GameManager.Instance.CurrentStage);
            }
        }

        private void HandleStageMusic(GameStage newStage)
        {
            switch (newStage)
            {
                case GameStage.Stage1_Normal:
                    StopAllMusic();
                    PlayMusic(normalStageMusicID, false);
                    break;

                case GameStage.Stage2_Zombie:
                    StopAllMusic();
                    PlayMusic(zombieStageMusicID, false);
                    break;

                case GameStage.GameOver:
                    StopAllMusic();
                    break;
            }
        }

        public void PlaySfx(string id)
        {
            PlaySfxInternal(id, 1f, false, 0f, 0f);
        }

        public void PlaySfx(string id, float pitch)
        {
            PlaySfxInternal(id, pitch, false, 0f, 0f);
        }

        public void PlaySfxRandomPitch(string id, float minPitch, float maxPitch)
        {
            PlaySfxInternal(id, 1f, true, minPitch, maxPitch);
        }

        public void PlayMusic(string id, bool restart = true)
        {
            if (!musicEnabled)
                return;

            if (!TryGetMusic(id, out var clipData))
            {
                Debug.LogWarning($"[AudioManager] Could not find Music SO with ID: {id}");
                return;
            }

            if (!musicSources.TryGetValue(id, out var source) || source == null)
            {
                Debug.LogWarning($"[AudioManager] AudioSource for Music ID '{id}' is missing!");
                return;
            }

            if (source.isPlaying && !restart)
                return;

            source.clip = clipData.Clip;
            source.loop = clipData.Loop;
            source.volume = clipData.Volume;
            source.outputAudioMixerGroup = clipData.MixerGroup;
            source.pitch = 1f;
            source.Play();
            Debug.Log($"<color=cyan>[AudioManager]</color> Now Playing Music: {id}");
        }

        public void StopMusic(string id)
        {
            if (!musicSources.TryGetValue(id, out var source) || source == null)
                return;

            source.Stop();
        }

        public void StopAllMusic()
        {
            foreach (var source in musicSources.Values)
            {
                if (source == null)
                    continue;

                source.Stop();
            }
        }

        public void StopAllSfx()
        {
            foreach (var source in sfxSources)
            {
                if (source == null)
                    continue;

                source.Stop();
            }
        }

        public void SetMasterVolume(float volume01)
        {
            SetVolume(masterVolumeParameter, volume01);
            PlayerPrefs.SetFloat(MasterVolumeKey, volume01);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float volume01)
        {
            SetVolume(sfxVolumeParameter, volume01);
            PlayerPrefs.SetFloat(SfxVolumeKey, volume01);
            PlayerPrefs.Save();
        }

        public void SetMusicVolume(float volume01)
        {
            SetVolume(musicVolumeParameter, volume01);
            PlayerPrefs.SetFloat(MusicVolumeKey, volume01);
            PlayerPrefs.Save();
        }

        public void SetMasterEnabled(bool enabled)
        {
            masterEnabled = enabled;
            ApplyMuteState();
            PlayerPrefs.SetInt(MasterEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetSfxEnabled(bool enabled)
        {
            sfxEnabled = enabled;
            ApplyMuteState();
            PlayerPrefs.SetInt(SfxEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetMusicEnabled(bool enabled)
        {
            musicEnabled = enabled;
            ApplyMuteState();
            PlayerPrefs.SetInt(MusicEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void PlaySfxInternal(string id, float pitch, bool randomPitch, float minPitch, float maxPitch)
        {
            if (!sfxEnabled)
                return;

            if (!TryGetSfx(id, out var clipData))
                return;

            var source = GetAvailableSfxSource();
            if (source == null)
                return;

            var clip = clipData.Clip;
            if (clip == null)
                return;

            source.clip = clip;
            source.loop = clipData.Loop;
            source.volume = clipData.Volume;
            source.outputAudioMixerGroup = clipData.MixerGroup;
            source.pitch = randomPitch ? Random.Range(minPitch, maxPitch) : pitch;

            source.Play();
        }

        private void BuildLookup()
        {
            sfxLookup.Clear();
            foreach (var clip in sfxClips)
            {
                if (clip == null || string.IsNullOrWhiteSpace(clip.Id))
                    continue;

                if (!sfxLookup.ContainsKey(clip.Id))
                    sfxLookup.Add(clip.Id, clip);
            }

            musicLookup.Clear();
            foreach (var clip in musicClips)
            {
                if (clip == null || string.IsNullOrWhiteSpace(clip.Id))
                    continue;

                if (!musicLookup.ContainsKey(clip.Id))
                    musicLookup.Add(clip.Id, clip);
            }
        }

        private bool TryGetSfx(string id, out SfxClipDataSO data)
        {
            return sfxLookup.TryGetValue(id, out data);
        }

        private bool TryGetMusic(string id, out MusicClipDataSO data)
        {
            return musicLookup.TryGetValue(id, out data);
        }

        private AudioSource GetAvailableSfxSource()
        {
            foreach (var source in sfxSources)
            {
                if (source != null && !source.isPlaying)
                    return source;
            }

            if (sfxSourceRoot != null)
            {
                var newSource = sfxSourceRoot.gameObject.AddComponent<AudioSource>();
                sfxSources.Add(newSource);
                return newSource;
            }

            return null;
        }

        private void EnsureRoots()
        {
            if (sfxSourceRoot == null)
            {
                var sfxGO = new GameObject("SFX_Sources");
                sfxGO.transform.SetParent(transform);
                sfxSourceRoot = sfxGO.transform;
            }

            if (musicSourceRoot == null)
            {
                var musicGO = new GameObject("Music_Sources");
                musicGO.transform.SetParent(transform);
                musicSourceRoot = musicGO.transform;
            }
        }

        private void InitializeSources()
        {
            foreach (var kvp in musicLookup)
            {
                if (kvp.Value == null) continue;
                var go = new GameObject($"MusicSource_{kvp.Key}");
                go.transform.SetParent(musicSourceRoot);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                musicSources[kvp.Key] = src;
            }
        }

        private void SetVolume(string parameterName, float volume01)
        {
            if (audioMixer == null) return;
            float dB = volume01 > 0.0001f ? Mathf.Log10(volume01) * 20f : -80f;
            audioMixer.SetFloat(parameterName, dB);
        }

        private void LoadSettings()
        {
            masterEnabled = PlayerPrefs.GetInt(MasterEnabledKey, 1) == 1;
            sfxEnabled = PlayerPrefs.GetInt(SfxEnabledKey, 1) == 1;
            musicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;

            SetMasterVolume(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
            SetSfxVolume(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
            SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));

            ApplyMuteState();
        }

        private void ApplyMuteState()
        {
            SetVolume(masterVolumeParameter, masterEnabled ? PlayerPrefs.GetFloat(MasterVolumeKey, 1f) : 0f);
            SetVolume(sfxVolumeParameter, sfxEnabled ? PlayerPrefs.GetFloat(SfxVolumeKey, 1f) : 0f);
            SetVolume(musicVolumeParameter, musicEnabled ? PlayerPrefs.GetFloat(MusicVolumeKey, 1f) : 0f);
        }
    }
}