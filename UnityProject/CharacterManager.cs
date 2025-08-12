using System;
using System.Collections.Generic;
using UnityEngine;

namespace SchoolLoveSimulator
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public int affectionLevel = 0;
        public int maxAffection = 100;
        public Sprite characterPortrait;
        public GameObject character3DModel;
        public string bio;
        public List<string> unlockedEvents = new List<string>();
        public CharacterPersonality personality;
        public bool isDateable = true;
        public int currentRoute = 0;
    }

    public enum CharacterPersonality
    {
        Tsundere,
        Kuudere,
        Dandere,
        Yandere,
        Genki,
        Ojousama,
        Tomboy,
        Intellectual
    }

    [System.Serializable]
    public class AffectionThreshold
    {
        public int requiredAffection;
        public string eventName;
        public string description;
        public bool isUnlocked = false;
    }

    public class CharacterManager : MonoBehaviour
    {
        [Header("Character Database")]
        public List<CharacterData> allCharacters = new List<CharacterData>();
        
        [Header("Affection System")]
        public List<AffectionThreshold> affectionEvents = new List<AffectionThreshold>();
        public int maxDailyInteractions = 3;
        private Dictionary<string, int> dailyInteractions = new Dictionary<string, int>();
        
        [Header("UI References")]
        public GameObject affectionPopupPrefab;
        public Transform popupContainer;
        
        [Header("Audio")]
        public AudioClip affectionUpSound;
        public AudioClip affectionDownSound;
        public AudioClip maxAffectionSound;
        private AudioSource audioSource;
        
        private SaveLoadSystem saveLoadSystem;
        private DialogueSystem dialogueSystem;
        private GameEventSystem eventSystem;
        
        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            InitializeCharacters();
        }
        
        void Start()
        {
            saveLoadSystem = FindObjectOfType<SaveLoadSystem>();
            dialogueSystem = FindObjectOfType<DialogueSystem>();
            eventSystem = FindObjectOfType<GameEventSystem>();
        }
        
        private void InitializeCharacters()
        {
            // デフォルトキャラクターの初期化
            if (allCharacters.Count == 0)
            {
                CreateDefaultCharacters();
            }
            
            foreach (var character in allCharacters)
            {
                dailyInteractions[character.characterName] = 0;
            }
        }
        
        private void CreateDefaultCharacters()
        {
            // サンプルキャラクター作成
            allCharacters.Add(new CharacterData
            {
                characterName = "桜井 美咲",
                affectionLevel = 0,
                maxAffection = 100,
                bio = "クラスの人気者。明るく元気な性格で、誰とでも仲良くなれる。",
                personality = CharacterPersonality.Genki,
                isDateable = true
            });
            
            allCharacters.Add(new CharacterData
            {
                characterName = "藤原 雪乃",
                affectionLevel = 0,
                maxAffection = 100,
                bio = "生徒会長。クールで知的だが、実は優しい一面も。",
                personality = CharacterPersonality.Kuudere,
                isDateable = true
            });
            
            allCharacters.Add(new CharacterData
            {
                characterName = "小林 あやめ",
                affectionLevel = 0,
                maxAffection = 100,
                bio = "図書委員。内気で本が好き。隠れた才能を持っている。",
                personality = CharacterPersonality.Dandere,
                isDateable = true
            });
            
            allCharacters.Add(new CharacterData
            {
                characterName = "山田 さくら",
                affectionLevel = 0,
                maxAffection = 100,
                bio = "運動部のエース。活発でボーイッシュな性格。",
                personality = CharacterPersonality.Tomboy,
                isDateable = true
            });
        }
        
        public void ChangeAffection(string characterName, int amount)
        {
            CharacterData character = GetCharacter(characterName);
            if (character == null) return;
            
            // 一日の交流制限チェック
            if (dailyInteractions.ContainsKey(characterName) && 
                dailyInteractions[characterName] >= maxDailyInteractions && 
                amount > 0)
            {
                ShowNotification($"{characterName}は今日はもう疲れているようだ...");
                return;
            }
            
            int oldAffection = character.affectionLevel;
            character.affectionLevel = Mathf.Clamp(character.affectionLevel + amount, 0, character.maxAffection);
            
            // 効果音再生
            if (amount > 0)
            {
                PlaySound(affectionUpSound);
                ShowAffectionChange(characterName, amount, true);
            }
            else if (amount < 0)
            {
                PlaySound(affectionDownSound);
                ShowAffectionChange(characterName, amount, false);
            }
            
            // 最大値到達チェック
            if (character.affectionLevel == character.maxAffection && oldAffection < character.maxAffection)
            {
                PlaySound(maxAffectionSound);
                UnlockSpecialEvent(characterName, "MaxAffection");
            }
            
            // イベント解放チェック
            CheckAffectionEvents(character);
            
            // 交流回数カウント
            if (amount > 0 && dailyInteractions.ContainsKey(characterName))
            {
                dailyInteractions[characterName]++;
            }
        }
        
        private void CheckAffectionEvents(CharacterData character)
        {
            foreach (var threshold in affectionEvents)
            {
                if (!threshold.isUnlocked && character.affectionLevel >= threshold.requiredAffection)
                {
                    threshold.isUnlocked = true;
                    character.unlockedEvents.Add(threshold.eventName);
                    
                    if (eventSystem != null)
                    {
                        eventSystem.TriggerEvent(threshold.eventName, character.characterName);
                    }
                    
                    ShowNotification($"新しいイベント解放: {threshold.description}");
                }
            }
        }
        
        public CharacterData GetCharacter(string name)
        {
            return allCharacters.Find(c => c.characterName == name);
        }
        
        public List<CharacterData> GetAllCharacters()
        {
            return new List<CharacterData>(allCharacters);
        }
        
        public List<CharacterData> GetDateableCharacters()
        {
            return allCharacters.FindAll(c => c.isDateable);
        }
        
        public int GetAffectionLevel(string characterName)
        {
            CharacterData character = GetCharacter(characterName);
            return character != null ? character.affectionLevel : 0;
        }
        
        public float GetAffectionPercentage(string characterName)
        {
            CharacterData character = GetCharacter(characterName);
            if (character == null) return 0f;
            
            return (float)character.affectionLevel / character.maxAffection * 100f;
        }
        
        public void ResetDailyInteractions()
        {
            foreach (var key in dailyInteractions.Keys)
            {
                dailyInteractions[key] = 0;
            }
        }
        
        public bool CanInteract(string characterName)
        {
            if (!dailyInteractions.ContainsKey(characterName)) return false;
            return dailyInteractions[characterName] < maxDailyInteractions;
        }
        
        private void ShowAffectionChange(string characterName, int amount, bool isPositive)
        {
            if (affectionPopupPrefab != null && popupContainer != null)
            {
                GameObject popup = Instantiate(affectionPopupPrefab, popupContainer);
                AffectionPopup popupScript = popup.GetComponent<AffectionPopup>();
                if (popupScript != null)
                {
                    popupScript.ShowAffectionChange(characterName, amount, isPositive);
                }
            }
        }
        
        private void ShowNotification(string message)
        {
            Debug.Log($"[Notification] {message}");
            // UI通知システムと連携
        }
        
        private void UnlockSpecialEvent(string characterName, string eventType)
        {
            Debug.Log($"[Special Event] {eventType} unlocked for {characterName}");
            // 特別イベントの処理
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        // セーブ/ロード用
        public CharacterSaveData GetSaveData()
        {
            CharacterSaveData saveData = new CharacterSaveData();
            saveData.characterDataList = new List<CharacterData>(allCharacters);
            saveData.dailyInteractions = new Dictionary<string, int>(dailyInteractions);
            saveData.unlockedThresholds = new List<string>();
            
            foreach (var threshold in affectionEvents)
            {
                if (threshold.isUnlocked)
                {
                    saveData.unlockedThresholds.Add(threshold.eventName);
                }
            }
            
            return saveData;
        }
        
        public void LoadSaveData(CharacterSaveData saveData)
        {
            if (saveData == null) return;
            
            allCharacters = new List<CharacterData>(saveData.characterDataList);
            dailyInteractions = new Dictionary<string, int>(saveData.dailyInteractions);
            
            foreach (var eventName in saveData.unlockedThresholds)
            {
                var threshold = affectionEvents.Find(t => t.eventName == eventName);
                if (threshold != null)
                {
                    threshold.isUnlocked = true;
                }
            }
        }
    }
    
    [System.Serializable]
    public class CharacterSaveData
    {
        public List<CharacterData> characterDataList;
        public Dictionary<string, int> dailyInteractions;
        public List<string> unlockedThresholds;
    }
}