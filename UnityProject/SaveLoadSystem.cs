using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SchoolLoveSimulator
{
    [System.Serializable]
    public class GameSaveData
    {
        public string saveName;
        public DateTime saveDate;
        public int saveSlot;
        
        // プレイヤーデータ
        public string playerName;
        public int currentDay;
        public int currentPeriod;
        public float playTime;
        
        // ゲーム進行データ
        public string currentScene;
        public Vector3 playerPosition;
        public Dictionary<string, bool> gameFlags;
        public List<string> completedEvents;
        
        // キャラクターデータ
        public CharacterSaveData characterData;
        
        // インベントリデータ
        public List<string> inventory;
        public int money;
        
        // 統計データ
        public int totalChoicesMade;
        public Dictionary<string, int> routeProgress;
        
        // スクリーンショット
        public byte[] screenshot;
    }
    
    public class SaveLoadSystem : MonoBehaviour
    {
        [Header("Save Settings")]
        public int maxSaveSlots = 20;
        public string saveFilePrefix = "SchoolLove_Save_";
        public bool autoSaveEnabled = true;
        public float autoSaveInterval = 300f; // 5分ごと
        
        [Header("UI References")]
        public GameObject saveLoadMenuPrefab;
        public Transform uiContainer;
        
        private string saveDirectory;
        private float timeSinceLastAutoSave;
        private float totalPlayTime;
        private GameSaveData currentGameData;
        
        // 関連システムの参照
        private CharacterManager characterManager;
        private DialogueSystem dialogueSystem;
        private GameEventSystem eventSystem;
        
        void Awake()
        {
            InitializeSaveDirectory();
            LoadGameSettings();
        }
        
        void Start()
        {
            characterManager = FindObjectOfType<CharacterManager>();
            dialogueSystem = FindObjectOfType<DialogueSystem>();
            eventSystem = FindObjectOfType<GameEventSystem>();
            
            if (autoSaveEnabled)
            {
                InvokeRepeating(nameof(AutoSave), autoSaveInterval, autoSaveInterval);
            }
        }
        
        void Update()
        {
            totalPlayTime += Time.deltaTime;
            timeSinceLastAutoSave += Time.deltaTime;
        }
        
        private void InitializeSaveDirectory()
        {
            saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
        }
        
        public void SaveGame(int slot, string saveName = "")
        {
            try
            {
                GameSaveData saveData = CreateSaveData(slot, saveName);
                string json = JsonUtility.ToJson(saveData, true);
                string filePath = GetSaveFilePath(slot);
                
                File.WriteAllText(filePath, json);
                
                // スクリーンショット保存
                SaveScreenshot(slot);
                
                ShowSaveNotification($"セーブ完了: スロット {slot}");
                Debug.Log($"Game saved to slot {slot}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
                ShowSaveNotification("セーブに失敗しました", true);
            }
        }
        
        public bool LoadGame(int slot)
        {
            try
            {
                string filePath = GetSaveFilePath(slot);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"Save file not found for slot {slot}");
                    return false;
                }
                
                string json = File.ReadAllText(filePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                
                ApplySaveData(saveData);
                
                ShowSaveNotification($"ロード完了: スロット {slot}");
                Debug.Log($"Game loaded from slot {slot}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                ShowSaveNotification("ロードに失敗しました", true);
                return false;
            }
        }
        
        private GameSaveData CreateSaveData(int slot, string saveName)
        {
            GameSaveData saveData = new GameSaveData();
            
            saveData.saveSlot = slot;
            saveData.saveName = string.IsNullOrEmpty(saveName) ? $"Save {slot}" : saveName;
            saveData.saveDate = DateTime.Now;
            saveData.playTime = totalPlayTime;
            
            // プレイヤーデータ
            saveData.playerName = GetPlayerName();
            saveData.currentDay = GetCurrentDay();
            saveData.currentPeriod = GetCurrentPeriod();
            
            // シーン情報
            saveData.currentScene = SceneManager.GetActiveScene().name;
            saveData.playerPosition = GetPlayerPosition();
            
            // ゲームフラグ
            if (dialogueSystem != null)
            {
                saveData.gameFlags = dialogueSystem.GetAllFlags();
            }
            
            // キャラクターデータ
            if (characterManager != null)
            {
                saveData.characterData = characterManager.GetSaveData();
            }
            
            // イベントデータ
            if (eventSystem != null)
            {
                saveData.completedEvents = eventSystem.GetCompletedEvents();
            }
            
            // インベントリ
            saveData.inventory = GetInventory();
            saveData.money = GetMoney();
            
            // 統計
            saveData.totalChoicesMade = GetTotalChoicesMade();
            saveData.routeProgress = GetRouteProgress();
            
            return saveData;
        }
        
        private void ApplySaveData(GameSaveData saveData)
        {
            // プレイヤーデータ復元
            SetPlayerName(saveData.playerName);
            SetCurrentDay(saveData.currentDay);
            SetCurrentPeriod(saveData.currentPeriod);
            totalPlayTime = saveData.playTime;
            
            // シーン読み込み
            if (SceneManager.GetActiveScene().name != saveData.currentScene)
            {
                SceneManager.LoadScene(saveData.currentScene);
            }
            
            // プレイヤー位置復元
            SetPlayerPosition(saveData.playerPosition);
            
            // ゲームフラグ復元
            if (dialogueSystem != null && saveData.gameFlags != null)
            {
                dialogueSystem.LoadFlags(saveData.gameFlags);
            }
            
            // キャラクターデータ復元
            if (characterManager != null && saveData.characterData != null)
            {
                characterManager.LoadSaveData(saveData.characterData);
            }
            
            // イベントデータ復元
            if (eventSystem != null && saveData.completedEvents != null)
            {
                eventSystem.LoadCompletedEvents(saveData.completedEvents);
            }
            
            // インベントリ復元
            SetInventory(saveData.inventory);
            SetMoney(saveData.money);
            
            // 統計復元
            SetTotalChoicesMade(saveData.totalChoicesMade);
            SetRouteProgress(saveData.routeProgress);
        }
        
        public void DeleteSave(int slot)
        {
            try
            {
                string filePath = GetSaveFilePath(slot);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
                string screenshotPath = GetScreenshotPath(slot);
                if (File.Exists(screenshotPath))
                {
                    File.Delete(screenshotPath);
                }
                
                ShowSaveNotification($"セーブデータを削除しました: スロット {slot}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Delete save failed: {e.Message}");
            }
        }
        
        public List<GameSaveData> GetAllSaves()
        {
            List<GameSaveData> saves = new List<GameSaveData>();
            
            for (int i = 0; i < maxSaveSlots; i++)
            {
                string filePath = GetSaveFilePath(i);
                if (File.Exists(filePath))
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                        saves.Add(saveData);
                    }
                    catch
                    {
                        // 破損したセーブファイルはスキップ
                        continue;
                    }
                }
            }
            
            return saves;
        }
        
        public bool SaveExists(int slot)
        {
            return File.Exists(GetSaveFilePath(slot));
        }
        
        private void AutoSave()
        {
            if (!autoSaveEnabled) return;
            
            SaveGame(0, "オートセーブ");
            timeSinceLastAutoSave = 0f;
        }
        
        public void QuickSave()
        {
            SaveGame(GetQuickSaveSlot(), "クイックセーブ");
        }
        
        public void QuickLoad()
        {
            int quickSaveSlot = GetQuickSaveSlot();
            if (SaveExists(quickSaveSlot))
            {
                LoadGame(quickSaveSlot);
            }
            else
            {
                ShowSaveNotification("クイックセーブが見つかりません", true);
            }
        }
        
        private int GetQuickSaveSlot()
        {
            return maxSaveSlots - 1; // 最後のスロットをクイックセーブ用に予約
        }
        
        private string GetSaveFilePath(int slot)
        {
            return Path.Combine(saveDirectory, $"{saveFilePrefix}{slot}.json");
        }
        
        private string GetScreenshotPath(int slot)
        {
            return Path.Combine(saveDirectory, $"{saveFilePrefix}{slot}_screenshot.png");
        }
        
        private void SaveScreenshot(int slot)
        {
            StartCoroutine(CaptureScreenshot(slot));
        }
        
        private System.Collections.IEnumerator CaptureScreenshot(int slot)
        {
            yield return new WaitForEndOfFrame();
            
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();
            
            // リサイズ（サムネイル用）
            Texture2D thumbnail = ResizeTexture(screenshot, 320, 180);
            
            byte[] bytes = thumbnail.EncodeToPNG();
            string path = GetScreenshotPath(slot);
            File.WriteAllBytes(path, bytes);
            
            Destroy(screenshot);
            Destroy(thumbnail);
        }
        
        private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;
            
            Texture2D result = new Texture2D(targetWidth, targetHeight);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();
            
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            
            return result;
        }
        
        private void ShowSaveNotification(string message, bool isError = false)
        {
            Debug.Log($"[SaveLoad] {message}");
            // UI通知システムと連携
        }
        
        private void LoadGameSettings()
        {
            // ゲーム設定の読み込み
            string settingsPath = Path.Combine(Application.persistentDataPath, "settings.json");
            if (File.Exists(settingsPath))
            {
                // 設定ファイルから自動セーブ設定などを読み込む
            }
        }
        
        // プレースホルダーメソッド群（実際のゲームシステムと接続）
        private string GetPlayerName() => PlayerPrefs.GetString("PlayerName", "プレイヤー");
        private void SetPlayerName(string name) => PlayerPrefs.SetString("PlayerName", name);
        
        private int GetCurrentDay() => PlayerPrefs.GetInt("CurrentDay", 1);
        private void SetCurrentDay(int day) => PlayerPrefs.SetInt("CurrentDay", day);
        
        private int GetCurrentPeriod() => PlayerPrefs.GetInt("CurrentPeriod", 0);
        private void SetCurrentPeriod(int period) => PlayerPrefs.SetInt("CurrentPeriod", period);
        
        private Vector3 GetPlayerPosition()
        {
            GameObject player = GameObject.FindWithTag("Player");
            return player != null ? player.transform.position : Vector3.zero;
        }
        
        private void SetPlayerPosition(Vector3 position)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) player.transform.position = position;
        }
        
        private List<string> GetInventory() => new List<string>();
        private void SetInventory(List<string> inventory) { }
        
        private int GetMoney() => PlayerPrefs.GetInt("Money", 1000);
        private void SetMoney(int money) => PlayerPrefs.SetInt("Money", money);
        
        private int GetTotalChoicesMade() => PlayerPrefs.GetInt("TotalChoices", 0);
        private void SetTotalChoicesMade(int choices) => PlayerPrefs.SetInt("TotalChoices", choices);
        
        private Dictionary<string, int> GetRouteProgress() => new Dictionary<string, int>();
        private void SetRouteProgress(Dictionary<string, int> progress) { }
    }
}