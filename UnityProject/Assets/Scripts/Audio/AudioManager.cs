using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;

namespace SchoolLoveSimulator.Audio
{
    /// <summary>
    /// BGM/SE統合管理システム
    /// 3Dサウンド、クロスフェード、動的ミキシング対応
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        instance = go.AddComponent<AudioManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioMixerGroup bgmMixerGroup;
        [SerializeField] private AudioMixerGroup seMixerGroup;
        [SerializeField] private AudioMixerGroup voiceMixerGroup;
        [SerializeField] private AudioMixerGroup ambientMixerGroup;
        
        [Header("Audio Sources")]
        [SerializeField] private int bgmSourceCount = 2;  // クロスフェード用
        [SerializeField] private int seSourceCount = 10;  // 同時再生SE数
        [SerializeField] private int voiceSourceCount = 3;  // 同時再生ボイス数
        
        [Header("Audio Libraries")]
        [SerializeField] private AudioLibrary bgmLibrary;
        [SerializeField] private AudioLibrary seLibrary;
        [SerializeField] private AudioLibrary voiceLibrary;
        [SerializeField] private AudioLibrary ambientLibrary;
        
        [Header("Settings")]
        [SerializeField] private float defaultFadeTime = 1.5f;
        [SerializeField] private float masterVolume = 1.0f;
        [SerializeField] private float bgmVolume = 0.7f;
        [SerializeField] private float seVolume = 0.8f;
        [SerializeField] private float voiceVolume = 0.9f;
        [SerializeField] private float ambientVolume = 0.5f;
        
        // オーディオソースプール
        private List<AudioSource> bgmSources;
        private Queue<AudioSource> seSources;
        private Queue<AudioSource> voiceSources;
        private AudioSource ambientSource;
        
        // 現在の再生状態
        private int currentBGMIndex = 0;
        private string currentBGMName;
        private Dictionary<string, Coroutine> fadeCoroutines;
        private Dictionary<string, AudioSource> playingSounds;
        private List<ScheduledSound> scheduledSounds;
        
        [System.Serializable]
        public class AudioLibrary
        {
            public List<AudioData> audioClips = new List<AudioData>();
            private Dictionary<string, AudioData> clipDictionary;
            
            public void Initialize()
            {
                clipDictionary = new Dictionary<string, AudioData>();
                foreach (var clip in audioClips)
                {
                    if (clip.clip != null && !string.IsNullOrEmpty(clip.name))
                    {
                        clipDictionary[clip.name] = clip;
                    }
                }
            }
            
            public AudioData GetAudioData(string name)
            {
                if (clipDictionary == null)
                    Initialize();
                    
                return clipDictionary.ContainsKey(name) ? clipDictionary[name] : null;
            }
            
            public AudioClip GetClip(string name)
            {
                var data = GetAudioData(name);
                return data?.clip;
            }
        }
        
        [System.Serializable]
        public class AudioData
        {
            public string name;
            public AudioClip clip;
            public float volume = 1.0f;
            public float pitch = 1.0f;
            public bool loop = false;
            public bool is3D = false;
            public float minDistance = 1f;
            public float maxDistance = 10f;
            public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
            public string[] tags;  // カテゴリタグ
        }
        
