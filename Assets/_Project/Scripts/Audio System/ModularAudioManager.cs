using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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
                return;

            if (!musicSources.TryGetValue(id, out var source) || source == null)
                return;

            if (source.isPlaying && !restart)
                return;

            source.clip = clipData.Clip;
            source.loop = clipData.Loop;
            source.volume = clipData.Volume;
            source.outputAudioMixerGroup = clipData.MixerGroup;
            source.pitch = 1f;
            source.Play();
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

        private void InitializeSources()
        {
            sfxSources.Clear();
            foreach (var clip in sfxClips)
            {
                if (clip == null)
                    continue;

                var source = CreateSource(sfxSourceRoot, clip.name, clip.MixerGroup, clip.Loop);
                sfxSources.Add(source);
            }

            musicSources.Clear();
            foreach (var clip in musicClips)
            {
                if (clip == null)
                    continue;

                var source = CreateSource(musicSourceRoot, clip.name, clip.MixerGroup, clip.Loop);
                if (!musicSources.ContainsKey(clip.Id))
                    musicSources.Add(clip.Id, source);
            }
        }

        private AudioSource CreateSource(Transform parent, string sourceName, AudioMixerGroup mixerGroup, bool loop)
        {
            var gameObject = new GameObject(sourceName);
            gameObject.transform.SetParent(parent, false);
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.outputAudioMixerGroup = mixerGroup;
            return source;
        }

        private AudioSource GetAvailableSfxSource()
        {
            foreach (var source in sfxSources)
            {
                if (source == null)
                    continue;

                if (!source.isPlaying)
                    return source;
            }

            if (sfxSourceRoot == null)
                return null;

            var extraSource = CreateSource(sfxSourceRoot, "Sfx_Extra", null, false);
            sfxSources.Add(extraSource);
            return extraSource;
        }

        private bool TryGetSfx(string id, out SfxClipDataSO clip)
        {
            clip = null;
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return sfxLookup.TryGetValue(id, out clip) && clip != null && clip.Clip != null;
        }

        private bool TryGetMusic(string id, out MusicClipDataSO clip)
        {
            clip = null;
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return musicLookup.TryGetValue(id, out clip) && clip != null && clip.Clip != null;
        }

        private void EnsureRoots()
        {
            if (sfxSourceRoot == null)
            {
                var root = new GameObject("SfxSources");
                root.transform.SetParent(transform, false);
                sfxSourceRoot = root.transform;
            }

            if (musicSourceRoot == null)
            {
                var root = new GameObject("MusicSources");
                root.transform.SetParent(transform, false);
                musicSourceRoot = root.transform;
            }
        }

        private void LoadSettings()
        {
            masterEnabled = PlayerPrefs.GetInt(MasterEnabledKey, 1) == 1;
            sfxEnabled = PlayerPrefs.GetInt(SfxEnabledKey, 1) == 1;
            musicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;

            var masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            var sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            var musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);

            SetVolume(masterVolumeParameter, masterVolume);
            SetVolume(sfxVolumeParameter, sfxVolume);
            SetVolume(musicVolumeParameter, musicVolume);
            ApplyMuteState();
        }

        private void SetVolume(string parameter, float volume01)
        {
            if (audioMixer == null || string.IsNullOrWhiteSpace(parameter))
                return;

            var clamped = Mathf.Clamp(volume01, 0.0001f, 1f);
            var db = Mathf.Log10(clamped) * 20f;
            audioMixer.SetFloat(parameter, db);
        }

        private void ApplyMuteState()
        {
            if (!masterEnabled)
            {
                SetVolume(masterVolumeParameter, 0.0001f);
                return;
            }

            SetVolume(masterVolumeParameter, PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
            SetVolume(sfxVolumeParameter, sfxEnabled ? PlayerPrefs.GetFloat(SfxVolumeKey, 1f) : 0.0001f);
            SetVolume(musicVolumeParameter, musicEnabled ? PlayerPrefs.GetFloat(MusicVolumeKey, 1f) : 0.0001f);
        }
    }
}
