using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace SchoolLoveSimulator.Map
{
    /// <summary>
    /// 3Dマップ管理システム
    /// エリア遷移、キャラクター配置、時間経過を管理
    /// </summary>
    public class MapSystem : MonoBehaviour
    {
        private static MapSystem instance;
        public static MapSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MapSystem>();
                }
                return instance;
            }
        }
        
        [Header("Map Configuration")]
        [SerializeField] private List<MapArea> allAreas;
        [SerializeField] private MapArea currentArea;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float transitionDuration = 1.5f;
        
        [Header("Time System")]
        [SerializeField] private GameTimePhase currentTimePhase = GameTimePhase.Morning;
        [SerializeField] private int currentDay = 1;
        [SerializeField] private Season currentSeason = Season.Spring;
        
        [Header("Character Placement")]
        [SerializeField] private List<CharacterPlacement> characterPlacements;
        [SerializeField] private float characterSpawnRadius = 2f;
        
        [Header("Navigation")]
        [SerializeField] private NavMeshSurface[] navMeshSurfaces;
        [SerializeField] private bool useNavMesh = true;
        
        // イベント
        public delegate void AreaChangedDelegate(MapArea newArea);
        public event AreaChangedDelegate OnAreaChanged;
        
        public delegate void TimeChangedDelegate(GameTimePhase newPhase);
        public event TimeChangedDelegate OnTimeChanged;
        
        [System.Serializable]
        public class MapArea
        {
            public string areaID;
            public string areaName;
            public AreaType areaType;
            public GameObject areaPrefab;
            public Transform[] spawnPoints;
            public Transform[] interactionPoints;
            public Transform cameraPosition;
            public bool isUnlocked = true;
            public int requiredIntimacyLevel = 0;
            public List<string> connectedAreaIDs;
            public AudioClip ambientSound;
            public Light areaLighting;
            public Material skyboxMaterial;
            
            // エリア別イベント
            public List<AreaEvent> availableEvents;
            
            // エリア内のNPC
            public List<NPCData> npcs;
        }
        
        [System.Serializable]
        public class CharacterPlacement
        {
            public string characterID;
            public string preferredAreaID;
            public GameTimePhase[] activeTimePhases;
            public float movementSpeed = 3f;
            public GameObject characterPrefab;
            public CharacterBehavior behavior;
            
            [HideInInspector] public GameObject instanceObject;
            [HideInInspector] public NavMeshAgent navAgent;
        }
        
        [System.Serializable]
        public class AreaEvent
        {
            public string eventID;
            public string eventName;
            public GameTimePhase requiredTimePhase;
            public int minimumIntimacy;
            public bool isRepeatable;
            public bool hasBeenTriggered;
            public UnityEngine.Events.UnityEvent onEventTrigger;
        }
        
        [System.Serializable]
        public class NPCData
        {
            public string npcID;
            public string npcName;
            public GameObject npcPrefab;
            public Vector3 position;
            public NPCBehaviorType behaviorType;
            public string[] dialogueLines;
        }
        
        public enum AreaType
        {
            School,
            City,
            Private,
            Special
        }
        
        public enum GameTimePhase
        {
            Morning,        // 朝 (7:00-12:00)
            Afternoon,      // 昼 (12:00-17:00)
            Evening,        // 放課後/夕方 (17:00-20:00)
            Night          // 夜 (20:00-24:00)
        }
        
        public enum Season
        {
            Spring,
            Summer,
            Autumn,
            Winter
        }
        
        public enum CharacterBehavior
        {
            Static,         // 固定位置
            Patrol,         // 巡回
            Random,         // ランダム移動
            FollowPlayer,   // プレイヤー追従
            Schedule        // スケジュール通り
        }
        
        public enum NPCBehaviorType
        {
            Static,
            Walking,
            Interactive,
            Vendor
        }
        
        void Awake()
        {
            instance = this;
            InitializeMapSystem();
        }
        
        void Start()
        {
            LoadInitialArea();
            PlaceCharacters();
            UpdateTimeBasedElements();
        }
        
        void InitializeMapSystem()
        {
            // エリアデータの初期化
            if (allAreas == null)
            {
                allAreas = new List<MapArea>();
                CreateDefaultAreas();
            }
            
            // NavMesh設定
            if (useNavMesh)
            {
                navMeshSurfaces = FindObjectsOfType<NavMeshSurface>();
                foreach (var surface in navMeshSurfaces)
                {
                    surface.BuildNavMesh();
                }
            }
        }
        
        void CreateDefaultAreas()
        {
            // 学園エリア
            allAreas.Add(new MapArea
            {
                areaID = "school_classroom",
                areaName = "教室",
                areaType = AreaType.School,
                isUnlocked = true,
                requiredIntimacyLevel = 0
            });
            
            allAreas.Add(new MapArea
            {
                areaID = "school_library",
                areaName = "図書館",
                areaType = AreaType.School,
                isUnlocked = true,
                requiredIntimacyLevel = 0
            });
            
            allAreas.Add(new MapArea
            {
                areaID = "school_rooftop",
                areaName = "屋上",
                areaType = AreaType.School,
                isUnlocked = true,
                requiredIntimacyLevel = 0
            });
            
            // 街エリア
            allAreas.Add(new MapArea
            {
                areaID = "city_cafe",
                areaName = "カフェ",
                areaType = AreaType.City,
                isUnlocked = true,
                requiredIntimacyLevel = 0
            });
            
            allAreas.Add(new MapArea
            {
                areaID = "city_park",
                areaName = "公園",
                areaType = AreaType.City,
                isUnlocked = true,
                requiredIntimacyLevel = 0
            });
            
            allAreas.Add(new MapArea
            {
                areaID = "city_cinema",
                areaName = "映画館",
                areaType = AreaType.City,
                isUnlocked = true,
                requiredIntimacyLevel = 0
            });
            
            // プライベートエリア
            allAreas.Add(new MapArea
            {
                areaID = "private_player_room",
                areaName = "主人公の部屋",
                areaType = AreaType.Private,
                isUnlocked = true,
                requiredIntimacyLevel = 0
            });
            
            allAreas.Add(new MapArea
            {
                areaID = "private_heroine_room",
                areaName = "ヒロインの部屋",
                areaType = AreaType.Private,
                isUnlocked = false,
                requiredIntimacyLevel = 2
            });
            
            allAreas.Add(new MapArea
            {
                areaID = "private_hotel",
                areaName = "ホテル",
                areaType = AreaType.Private,
                isUnlocked = false,
                requiredIntimacyLevel = 3
            });
        }
        
        void LoadInitialArea()
        {
            // 初期エリアをロード
            string initialAreaID = "school_classroom";
            ChangeArea(initialAreaID);
        }
        
        public void ChangeArea(string areaID, Transform specificSpawnPoint = null)
        {
            StartCoroutine(ChangeAreaCoroutine(areaID, specificSpawnPoint));
        }
        
        System.Collections.IEnumerator ChangeAreaCoroutine(string areaID, Transform specificSpawnPoint)
        {
            // フェードアウト
            yield return StartCoroutine(FadeOut());
            
            // 現在のエリアをアンロード
            if (currentArea != null && currentArea.areaPrefab != null)
            {
                Destroy(currentArea.areaPrefab);
            }
            
            // 新しいエリアを検索
            MapArea newArea = allAreas.FirstOrDefault(a => a.areaID == areaID);
            if (newArea == null)
            {
                Debug.LogError($"Area not found: {areaID}");
                yield break;
            }
            
            // アクセス権限チェック
            if (!CanAccessArea(newArea))
            {
                Debug.Log($"Cannot access area: {newArea.areaName}. Required intimacy: {newArea.requiredIntimacyLevel}");
                yield break;
            }
            
            // 新しいエリアをロード
            currentArea = newArea;
            if (newArea.areaPrefab != null)
            {
                GameObject areaInstance = Instantiate(newArea.areaPrefab);
                newArea.areaPrefab = areaInstance;
            }
            
            // プレイヤーを配置
            if (specificSpawnPoint != null)
            {
                playerTransform.position = specificSpawnPoint.position;
            }
            else if (newArea.spawnPoints != null && newArea.spawnPoints.Length > 0)
            {
                playerTransform.position = newArea.spawnPoints[0].position;
            }
            
            // カメラ位置調整
            if (newArea.cameraPosition != null)
            {
                mainCamera.transform.position = newArea.cameraPosition.position;
                mainCamera.transform.rotation = newArea.cameraPosition.rotation;
            }
            
            // 環境設定
            SetupAreaEnvironment(newArea);
            
            // キャラクター再配置
            ReplaceCharactersInArea(newArea);
            
            // NPC配置
            SpawnNPCs(newArea);
            
            // NavMesh再構築
            if (useNavMesh)
            {
                yield return RebuildNavMesh();
            }
            
            // フェードイン
            yield return StartCoroutine(FadeIn());
            
            // イベント発火
            OnAreaChanged?.Invoke(newArea);
        }
        
        bool CanAccessArea(MapArea area)
        {
            if (!area.isUnlocked)
                return false;
                
            // 親密度チェック（現在選択中のヒロインとの親密度）
            // IntimacySystem.Instance.GetCurrentIntimacyLevel() >= area.requiredIntimacyLevel
            
            return true;
        }
        
        void SetupAreaEnvironment(MapArea area)
        {
            // アンビエントサウンド
            if (area.ambientSound != null)
            {
                // AudioManager.Instance.PlayAmbient(area.ambientSound.name);
            }
            
            // ライティング
            if (area.areaLighting != null)
            {
                RenderSettings.ambientLight = area.areaLighting.color;
                RenderSettings.ambientIntensity = area.areaLighting.intensity;
            }
            
            // スカイボックス
            if (area.skyboxMaterial != null)
            {
                RenderSettings.skybox = area.skyboxMaterial;
            }
            
            // 時間帯による調整
            AdjustLightingForTimePhase();
        }
        
        void PlaceCharacters()
        {
            foreach (var placement in characterPlacements)
            {
                // 時間帯チェック
                if (!placement.activeTimePhases.Contains(currentTimePhase))
                    continue;
                    
                // エリアチェック
                if (placement.preferredAreaID != currentArea.areaID)
                    continue;
                    
                // キャラクター生成
                if (placement.characterPrefab != null)
                {
                    Vector3 spawnPos = GetRandomSpawnPosition();
                    placement.instanceObject = Instantiate(
                        placement.characterPrefab,
                        spawnPos,
                        Quaternion.identity
                    );
                    
                    // NavMeshAgent設定
                    if (useNavMesh)
                    {
                        placement.navAgent = placement.instanceObject.GetComponent<NavMeshAgent>();
                        if (placement.navAgent == null)
                        {
                            placement.navAgent = placement.instanceObject.AddComponent<NavMeshAgent>();
                        }
                        placement.navAgent.speed = placement.movementSpeed;
                    }
                    
                    // 行動パターン開始
                    StartCharacterBehavior(placement);
                }
            }
        }
        
        void ReplaceCharactersInArea(MapArea newArea)
        {
            // 既存キャラクターを削除
            foreach (var placement in characterPlacements)
            {
                if (placement.instanceObject != null)
                {
                    Destroy(placement.instanceObject);
                }
            }
            
            // 新エリアに再配置
            PlaceCharacters();
        }
        
        void SpawnNPCs(MapArea area)
        {
            if (area.npcs == null) return;
            
            foreach (var npc in area.npcs)
            {
                if (npc.npcPrefab != null)
                {
                    GameObject npcInstance = Instantiate(
                        npc.npcPrefab,
                        npc.position,
                        Quaternion.identity
                    );
                    
                    // NPC行動設定
                    NPCController controller = npcInstance.GetComponent<NPCController>();
                    if (controller == null)
                    {
                        controller = npcInstance.AddComponent<NPCController>();
                    }
                    controller.Initialize(npc);
                }
            }
        }
        
        Vector3 GetRandomSpawnPosition()
        {
            if (currentArea.spawnPoints != null && currentArea.spawnPoints.Length > 0)
            {
                Transform spawnPoint = currentArea.spawnPoints[Random.Range(0, currentArea.spawnPoints.Length)];
                Vector3 randomOffset = Random.insideUnitSphere * characterSpawnRadius;
                randomOffset.y = 0;
                return spawnPoint.position + randomOffset;
            }
            
            return Vector3.zero;
        }
        
        void StartCharacterBehavior(CharacterPlacement placement)
        {
            switch (placement.behavior)
            {
                case CharacterBehavior.Patrol:
                    StartCoroutine(PatrolBehavior(placement));
                    break;
                case CharacterBehavior.Random:
                    StartCoroutine(RandomMovementBehavior(placement));
                    break;
                case CharacterBehavior.FollowPlayer:
                    StartCoroutine(FollowPlayerBehavior(placement));
                    break;
                case CharacterBehavior.Schedule:
                    StartCoroutine(ScheduleBehavior(placement));
                    break;
            }
        }
        
        System.Collections.IEnumerator PatrolBehavior(CharacterPlacement placement)
        {
            if (currentArea.interactionPoints == null || currentArea.interactionPoints.Length < 2)
                yield break;
                
            int currentPointIndex = 0;
            
            while (placement.instanceObject != null)
            {
                Transform targetPoint = currentArea.interactionPoints[currentPointIndex];
                
                if (placement.navAgent != null)
                {
                    placement.navAgent.SetDestination(targetPoint.position);
                    
                    // 目的地到達待機
                    while (placement.navAgent.remainingDistance > 0.5f)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                
                // 次のポイントへ
                currentPointIndex = (currentPointIndex + 1) % currentArea.interactionPoints.Length;
                
                // 少し待機
                yield return new WaitForSeconds(Random.Range(2f, 5f));
            }
        }
        
        System.Collections.IEnumerator RandomMovementBehavior(CharacterPlacement placement)
        {
            while (placement.instanceObject != null)
            {
                Vector3 randomDestination = GetRandomSpawnPosition();
                
                if (placement.navAgent != null)
                {
                    placement.navAgent.SetDestination(randomDestination);
                }
                
                yield return new WaitForSeconds(Random.Range(5f, 10f));
            }
        }
        
        System.Collections.IEnumerator FollowPlayerBehavior(CharacterPlacement placement)
        {
            float followDistance = 3f;
            
            while (placement.instanceObject != null && playerTransform != null)
            {
                float distance = Vector3.Distance(
                    placement.instanceObject.transform.position,
                    playerTransform.position
                );
                
                if (distance > followDistance && placement.navAgent != null)
                {
                    placement.navAgent.SetDestination(playerTransform.position);
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        System.Collections.IEnumerator ScheduleBehavior(CharacterPlacement placement)
        {
            // 時間帯に応じた行動
            while (placement.instanceObject != null)
            {
                switch (currentTimePhase)
                {
                    case GameTimePhase.Morning:
                        // 朝の行動
                        break;
                    case GameTimePhase.Afternoon:
                        // 昼の行動
                        break;
                    case GameTimePhase.Evening:
                        // 夕方の行動
                        break;
                    case GameTimePhase.Night:
                        // 夜の行動
                        break;
                }
                
                yield return new WaitForSeconds(60f); // 1分ごとに更新
            }
        }
        
        public void AdvanceTime()
        {
            // 時間を進める
            int phaseIndex = (int)currentTimePhase;
            phaseIndex++;
            
            if (phaseIndex > (int)GameTimePhase.Night)
            {
                phaseIndex = 0;
                currentDay++;
                
                // 週末判定
                if (currentDay % 7 == 0 || currentDay % 7 == 6)
                {
                    // 週末イベント
                }
            }
            
            currentTimePhase = (GameTimePhase)phaseIndex;
            UpdateTimeBasedElements();
            
            OnTimeChanged?.Invoke(currentTimePhase);
        }
        
        void UpdateTimeBasedElements()
        {
            AdjustLightingForTimePhase();
            UpdateCharacterPlacements();
            CheckTimeBasedEvents();
        }
        
        void AdjustLightingForTimePhase()
        {
            Color ambientColor = Color.white;
            float intensity = 1f;
            
            switch (currentTimePhase)
            {
                case GameTimePhase.Morning:
                    ambientColor = new Color(1f, 0.95f, 0.8f);
                    intensity = 0.8f;
                    break;
                case GameTimePhase.Afternoon:
                    ambientColor = new Color(1f, 1f, 0.95f);
                    intensity = 1f;
                    break;
                case GameTimePhase.Evening:
                    ambientColor = new Color(1f, 0.7f, 0.5f);
                    intensity = 0.6f;
                    break;
                case GameTimePhase.Night:
                    ambientColor = new Color(0.4f, 0.5f, 0.7f);
                    intensity = 0.3f;
                    break;
            }
            
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = intensity;
        }
        
        void UpdateCharacterPlacements()
        {
            // 時間帯に応じてキャラクターを再配置
            ReplaceCharactersInArea(currentArea);
        }
        
        void CheckTimeBasedEvents()
        {
            if (currentArea.availableEvents == null) return;
            
            foreach (var areaEvent in currentArea.availableEvents)
            {
                if (areaEvent.requiredTimePhase == currentTimePhase &&
                    !areaEvent.hasBeenTriggered)
                {
                    // イベント発火条件チェック
                    areaEvent.onEventTrigger?.Invoke();
                    
                    if (!areaEvent.isRepeatable)
                    {
                        areaEvent.hasBeenTriggered = true;
                    }
                }
            }
        }
        
        System.Collections.IEnumerator RebuildNavMesh()
        {
            if (!useNavMesh) yield break;
            
            foreach (var surface in navMeshSurfaces)
            {
                surface.BuildNavMesh();
            }
            
            yield return null;
        }
        
        System.Collections.IEnumerator FadeOut()
        {
            // フェードアウトエフェクト
            float elapsed = 0;
            while (elapsed < transitionDuration / 2)
            {
                elapsed += Time.deltaTime;
                // UI fade logic
                yield return null;
            }
        }
        
        System.Collections.IEnumerator FadeIn()
        {
            // フェードインエフェクト
            float elapsed = 0;
            while (elapsed < transitionDuration / 2)
            {
                elapsed += Time.deltaTime;
                // UI fade logic
                yield return null;
            }
        }
        
        // Public API
        public MapArea GetCurrentArea() => currentArea;
        public GameTimePhase GetCurrentTimePhase() => currentTimePhase;
        public int GetCurrentDay() => currentDay;
        public Season GetCurrentSeason() => currentSeason;
        
        public List<MapArea> GetAccessibleAreas()
        {
            return allAreas.Where(a => CanAccessArea(a)).ToList();
        }
        
        public void UnlockArea(string areaID)
        {
            var area = allAreas.FirstOrDefault(a => a.areaID == areaID);
            if (area != null)
            {
                area.isUnlocked = true;
            }
        }
    }
    
    // NPC制御用の補助クラス
    public class NPCController : MonoBehaviour
    {
        private MapSystem.NPCData npcData;
        private NavMeshAgent navAgent;
        
        public void Initialize(MapSystem.NPCData data)
        {
            npcData = data;
            navAgent = GetComponent<NavMeshAgent>();
            
            StartBehavior();
        }
        
        void StartBehavior()
        {
            switch (npcData.behaviorType)
            {
                case MapSystem.NPCBehaviorType.Walking:
                    StartCoroutine(WalkingBehavior());
                    break;
                case MapSystem.NPCBehaviorType.Interactive:
                    // インタラクション設定
                    break;
            }
        }
        
        System.Collections.IEnumerator WalkingBehavior()
        {
            while (true)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 10f;
                randomDirection += transform.position;
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
                {
                    navAgent?.SetDestination(hit.position);
                }
                
                yield return new WaitForSeconds(Random.Range(5f, 15f));
            }
        }
    }
}