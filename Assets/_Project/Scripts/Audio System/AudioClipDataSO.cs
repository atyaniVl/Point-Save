using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    public abstract class AudioClipDataSO : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private bool loop;
        [SerializeField] private AudioMixerGroup mixerGroup;

        public string Id => id;
        public AudioClip Clip => clip;
        public float Volume => volume;
        public bool Loop => loop;
        public AudioMixerGroup MixerGroup => mixerGroup;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id) || id != name)
                id = name;
        }
    }
}
