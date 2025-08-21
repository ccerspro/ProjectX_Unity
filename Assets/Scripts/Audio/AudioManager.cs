using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ZhouSoftware
{
    [AddComponentMenu("ProjectX/Audio/Audio Manager"), DisallowMultipleComponent, DefaultExecutionOrder(-200)]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        [Header("Mixer Groups")]
        public AudioMixerGroup masterGroup;
        public AudioMixerGroup musicGroup;
        public AudioMixerGroup sfxGroup;
        public AudioMixerGroup uiGroup;
        public AudioMixerGroup ambienceGroup;

        [Header("Banks (drag ScriptableObjects)")]
        public AudioBank sfxBank;
        public AudioBank musicBank;
        public AudioBank ambienceBank;
        public AudioBank uiBank;

        [Header("SFX Pool")]
        public AudioSource sfxSourcePrefab;         // simple AudioSource prefab routed to SFX group
        [Range(0,64)] public int hardCap = 32;      // max sources at peak (pool grows as needed)

        // ---- runtime objects (not exposed in Inspector) ----
        AudioSource musicA, musicB, uiSrc, ambA, ambB;
        bool musicUseA = true, ambUseA = true;

        readonly Queue<AudioSource> free = new();
        readonly HashSet<AudioSource> busy = new();

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this; DontDestroyOnLoad(gameObject);

            // Build hidden sources we need (1 UI, 2 music, 2 ambience)
            musicA = BuildChild("Music_A", musicGroup);
            musicB = BuildChild("Music_B", musicGroup);
            ambA   = BuildChild("Ambience_A", ambienceGroup);
            ambB   = BuildChild("Ambience_B", ambienceGroup);
            uiSrc  = BuildChild("UI", uiGroup);

            foreach (var s in new[] { musicA, musicB, ambA, ambB, uiSrc })
            {
                if (!s) continue;
                s.playOnAwake = false; s.loop = false; s.volume = 1f;
            }
            ambA.loop = ambB.loop = true;
        }

        AudioSource BuildChild(string name, AudioMixerGroup g)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = g;
            return src;
        }

        // -------------------- PUBLIC API --------------------
        public void PlaySFX(string id, Vector3? worldPos = null, float volume = 1f, float pitch = 1f, bool spatial = false)
        {
            var clip = sfxBank ? sfxBank.Get(id) : null;
            if (!clip) return;

            if (!TryGetSfx(out var src)) return;

            src.clip = clip;
            src.volume = Mathf.Clamp01(volume);
            src.pitch = Mathf.Clamp(pitch, 0.3f, 3f);
            src.spatialBlend = spatial ? 1f : 0f;
            if (spatial && worldPos.HasValue) src.transform.position = worldPos.Value;

            src.Play();
            StartCoroutine(ReturnWhenDone(src));
        }

        public void StopSFX(string id)
        {
            var clip = sfxBank ? sfxBank.Get(id) : null;
            if (!clip) return;

            foreach (var src in busy)
            {
                if (src.clip == clip && src.isPlaying && src)
                {
                    src.Stop();
                    StartCoroutine(ReturnWhenDone(src));
                }
            }
        }

        public void PlayUI(string id, float volume = 1f)
        {
            var clip = uiBank ? uiBank.Get(id) : null;
            if (clip && uiSrc)
                uiSrc.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public void PlayMusic(string id, float fadeTime = 1.2f)
        {
            var clip = musicBank ? musicBank.Get(id) : null;
            if (!clip) return;

            var from = musicUseA ? musicA : musicB;
            var to   = musicUseA ? musicB : musicA;
            musicUseA = !musicUseA;

            to.clip = clip; to.loop = true; to.volume = 0f; to.Play();
            StartCoroutine(Crossfade(from, to, 1f, fadeTime)); // target vol = 1 (controlled by mixer)
        }

        public void SetAmbience(string id, float fadeTime = 1.2f)
        {
            var clip = ambienceBank ? ambienceBank.Get(id) : null;
            if (!clip) return;

            var from = ambUseA ? ambA : ambB;
            var to   = ambUseA ? ambB : ambA;
            ambUseA = !ambUseA;

            to.clip = clip; to.loop = true; to.volume = 0f; to.Play();
            StartCoroutine(Crossfade(from, to, 1f, fadeTime));
        }

        public void SetVolume(string exposedParam, float linear01)
        {
            if (!masterGroup) return;
            float db = linear01 > 0.0001f ? Mathf.Log10(Mathf.Clamp01(linear01)) * 20f : -80f;
            masterGroup.audioMixer.SetFloat(exposedParam, db);
        }

        public void Muffle(bool on, float duration = 0.6f, float muffledCutoff = 1000f)
        {
            if (!masterGroup) return;
            StartCoroutine(AnimateFloat(masterGroup.audioMixer, "LowPassCutoff",
                on ? muffledCutoff : 22000f, duration));
        }

        public bool IsSFXPlaying(string id)
        {
            var clip = sfxBank ? sfxBank.Get(id) : null;
            if (!clip) return false;

            foreach (var src in busy)
            {
                if (src.clip == clip && src.isPlaying && src) return true;
            }
            return false;
        }

        // -------------------- internals --------------------
        IEnumerator Crossfade(AudioSource from, AudioSource to, float targetVol, float dur)
        {
            float fromStart = from && from.isPlaying ? from.volume : 0f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                if (from) from.volume = Mathf.Lerp(fromStart, 0f, k);
                if (to)   to.volume   = Mathf.Lerp(0f, targetVol, k);
                yield return null;
            }
            if (from) { from.Stop(); from.volume = 0f; }
            if (to)   to.volume = targetVol;
        }

        bool TryGetSfx(out AudioSource src)
        {
            // reuse if available, otherwise create until hardCap
            if (free.Count > 0) { src = free.Dequeue(); busy.Add(src); return true; }
            if (busy.Count + free.Count < hardCap && sfxSourcePrefab)
            {
                var s = Instantiate(sfxSourcePrefab, transform);
                s.outputAudioMixerGroup = sfxGroup; s.playOnAwake = false; s.loop = false;
                src = s; busy.Add(src); return true;
            }
            src = null; return false;
        }

        IEnumerator ReturnWhenDone(AudioSource s)
        {
            yield return new WaitUntil(() => !s || !s.isPlaying);
            if (!s) yield break;
            busy.Remove(s);
            free.Enqueue(s);
        }

        static IEnumerator AnimateFloat(AudioMixer mixer, string param, float target, float dur)
        {
            if (!mixer.GetFloat(param, out float start)) start = target;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                mixer.SetFloat(param, Mathf.Lerp(start, target, t / dur));
                yield return null;
            }
            mixer.SetFloat(param, target);
        }
    }
}
