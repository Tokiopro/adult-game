using System;
using System.IO;
using UnityEngine;

namespace SchoolLoveSimulator.Config
{
    /// <summary>
    /// 外部設定ファイル読み込みシステム
    /// EXE再ビルドなしで設定変更可能
    /// </summary>
    public class ConfigLoader : MonoBehaviour
    {
        private static ConfigLoader instance;
        public static ConfigLoader Instance => instance;
        
        [Header("Config Files")]
        private string configPath;
        private GameConfig gameConfig;
        private ContentConfig contentConfig;
        
        [System.Serializable]
        public class GameConfig
        {
            public string version;
            public string buildDate;
            public GameSettings gameSettings;
            public ContentSettings contentSettings;
            public AudioSettings audioSettings;
            public DebugSettings debugSettings;
        }
        
        [System.Serializable]
        public class GameSettings
        {
            public string[] resolutions;
            public string defaultResolution;
            public bool fullscreen;
            public bool vsync;
            public int targetFrameRate;
        }
        
        [System.Serializable]
        public class ContentSettings
        {
            public bool enableAdultContent;
            public bool mosaicEnabled;
            public int mosaicSize;
            public bool ageVerificationRequired;
        }
        
        [System.Serializable]
        public class AudioSettings
        {
            public float masterVolume;
            public float bgmVolume;
            public float seVolume;
            public float voiceVolume;
        }
        
        [System.Serializable]
        public class DebugSettings
        {
            public bool enableDebugMode;
            public bool showFPS;
            public bool enableCheats;
        }
        
        [System.Serializable]
        public class ContentConfig
        {
            public HeroineData[] heroines;
            public StageSettings stages;
            public UnlockedContent unlockedContent;
        }
        
        [System.Serializable]
        public class HeroineData
        {
            public string id;
            public string name;
            public int age;
            public bool enabled;
        }
        
        [System.Serializable]
        public class StageSettings
        {
            public int maxIntimacyLevel;
            public int level1Threshold;
            public int level2Threshold;
            public int level3Threshold;
        }
        
        [System.Serializable]
        public class UnlockedContent
        {
            public string[] positions;
            public string[] actions;
            public string[] locations;
        }
        
        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeConfigPath();
            LoadConfigurations();
        }
        
