using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SchoolLoveSimulator.Progress
{
    /// <summary>
    /// 10時間のゲーム進行を管理
    /// ストーリー進行、イベント管理、エンディング分岐
    /// </summary>
    public class GameProgressManager : MonoBehaviour
    {
        private static GameProgressManager instance;
        public static GameProgressManager Instance => instance;
        
        [Header("Progress Tracking")]
        [SerializeField] private float totalPlayTime = 0f;
        [SerializeField] private int currentChapter = 1;
        [SerializeField] private float chapterProgress = 0f;
        
        [Header("Story Structure")]
        [SerializeField] private List<Chapter> storyChapters;
        [SerializeField] private Chapter currentChapterData;
        
        [Header("Endings")]
        [SerializeField] private List<EndingCondition> endingConditions;
        [SerializeField] private string achievedEnding = "";
        
        [System.Serializable]
        public class Chapter
        {
            public int chapterNumber;
            public string chapterTitle;
            public float estimatedDuration; // 時間
            public List<StoryEvent> mainEvents;
            public List<StoryEvent> sideEvents;
            public bool isCompleted;
        }
        
        [System.Serializable]
        public class StoryEvent
        {
            public string eventID;
            public string eventName;
            public float triggerTime;
            public EventType eventType;
            public string requiredHeroine;
            public int requiredIntimacy;
            public bool hasOccurred;
            public UnityEngine.Events.UnityEvent onEventTrigger;
        }
        
        [System.Serializable]
        public class EndingCondition
        {
            public string endingID;
            public string endingName;
            public EndingType endingType;
            public string targetHeroine;
            public int requiredIntimacy;
            public List<string> requiredEvents;
            public bool isUnlocked;
        }
        
        public enum EventType
        {
            Story,      // メインストーリー
            Date,       // デートイベント
            Confession, // 告白イベント
            Intimate,   // 親密イベント
            Special     // 特別イベント
        }
        
        public enum EndingType
        {
            Normal,     // ノーマルエンド
            Good,       // グッドエンド
            True,       // トゥルーエンド
            Bad,        // バッドエンド
            Harem       // ハーレムエンド
        }
        
        void Awake()
        {
            instance = this;
            InitializeStoryStructure();
        }
        
        void Start()
        {
            LoadProgress();
            StartChapter(1);
        }
        
        void InitializeStoryStructure()
        {
            storyChapters = new List<Chapter>
            {
                // 序盤（0-3時間）
                new Chapter
                {
                    chapterNumber = 1,
                    chapterTitle = "出会い",
                    estimatedDuration = 1.5f,
                    mainEvents = new List<StoryEvent>
                    {
                        new StoryEvent
                        {
                            eventID = "meet_ayame",
                            eventName = "あやめとの出会い",
                            triggerTime = 0.1f,
                            eventType = EventType.Story
                        },
                        new StoryEvent
                        {
                            eventID = "meet_misaki",
                            eventName = "みさきとの出会い",
                            triggerTime = 0.3f,
                            eventType = EventType.Story
                        },
                        new StoryEvent
                        {
                            eventID = "meet_yukino",
                            eventName = "ゆきのとの出会い",
                            triggerTime = 0.5f,
                            eventType = EventType.Story
                        }
                    }
                },
                
                // 中盤（3-7時間）
                new Chapter
                {
                    chapterNumber = 2,
                    chapterTitle = "親密な関係へ",
                    estimatedDuration = 2f,
                    mainEvents = new List<StoryEvent>
                    {
                        new StoryEvent
                        {
                            eventID = "first_date",
                            eventName = "初デート",
                            triggerTime = 0.2f,
                            eventType = EventType.Date,
                            requiredIntimacy = 50
                        },
                        new StoryEvent
                        {
                            eventID = "confession",
                            eventName = "告白イベント",
                            triggerTime = 1.5f,
                            eventType = EventType.Confession,
                            requiredIntimacy = 100
                        }
                    }
                },
                
                new Chapter
                {
                    chapterNumber = 3,
                    chapterTitle = "深まる絆",
                    estimatedDuration = 2f,
                    mainEvents = new List<StoryEvent>
                    {
                        new StoryEvent
                        {
                            eventID = "first_kiss",
                            eventName = "ファーストキス",
                            triggerTime = 0.5f,
                            eventType = EventType.Story,
                            requiredIntimacy = 150
                        },
                        new StoryEvent
                        {
                            eventID = "lovers_date",
                            eventName = "恋人デート",
                            triggerTime = 1f,
                            eventType = EventType.Date,
                            requiredIntimacy = 200
                        }
                    }
                },
                
                // 終盤（7-10時間）
                new Chapter
                {
                    chapterNumber = 4,
                    chapterTitle = "結ばれる二人",
                    estimatedDuration = 1.5f,
                    mainEvents = new List<StoryEvent>
                    {
                        new StoryEvent
                        {
                            eventID = "intimate_night",
                            eventName = "初めての夜",
                            triggerTime = 0.5f,
                            eventType = EventType.Intimate,
                            requiredIntimacy = 250
                        }
                    }
                },
                
                new Chapter
                {
                    chapterNumber = 5,
                    chapterTitle = "永遠の愛",
                    estimatedDuration = 1.5f,
                    mainEvents = new List<StoryEvent>
                    {
                        new StoryEvent
                        {
                            eventID = "proposal",
                            eventName = "プロポーズ",
                            triggerTime = 1f,
                            eventType = EventType.Special,
                            requiredIntimacy = 400
                        }
                    }
                }
            };
            
            // エンディング条件
            endingConditions = new List<EndingCondition>
            {
                new EndingCondition
                {
                    endingID = "true_ayame",
                    endingName = "あやめトゥルーエンド",
                    endingType = EndingType.True,
                    targetHeroine = "ayame",
                    requiredIntimacy = 500,
                    requiredEvents = new List<string> { "confession", "intimate_night", "proposal" }
                },
                new EndingCondition
                {
                    endingID = "good_ayame",
                    endingName = "あやめグッドエンド",
                    endingType = EndingType.Good,
                    targetHeroine = "ayame",
                    requiredIntimacy = 300,
                    requiredEvents = new List<string> { "confession", "intimate_night" }
                },
                new EndingCondition
                {
                    endingID = "harem",
                    endingName = "ハーレムエンド",
                    endingType = EndingType.Harem,
                    requiredIntimacy = 200, // 全員で
                    requiredEvents = new List<string> { "confession" }
                }
            };
        }
        
        public void StartChapter(int chapterNumber)
        {
            currentChapterData = storyChapters.FirstOrDefault(c => c.chapterNumber == chapterNumber);
            if (currentChapterData != null)
            {
                currentChapter = chapterNumber;
                chapterProgress = 0f;
                Debug.Log($"Chapter {chapterNumber} started: {currentChapterData.chapterTitle}");
            }
        }
        
        void Update()
        {
            // プレイ時間追跡
            totalPlayTime += Time.deltaTime / 3600f; // 時間単位
            chapterProgress += Time.deltaTime / 3600f;
            
            // イベントチェック
            CheckStoryEvents();
            
            // チャプター進行チェック
            if (currentChapterData != null && chapterProgress >= currentChapterData.estimatedDuration)
            {
                CompleteChapter();
            }
            
            // エンディングチェック
            if (totalPlayTime >= 9f) // 9時間経過後
            {
                CheckEndingConditions();
            }
        }
        
        void CheckStoryEvents()
        {
            if (currentChapterData == null) return;
            
            foreach (var storyEvent in currentChapterData.mainEvents)
            {
                if (!storyEvent.hasOccurred && chapterProgress >= storyEvent.triggerTime)
                {
                    if (CheckEventRequirements(storyEvent))
                    {
                        TriggerStoryEvent(storyEvent);
                    }
                }
            }
        }
        
        bool CheckEventRequirements(StoryEvent storyEvent)
        {
            // 親密度チェック
            if (storyEvent.requiredIntimacy > 0)
            {
                // IntimacySystem.Instance.GetCurrentIntimacy() >= storyEvent.requiredIntimacy
            }
            
            // ヒロイン指定チェック
            if (!string.IsNullOrEmpty(storyEvent.requiredHeroine))
            {
                // 現在のヒロインチェック
            }
            
            return true;
        }
        
        void TriggerStoryEvent(StoryEvent storyEvent)
        {
            Debug.Log($"Story Event: {storyEvent.eventName}");
            storyEvent.hasOccurred = true;
            storyEvent.onEventTrigger?.Invoke();
            
            // イベントタイプ別処理
            switch (storyEvent.eventType)
            {
                case EventType.Date:
                    // DateEventSystem.Instance.StartDate()
                    break;
                case EventType.Confession:
                    // 告白シーン開始
                    break;
                case EventType.Intimate:
                    // AdultContentController.Instance.StartIntimateScene()
                    break;
            }
        }
        
        void CompleteChapter()
        {
            if (currentChapterData != null)
            {
                currentChapterData.isCompleted = true;
                Debug.Log($"Chapter {currentChapter} completed!");
                
                // 次のチャプターへ
                if (currentChapter < storyChapters.Count)
                {
                    StartChapter(currentChapter + 1);
                }
                else
                {
                    // 全チャプター完了
                    CheckEndingConditions();
                }
            }
        }
        
        void CheckEndingConditions()
        {
            foreach (var ending in endingConditions)
            {
                if (CheckEndingRequirements(ending))
                {
                    TriggerEnding(ending);
                    break;
                }
            }
            
            // どのエンディング条件も満たさない場合
            if (string.IsNullOrEmpty(achievedEnding))
            {
                TriggerDefaultEnding();
            }
        }
        
        bool CheckEndingRequirements(EndingCondition ending)
        {
            // 親密度チェック
            // IntimacySystem.Instance.GetIntimacy(ending.targetHeroine) >= ending.requiredIntimacy
            
            // 必須イベントチェック
            foreach (var eventID in ending.requiredEvents)
            {
                bool eventOccurred = storyChapters
                    .SelectMany(c => c.mainEvents)
                    .Any(e => e.eventID == eventID && e.hasOccurred);
                    
                if (!eventOccurred) return false;
            }
            
            return true;
        }
        
        void TriggerEnding(EndingCondition ending)
        {
            achievedEnding = ending.endingID;
            ending.isUnlocked = true;
            
            Debug.Log($"Ending achieved: {ending.endingName}");
            
            // エンディングシーン遷移
            // UnityEngine.SceneManagement.SceneManager.LoadScene($"Ending_{ending.endingID}");
        }
        
        void TriggerDefaultEnding()
        {
            achievedEnding = "normal";
            Debug.Log("Normal ending achieved");
        }
        
        public void SaveProgress()
        {
            PlayerPrefs.SetFloat("TotalPlayTime", totalPlayTime);
            PlayerPrefs.SetInt("CurrentChapter", currentChapter);
            PlayerPrefs.SetFloat("ChapterProgress", chapterProgress);
            
            // イベント進行状況保存
            foreach (var chapter in storyChapters)
            {
                foreach (var evt in chapter.mainEvents)
                {
                    PlayerPrefs.SetInt($"Event_{evt.eventID}", evt.hasOccurred ? 1 : 0);
                }
            }
            
            PlayerPrefs.Save();
        }
        
        public void LoadProgress()
        {
            totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0);
            currentChapter = PlayerPrefs.GetInt("CurrentChapter", 1);
            chapterProgress = PlayerPrefs.GetFloat("ChapterProgress", 0);
            
            // イベント進行状況読込
            foreach (var chapter in storyChapters)
            {
                foreach (var evt in chapter.mainEvents)
                {
                    evt.hasOccurred = PlayerPrefs.GetInt($"Event_{evt.eventID}", 0) == 1;
                }
            }
        }
        
        // API
        public float GetTotalPlayTime() => totalPlayTime;
        public int GetCurrentChapter() => currentChapter;
        public float GetGameProgress() => totalPlayTime / 10f; // 10時間想定
        public List<StoryEvent> GetAvailableEvents() => currentChapterData?.mainEvents.Where(e => !e.hasOccurred).ToList();
    }
}