        public class ScheduledSound
        {
            public string soundName;
            public float scheduledTime;
            public System.Action onComplete;
        }
        
        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAudioSources();
            InitializeLibraries();
            fadeCoroutines = new Dictionary<string, Coroutine>();
            playingSounds = new Dictionary<string, AudioSource>();
            scheduledSounds = new List<ScheduledSound>();
        }
        
        void InitializeAudioSources()
        {
            // BGMソース初期化（クロスフェード用に複数）
            bgmSources = new List<AudioSource>();
            for (int i = 0; i < bgmSourceCount; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = bgmMixerGroup;
                source.playOnAwake = false;
                bgmSources.Add(source);
            }
            
            // SEソースプール初期化
            seSources = new Queue<AudioSource>();
            for (int i = 0; i < seSourceCount; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = seMixerGroup;
                source.playOnAwake = false;
                seSources.Enqueue(source);
            }
            
            // ボイスソースプール初期化
            voiceSources = new Queue<AudioSource>();
            for (int i = 0; i < voiceSourceCount; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = voiceMixerGroup;
                source.playOnAwake = false;
                voiceSources.Enqueue(source);
            }
            
            // アンビエントソース
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.outputAudioMixerGroup = ambientMixerGroup;
            ambientSource.playOnAwake = false;
            ambientSource.loop = true;
        }
        
        void InitializeLibraries()
        {
            bgmLibrary?.Initialize();
            seLibrary?.Initialize();
            voiceLibrary?.Initialize();
            ambientLibrary?.Initialize();
        }
        
        #region BGM Control
        
        public void PlayBGM(string bgmName, float fadeTime = -1)
        {
            if (fadeTime < 0)
                fadeTime = defaultFadeTime;
                
            var audioData = bgmLibrary?.GetAudioData(bgmName);
            if (audioData == null)
            {
                Debug.LogWarning($"BGM not found: {bgmName}");
                return;
            }
            
            // 次のBGMソースを取得
            currentBGMIndex = (currentBGMIndex + 1) % bgmSources.Count;
            var newSource = bgmSources[currentBGMIndex];
            var oldSource = bgmSources[(currentBGMIndex + bgmSources.Count - 1) % bgmSources.Count];
            
            // クロスフェード実行
            if (fadeCoroutines.ContainsKey("bgm_fade"))
            {
                StopCoroutine(fadeCoroutines["bgm_fade"]);
            }
            
            fadeCoroutines["bgm_fade"] = StartCoroutine(
                CrossfadeBGM(oldSource, newSource, audioData, fadeTime)
            );
            
            currentBGMName = bgmName;
        }
        
        IEnumerator CrossfadeBGM(AudioSource oldSource, AudioSource newSource, AudioData audioData, float fadeTime)
        {
            // 新しいBGMを準備
            newSource.clip = audioData.clip;
            newSource.volume = 0;
            newSource.pitch = audioData.pitch;
            newSource.loop = audioData.loop;
            newSource.Play();
            
            float elapsed = 0;
            float startVolumeOld = oldSource.isPlaying ? oldSource.volume : 0;
            float targetVolumeNew = audioData.volume * bgmVolume;
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeTime;
                
                // イージング適用
                float easedT = EaseInOutQuad(t);
                
                if (oldSource.isPlaying)
                {
                    oldSource.volume = Mathf.Lerp(startVolumeOld, 0, easedT);
                }
                
                newSource.volume = Mathf.Lerp(0, targetVolumeNew, easedT);
                
                yield return null;
            }
            
            // 完了処理
            if (oldSource.isPlaying)
            {
                oldSource.Stop();
                oldSource.volume = 0;
            }
            
            newSource.volume = targetVolumeNew;
        }
        
        public void StopBGM(float fadeTime = -1)
        {
            if (fadeTime < 0)
                fadeTime = defaultFadeTime;
                
            foreach (var source in bgmSources)
            {
                if (source.isPlaying)
                {
                    StartCoroutine(FadeOut(source, fadeTime));
                }
            }
            
            currentBGMName = null;
        }
        
        public void PauseBGM()
        {
            foreach (var source in bgmSources)
            {
                if (source.isPlaying)
                {
                    source.Pause();
                }
            }
        }
        
        public void ResumeBGM()
        {
            foreach (var source in bgmSources)
            {
                source.UnPause();
            }
        }
        
        #endregion
        
        #region SE Control
        
        public void PlaySE(string seName, Vector3? position = null)
        {
            var audioData = seLibrary?.GetAudioData(seName);
            if (audioData == null)
            {
                Debug.LogWarning($"SE not found: {seName}");
                return;
            }
            
            if (seSources.Count == 0)
            {
                Debug.LogWarning("No available SE sources");
                return;
            }
            
            var source = seSources.Dequeue();
            
            // 3Dサウンド設定
            if (position.HasValue && audioData.is3D)
            {
                source.spatialBlend = 1.0f;  // 3D
                source.transform.position = position.Value;
                source.minDistance = audioData.minDistance;
                source.maxDistance = audioData.maxDistance;
                source.rolloffMode = audioData.rolloffMode;
            }
            else
            {
                source.spatialBlend = 0.0f;  // 2D
            }
            
            source.clip = audioData.clip;
            source.volume = audioData.volume * seVolume;
            source.pitch = audioData.pitch;
            source.loop = audioData.loop;
            
            source.Play();
            
            if (!audioData.loop)
            {
                StartCoroutine(ReturnSourceToPool(source, source.clip.length, seSources));
            }
            else
            {
                playingSounds[seName] = source;
            }
        }
        
        public void PlaySEAtPosition(string seName, Vector3 position)
        {
            PlaySE(seName, position);
        }
        
        public void PlayRandomSE(params string[] seNames)
        {
            if (seNames.Length == 0) return;
            
            int randomIndex = Random.Range(0, seNames.Length);
            PlaySE(seNames[randomIndex]);
        }
        
        public void StopSE(string seName)
        {
            if (playingSounds.ContainsKey(seName))
            {
                var source = playingSounds[seName];
                source.Stop();
                seSources.Enqueue(source);
                playingSounds.Remove(seName);
            }
        }
        
        public void StopAllSE()
        {
            foreach (var kvp in playingSounds.ToList())
            {
                StopSE(kvp.Key);
            }
        }
        
        #endregion
        
        #region Voice Control
        
        public void PlayVoice(string voiceName, string characterName = null)
        {
            var audioData = voiceLibrary?.GetAudioData(voiceName);
            if (audioData == null)
            {
                Debug.LogWarning($"Voice not found: {voiceName}");
                return;
            }
            
            if (voiceSources.Count == 0)
            {
                Debug.LogWarning("No available voice sources");
                return;
            }
            
            // キャラクター別のボイス管理
            if (!string.IsNullOrEmpty(characterName))
            {
                StopCharacterVoice(characterName);
            }
            
            var source = voiceSources.Dequeue();
            
            source.clip = audioData.clip;
            source.volume = audioData.volume * voiceVolume;
            source.pitch = audioData.pitch;
            source.spatialBlend = 0.0f;  // ボイスは通常2D
            
            source.Play();
            
            StartCoroutine(ReturnSourceToPool(source, source.clip.length, voiceSources));
            
            if (!string.IsNullOrEmpty(characterName))
            {
                playingSounds[$"voice_{characterName}"] = source;
            }
        }
        
        public void StopCharacterVoice(string characterName)
        {
            string key = $"voice_{characterName}";
            if (playingSounds.ContainsKey(key))
            {
                var source = playingSounds[key];
                source.Stop();
                voiceSources.Enqueue(source);
                playingSounds.Remove(key);
            }
        }
        
        #endregion
        
        #region Ambient Control
        
        public void PlayAmbient(string ambientName, float fadeTime = -1)
        {
            if (fadeTime < 0)
                fadeTime = defaultFadeTime;
                
            var audioData = ambientLibrary?.GetAudioData(ambientName);
            if (audioData == null)
            {
                Debug.LogWarning($"Ambient not found: {ambientName}");
                return;
            }
            
            ambientSource.clip = audioData.clip;
            ambientSource.volume = 0;
            ambientSource.pitch = audioData.pitch;
            ambientSource.loop = true;
            ambientSource.Play();
            
            StartCoroutine(FadeIn(ambientSource, audioData.volume * ambientVolume, fadeTime));
        }
        
        public void StopAmbient(float fadeTime = -1)
        {
            if (fadeTime < 0)
                fadeTime = defaultFadeTime;
                
            StartCoroutine(FadeOut(ambientSource, fadeTime));
        }
        
        #endregion
        
        #region Volume Control
        
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("MasterVolume", LinearToDecibel(masterVolume));
        }
        
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("BGMVolume", LinearToDecibel(bgmVolume));
        }
        
        public void SetSEVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("SEVolume", LinearToDecibel(seVolume));
        }
        
        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("VoiceVolume", LinearToDecibel(voiceVolume));
        }
        
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("AmbientVolume", LinearToDecibel(ambientVolume));
        }
        
        float LinearToDecibel(float linear)
        {
            float dB;
            if (linear != 0)
                dB = 20.0f * Mathf.Log10(linear);
            else
                dB = -144.0f;
                
            return dB;
        }
        
        #endregion
        
        #region Advanced Features
        
        public void PlaySequence(List<string> soundNames, float interval)
        {
            StartCoroutine(PlaySoundSequence(soundNames, interval));
        }
        
        IEnumerator PlaySoundSequence(List<string> soundNames, float interval)
        {
            foreach (var soundName in soundNames)
            {
                PlaySE(soundName);
                yield return new WaitForSeconds(interval);
            }
        }
        
        public void PlayLayeredBGM(string[] bgmNames, float[] volumes)
        {
            for (int i = 0; i < Mathf.Min(bgmNames.Length, bgmSources.Count); i++)
            {
                var audioData = bgmLibrary?.GetAudioData(bgmNames[i]);
                if (audioData != null)
                {
                    var source = bgmSources[i];
                    source.clip = audioData.clip;
                    source.volume = volumes[i] * bgmVolume;
                    source.loop = true;
                    source.Play();
                }
            }
        }
        
        public void SetLayerVolume(int layerIndex, float volume, float fadeTime = 0.5f)
        {
            if (layerIndex < bgmSources.Count)
            {
                StartCoroutine(FadeVolume(bgmSources[layerIndex], volume * bgmVolume, fadeTime));
            }
        }
        
        public void ApplyLowPassFilter(float cutoffFrequency)
        {
            mainMixer?.SetFloat("LowpassFreq", cutoffFrequency);
        }
        
        public void ApplyReverb(float reverbLevel)
        {
            mainMixer?.SetFloat("ReverbLevel", reverbLevel);
        }
        
        public void CreateAudioSnapshot(string snapshotName)
        {
            var snapshot = mainMixer?.FindSnapshot(snapshotName);
            snapshot?.TransitionTo(0.5f);
        }
        
        #endregion
        
        #region Utility Methods
        
        IEnumerator ReturnSourceToPool(AudioSource source, float delay, Queue<AudioSource> pool)
        {
            yield return new WaitForSeconds(delay);
            
            source.Stop();
            source.clip = null;
            source.spatialBlend = 0;
            pool.Enqueue(source);
        }
        
        IEnumerator FadeIn(AudioSource source, float targetVolume, float fadeTime)
        {
            float startVolume = source.volume;
            float elapsed = 0;
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeTime);
                yield return null;
            }
            
            source.volume = targetVolume;
        }
        
        IEnumerator FadeOut(AudioSource source, float fadeTime)
        {
            float startVolume = source.volume;
            float elapsed = 0;
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeTime);
                yield return null;
            }
            
            source.Stop();
            source.volume = 0;
        }
        
        IEnumerator FadeVolume(AudioSource source, float targetVolume, float fadeTime)
        {
            float startVolume = source.volume;
            float elapsed = 0;
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeTime);
                yield return null;
            }
            
            source.volume = targetVolume;
        }
        
        float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
        }
        
        #endregion
        
        #region Save/Load Settings
        
        public void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("SEVolume", seVolume);
            PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
            PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
            PlayerPrefs.Save();
        }
        
        public void LoadAudioSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
            seVolume = PlayerPrefs.GetFloat("SEVolume", 0.8f);
            voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 0.9f);
            ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.5f);
            
            SetMasterVolume(masterVolume);
            SetBGMVolume(bgmVolume);
            SetSEVolume(seVolume);
            SetVoiceVolume(voiceVolume);
            SetAmbientVolume(ambientVolume);
        }
        
        #endregion
        
        void OnDestroy()
        {
            SaveAudioSettings();
        }
    }
}