        void InitializeConfigPath()
        {
            // StreamingAssets内の設定ファイルを参照
#if UNITY_EDITOR
            configPath = Path.Combine(Application.dataPath, "StreamingAssets");
#else
            configPath = Path.Combine(Application.dataPath, "StreamingAssets");
#endif
            
            // 設定フォルダが存在しない場合は作成
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
                CreateDefaultConfigs();
            }
        }
        
        public void LoadConfigurations()
        {
            LoadGameConfig();
            LoadContentConfig();
            ApplySettings();
        }
        
        void LoadGameConfig()
        {
            string gameConfigPath = Path.Combine(configPath, "GameConfig.json");
            
            if (File.Exists(gameConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(gameConfigPath);
                    gameConfig = JsonUtility.FromJson<GameConfig>(json);
                    Debug.Log($"Game Config loaded: v{gameConfig.version}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load GameConfig: {e.Message}");
                    CreateDefaultGameConfig();
                }
            }
            else
            {
                CreateDefaultGameConfig();
            }
        }
        
        void LoadContentConfig()
        {
            string contentConfigPath = Path.Combine(configPath, "ContentConfig.json");
            
            if (File.Exists(contentConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(contentConfigPath);
                    contentConfig = JsonUtility.FromJson<ContentConfig>(json);
                    Debug.Log($"Content Config loaded: {contentConfig.heroines.Length} heroines");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load ContentConfig: {e.Message}");
                    CreateDefaultContentConfig();
                }
            }
            else
            {
                CreateDefaultContentConfig();
            }
        }
        
        void ApplySettings()
        {
            if (gameConfig == null) return;
            
            // ゲーム設定適用
            ApplyResolution();
            ApplyGraphicsSettings();
            ApplyAudioSettings();
            ApplyContentSettings();
            
            // デバッグ設定
            if (gameConfig.debugSettings.enableDebugMode)
            {
                EnableDebugMode();
            }
        }
        
        void ApplyResolution()
        {
            if (gameConfig.gameSettings.defaultResolution != null)
            {
                string[] parts = gameConfig.gameSettings.defaultResolution.Split('x');
                if (parts.Length == 2)
                {
                    int width = int.Parse(parts[0]);
                    int height = int.Parse(parts[1]);
                    Screen.SetResolution(width, height, gameConfig.gameSettings.fullscreen);
                }
            }
            
            QualitySettings.vSyncCount = gameConfig.gameSettings.vsync ? 1 : 0;
            Application.targetFrameRate = gameConfig.gameSettings.targetFrameRate;
        }
        
        void ApplyGraphicsSettings()
        {
            // グラフィック設定
            QualitySettings.SetQualityLevel(5); // Ultra
        }
        
        void ApplyAudioSettings()
        {
            // オーディオ設定
            AudioListener.volume = gameConfig.audioSettings.masterVolume;
            
            // AudioManagerに設定を渡す
            // AudioManager.Instance?.SetBGMVolume(gameConfig.audioSettings.bgmVolume);
            // AudioManager.Instance?.SetSEVolume(gameConfig.audioSettings.seVolume);
            // AudioManager.Instance?.SetVoiceVolume(gameConfig.audioSettings.voiceVolume);
        }
        
        void ApplyContentSettings()
        {
            // コンテンツ設定
            if (gameConfig.contentSettings.mosaicEnabled)
            {
                GameObject mosaicObj = GameObject.Find("MosaicRenderer");
                if (mosaicObj == null)
                {
                    mosaicObj = new GameObject("MosaicRenderer");
                    var mosaicRenderer = mosaicObj.AddComponent<MosaicRenderer>();
                    // mosaicRenderer.SetMosaicSize(gameConfig.contentSettings.mosaicSize);
                }
            }
        }
        
        void EnableDebugMode()
        {
            // デバッグモード有効化
            GameObject debugObj = new GameObject("DebugManager");
            debugObj.AddComponent<DebugManager>();
        }
        
        void CreateDefaultConfigs()
        {
            CreateDefaultGameConfig();
            CreateDefaultContentConfig();
        }
        
        void CreateDefaultGameConfig()
        {
            gameConfig = new GameConfig
            {
                version = "1.0.0",
                buildDate = DateTime.Now.ToString("yyyy-MM-dd"),
                gameSettings = new GameSettings
                {
                    resolutions = new[] { "1920x1080", "1280x720" },
                    defaultResolution = "1920x1080",
                    fullscreen = false,
                    vsync = true,
                    targetFrameRate = 60
                },
                contentSettings = new ContentSettings
                {
                    enableAdultContent = true,
                    mosaicEnabled = true,
                    mosaicSize = 16,
                    ageVerificationRequired = true
                },
                audioSettings = new AudioSettings
                {
                    masterVolume = 0.8f,
                    bgmVolume = 0.7f,
                    seVolume = 0.8f,
                    voiceVolume = 0.9f
                },
                debugSettings = new DebugSettings
                {
                    enableDebugMode = false,
                    showFPS = false,
                    enableCheats = false
                }
            };
            
            SaveGameConfig();
        }
        
        void CreateDefaultContentConfig()
        {
            contentConfig = new ContentConfig
            {
                heroines = new[]
                {
                    new HeroineData { id = "ayame", name = "綾女", age = 18, enabled = true },
                    new HeroineData { id = "misaki", name = "美咲", age = 19, enabled = true },
                    new HeroineData { id = "yukino", name = "雪乃", age = 20, enabled = true }
                },
                stages = new StageSettings
                {
                    maxIntimacyLevel = 500,
                    level1Threshold = 100,
                    level2Threshold = 250,
                    level3Threshold = 500
                },
                unlockedContent = new UnlockedContent
                {
                    positions = new[] { "missionary" },
                    actions = new[] { "hold_hands", "hug" },
                    locations = new[] { "school", "cafe", "park" }
                }
            };
            
            SaveContentConfig();
        }
        
        public void SaveGameConfig()
        {
            string path = Path.Combine(configPath, "GameConfig.json");
            string json = JsonUtility.ToJson(gameConfig, true);
            File.WriteAllText(path, json);
        }
        
        public void SaveContentConfig()
        {
            string path = Path.Combine(configPath, "ContentConfig.json");
            string json = JsonUtility.ToJson(contentConfig, true);
            File.WriteAllText(path, json);
        }
        
        // 外部からアクセス可能なAPI
        public GameConfig GetGameConfig() => gameConfig;
        public ContentConfig GetContentConfig() => contentConfig;
        
        public void ReloadConfigs()
        {
            LoadConfigurations();
        }
        
        public bool IsAdultContentEnabled()
        {
            return gameConfig?.contentSettings?.enableAdultContent ?? false;
        }
        
        public int GetMosaicSize()
        {
            return gameConfig?.contentSettings?.mosaicSize ?? 16;
        }
        
        public HeroineData[] GetHeroines()
        {
            return contentConfig?.heroines ?? new HeroineData[0];
        }
    }
    
    // デバッグマネージャー
    public class DebugManager : MonoBehaviour
    {
        private bool showFPS = false;
        private float deltaTime = 0.0f;
        
        void Start()
        {
            var config = ConfigLoader.Instance?.GetGameConfig();
            if (config != null)
            {
                showFPS = config.debugSettings.showFPS;
            }
        }
        
        void Update()
        {
            if (showFPS)
            {
                deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            }
            
            // デバッグコマンド
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showFPS = !showFPS;
            }
        }
        
        void OnGUI()
        {
            if (showFPS)
            {
                int fps = Mathf.RoundToInt(1.0f / deltaTime);
                GUI.Label(new Rect(10, 10, 100, 20), $"FPS: {fps}");
            }
        }
    }
}