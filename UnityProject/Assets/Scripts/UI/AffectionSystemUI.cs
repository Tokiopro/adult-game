using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace SchoolLoveSimulator.UI
{
    /// <summary>
    /// 好感度システムUI管理
    /// キャラクターごとの好感度表示とアニメーション
    /// </summary>
    public class AffectionSystemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject affectionPanelPrefab;
        [SerializeField] private Transform affectionPanelContainer;
        [SerializeField] private GameObject heartEffectPrefab;
        [SerializeField] private Canvas uiCanvas;
        
        [Header("Character Affection Display")]
        [SerializeField] private GameObject characterDetailPanel;
        [SerializeField] private Image characterPortrait;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private Slider affectionSlider;
        [SerializeField] private TextMeshProUGUI affectionLevelText;
        [SerializeField] private TextMeshProUGUI affectionPointsText;
        [SerializeField] private Image relationshipIcon;
        
        [Header("Affection Bar Settings")]
        [SerializeField] private Gradient affectionGradient;
        [SerializeField] private AnimationCurve fillAnimationCurve;
        [SerializeField] private float fillAnimationDuration = 0.5f;
        
        [Header("Heart Icons")]
        [SerializeField] private Sprite[] heartSprites;
        [SerializeField] private GameObject floatingHeartPrefab;
        [SerializeField] private Transform heartSpawnPoint;
        
        [Header("Relationship Status")]
        [SerializeField] private Sprite[] relationshipSprites;
        [SerializeField] private string[] relationshipNames = {
            "Stranger", "Acquaintance", "Friend", "Close Friend", 
            "Love Interest", "Girlfriend", "Lover", "Soulmate"
        };
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem levelUpEffect;
        [SerializeField] private ParticleSystem maxLevelEffect;
        [SerializeField] private AudioClip levelUpSound;
        [SerializeField] private AudioClip heartbeatSound;
        
        // 内部データ
        private Dictionary<string, CharacterAffectionData> characterAffectionData;
        private CharacterAffectionData currentCharacter;
        private List<AffectionPanel> activePanels;
        private Coroutine currentAnimationCoroutine;
        
        [System.Serializable]
        public class CharacterAffectionData
        {
            public string characterName;
            public Sprite portrait;
            public float currentAffection;
            public float maxAffection = 100f;
            public int affectionLevel;
            public RelationshipStatus relationshipStatus;
            public List<AffectionEvent> affectionHistory;
            public bool isLocked;
            public List<string> unlockedScenes;
            
            public CharacterAffectionData(string name)
            {
                characterName = name;
                currentAffection = 0;
                affectionLevel = 0;
                relationshipStatus = RelationshipStatus.Stranger;
                affectionHistory = new List<AffectionEvent>();
                unlockedScenes = new List<string>();
                isLocked = false;
            }
        }
        
        [System.Serializable]
        public class AffectionEvent
        {
            public float amount;
            public string reason;
            public float timestamp;
            
            public AffectionEvent(float amt, string rsn)
            {
                amount = amt;
                reason = rsn;
                timestamp = Time.time;
            }
        }
        
        public enum RelationshipStatus
        {
            Stranger = 0,
            Acquaintance = 1,
            Friend = 2,
            CloseFriend = 3,
            LoveInterest = 4,
            Girlfriend = 5,
            Lover = 6,
            Soulmate = 7
        }
        
        public class AffectionPanel
        {
            public GameObject panelObject;
            public Image portrait;
            public Slider affectionBar;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI levelText;
            public Image[] heartIcons;
            public CharacterAffectionData data;
        }
        
        void Awake()
        {
            characterAffectionData = new Dictionary<string, CharacterAffectionData>();
            activePanels = new List<AffectionPanel>();
            InitializeGradient();
        }
        
        void Start()
        {
            // テスト用キャラクター初期化
            InitializeCharacters();
            UpdateUI();
        }
        
        void InitializeGradient()
        {
            if (affectionGradient == null)
            {
                affectionGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[5];
                colorKeys[0] = new GradientColorKey(Color.gray, 0.0f);
                colorKeys[1] = new GradientColorKey(Color.white, 0.25f);
                colorKeys[2] = new GradientColorKey(new Color(1f, 0.7f, 0.7f), 0.5f);
                colorKeys[3] = new GradientColorKey(new Color(1f, 0.4f, 0.4f), 0.75f);
                colorKeys[4] = new GradientColorKey(Color.red, 1.0f);
                
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
                alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
                
                affectionGradient.SetKeys(colorKeys, alphaKeys);
            }
        }
        
        void InitializeCharacters()
        {
            // テスト用キャラクターデータ
            string[] characterNames = { "Ayame", "Misaki", "Yukino" };
            
            foreach (string name in characterNames)
            {
                var data = new CharacterAffectionData(name);
                data.currentAffection = Random.Range(0, 50);
                data.affectionLevel = Mathf.FloorToInt(data.currentAffection / 10);
                characterAffectionData[name] = data;
                
                CreateAffectionPanel(data);
            }
            
            // 最初のキャラクターを選択
            if (characterAffectionData.Count > 0)
            {
                SelectCharacter(characterNames[0]);
            }
        }
        
        void CreateAffectionPanel(CharacterAffectionData data)
        {
            if (affectionPanelPrefab == null || affectionPanelContainer == null)
                return;
                
            GameObject panelObj = Instantiate(affectionPanelPrefab, affectionPanelContainer);
            AffectionPanel panel = new AffectionPanel
            {
                panelObject = panelObj,
                data = data
            };
            
            // UIコンポーネント取得
            panel.portrait = panelObj.transform.Find("Portrait")?.GetComponent<Image>();
            panel.affectionBar = panelObj.transform.Find("AffectionBar")?.GetComponent<Slider>();
            panel.nameText = panelObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            panel.levelText = panelObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            
            // ハートアイコン取得
            Transform heartsContainer = panelObj.transform.Find("Hearts");
            if (heartsContainer != null)
            {
                panel.heartIcons = heartsContainer.GetComponentsInChildren<Image>();
            }
            
            // クリックイベント設定
            Button btn = panelObj.GetComponent<Button>();
            if (btn != null)
            {
                string charName = data.characterName;
                btn.onClick.AddListener(() => SelectCharacter(charName));
            }
            
            activePanels.Add(panel);
            UpdatePanelDisplay(panel);
        }
        
        void UpdatePanelDisplay(AffectionPanel panel)
        {
            if (panel == null || panel.data == null) return;
            
            // 名前とレベル
            if (panel.nameText != null)
                panel.nameText.text = panel.data.characterName;
                
            if (panel.levelText != null)
                panel.levelText.text = $"Lv.{panel.data.affectionLevel}";
                
            // 好感度バー
            if (panel.affectionBar != null)
            {
                float fillAmount = panel.data.currentAffection / panel.data.maxAffection;
                panel.affectionBar.value = fillAmount;
                
                // バーの色を好感度に応じて変更
                Image fillImage = panel.affectionBar.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = affectionGradient.Evaluate(fillAmount);
                }
            }
            
            // ハートアイコン更新
            UpdateHeartIcons(panel);
        }
        
        void UpdateHeartIcons(AffectionPanel panel)
        {
            if (panel.heartIcons == null) return;
            
            int filledHearts = Mathf.FloorToInt(panel.data.currentAffection / 20f);
            
            for (int i = 0; i < panel.heartIcons.Length; i++)
            {
                if (i < filledHearts)
                {
                    panel.heartIcons[i].sprite = heartSprites?[1]; // Filled heart
                    panel.heartIcons[i].color = Color.red;
                }
                else
                {
                    panel.heartIcons[i].sprite = heartSprites?[0]; // Empty heart
                    panel.heartIcons[i].color = Color.gray;
                }
            }
        }
        
        public void SelectCharacter(string characterName)
        {
            if (!characterAffectionData.ContainsKey(characterName))
                return;
                
            currentCharacter = characterAffectionData[characterName];
            UpdateDetailPanel();
            
            // パネルハイライト
            foreach (var panel in activePanels)
            {
                bool isSelected = panel.data.characterName == characterName;
                
                // 選択状態の視覚的フィードバック
                Outline outline = panel.panelObject.GetComponent<Outline>();
                if (outline == null)
                    outline = panel.panelObject.AddComponent<Outline>();
                    
                outline.enabled = isSelected;
                outline.effectColor = Color.yellow;
                outline.effectDistance = new Vector2(2, 2);
            }
        }
        
        void UpdateDetailPanel()
        {
            if (currentCharacter == null || characterDetailPanel == null)
                return;
                
            characterDetailPanel.SetActive(true);
            
            // キャラクター情報更新
            if (characterNameText != null)
                characterNameText.text = currentCharacter.characterName;
                
            if (characterPortrait != null && currentCharacter.portrait != null)
                characterPortrait.sprite = currentCharacter.portrait;
                
            // 好感度スライダー
            if (affectionSlider != null)
            {
                float targetValue = currentCharacter.currentAffection / currentCharacter.maxAffection;
                
                if (currentAnimationCoroutine != null)
                    StopCoroutine(currentAnimationCoroutine);
                    
                currentAnimationCoroutine = StartCoroutine(
                    AnimateSlider(affectionSlider, targetValue)
                );
            }
            
            // テキスト更新
            if (affectionLevelText != null)
                affectionLevelText.text = $"Level {currentCharacter.affectionLevel}";
                
            if (affectionPointsText != null)
                affectionPointsText.text = $"{currentCharacter.currentAffection:F0}/{currentCharacter.maxAffection:F0}";
                
            // 関係性アイコン
            UpdateRelationshipDisplay();
        }
        
        void UpdateRelationshipDisplay()
        {
            if (currentCharacter == null) return;
            
            // 関係性アイコン更新
            if (relationshipIcon != null && relationshipSprites != null)
            {
                int statusIndex = (int)currentCharacter.relationshipStatus;
                if (statusIndex < relationshipSprites.Length)
                {
                    relationshipIcon.sprite = relationshipSprites[statusIndex];
                }
            }
            
            // 関係性テキスト表示
            GameObject relationshipTextObj = characterDetailPanel?.transform.Find("RelationshipText")?.gameObject;
            if (relationshipTextObj != null)
            {
                TextMeshProUGUI relationshipText = relationshipTextObj.GetComponent<TextMeshProUGUI>();
                if (relationshipText != null)
                {
                    int statusIndex = (int)currentCharacter.relationshipStatus;
                    if (statusIndex < relationshipNames.Length)
                    {
                        relationshipText.text = relationshipNames[statusIndex];
                    }
                }
            }
        }
        
        public void AddAffection(string characterName, float amount, string reason = "")
        {
            if (!characterAffectionData.ContainsKey(characterName))
                return;
                
            var data = characterAffectionData[characterName];
            float previousAffection = data.currentAffection;
            int previousLevel = data.affectionLevel;
            
            // 好感度追加
            data.currentAffection = Mathf.Clamp(
                data.currentAffection + amount,
                0,
                data.maxAffection
            );
            
            // レベル計算
            data.affectionLevel = CalculateAffectionLevel(data.currentAffection);
            
            // 関係性ステータス更新
            UpdateRelationshipStatus(data);
            
            // 履歴追加
            data.affectionHistory.Add(new AffectionEvent(amount, reason));
            
            // エフェクト再生
            if (amount > 0)
            {
                ShowAffectionGainEffect(characterName, amount);
            }
            
            // レベルアップチェック
            if (data.affectionLevel > previousLevel)
            {
                OnLevelUp(data, previousLevel, data.affectionLevel);
            }
            
            // UI更新
            UpdateUI();
            
            // 最大値到達チェック
            if (data.currentAffection >= data.maxAffection && previousAffection < data.maxAffection)
            {
                OnMaxAffectionReached(data);
            }
        }
        
        int CalculateAffectionLevel(float affection)
        {
            // 10ポイントごとに1レベル
            return Mathf.FloorToInt(affection / 10f);
        }
        
        void UpdateRelationshipStatus(CharacterAffectionData data)
        {
            // 好感度に基づいて関係性を更新
            if (data.currentAffection >= 90)
                data.relationshipStatus = RelationshipStatus.Soulmate;
            else if (data.currentAffection >= 75)
                data.relationshipStatus = RelationshipStatus.Lover;
            else if (data.currentAffection >= 60)
                data.relationshipStatus = RelationshipStatus.Girlfriend;
            else if (data.currentAffection >= 45)
                data.relationshipStatus = RelationshipStatus.LoveInterest;
            else if (data.currentAffection >= 30)
                data.relationshipStatus = RelationshipStatus.CloseFriend;
            else if (data.currentAffection >= 15)
                data.relationshipStatus = RelationshipStatus.Friend;
            else if (data.currentAffection >= 5)
                data.relationshipStatus = RelationshipStatus.Acquaintance;
            else
                data.relationshipStatus = RelationshipStatus.Stranger;
        }
        
        void ShowAffectionGainEffect(string characterName, float amount)
        {
            // フローティングハート表示
            if (floatingHeartPrefab != null && heartSpawnPoint != null)
            {
                GameObject heart = Instantiate(floatingHeartPrefab, heartSpawnPoint.position, Quaternion.identity);
                heart.transform.SetParent(uiCanvas.transform, false);
                
                // アニメーション設定
                StartCoroutine(FloatingHeartAnimation(heart, amount));
            }
            
            // サウンド再生
            if (heartbeatSound != null)
            {
                AudioSource.PlayClipAtPoint(heartbeatSound, Camera.main.transform.position, 0.5f);
            }
        }
        
        IEnumerator FloatingHeartAnimation(GameObject heart, float amount)
        {
            RectTransform rectTransform = heart.GetComponent<RectTransform>();
            TextMeshProUGUI amountText = heart.GetComponentInChildren<TextMeshProUGUI>();
            
            if (amountText != null)
            {
                amountText.text = $"+{amount:F0}";
            }
            
            Vector3 startPos = rectTransform.anchoredPosition;
            Vector3 endPos = startPos + new Vector3(Random.Range(-50, 50), 150, 0);
            
            float duration = 2f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 位置アニメーション
                rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
                
                // フェードアウト
                CanvasGroup canvasGroup = heart.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = heart.AddComponent<CanvasGroup>();
                    
                canvasGroup.alpha = Mathf.Lerp(1, 0, t);
                
                // スケールアニメーション
                float scale = fillAnimationCurve.Evaluate(t);
                rectTransform.localScale = Vector3.one * scale;
                
                yield return null;
            }
            
            Destroy(heart);
        }
        
        void OnLevelUp(CharacterAffectionData data, int previousLevel, int newLevel)
        {
            Debug.Log($"{data.characterName} leveled up from {previousLevel} to {newLevel}!");
            
            // レベルアップエフェクト
            if (levelUpEffect != null)
            {
                levelUpEffect.Play();
            }
            
            // サウンド再生
            if (levelUpSound != null)
            {
                AudioSource.PlayClipAtPoint(levelUpSound, Camera.main.transform.position);
            }
            
            // 報酬解放チェック
            CheckUnlockRewards(data, newLevel);
            
            // 通知表示
            ShowLevelUpNotification(data, newLevel);
        }
        
        void CheckUnlockRewards(CharacterAffectionData data, int level)
        {
            // レベルに応じて新しいシーンや機能を解放
            switch (level)
            {
                case 2:
                    data.unlockedScenes.Add("DateScene_Park");
                    ShowUnlockNotification($"Unlocked: Park Date with {data.characterName}");
                    break;
                case 5:
                    data.unlockedScenes.Add("DateScene_Restaurant");
                    ShowUnlockNotification($"Unlocked: Restaurant Date with {data.characterName}");
                    break;
                case 8:
                    data.unlockedScenes.Add("SpecialEvent_Beach");
                    ShowUnlockNotification($"Unlocked: Beach Event with {data.characterName}");
                    break;
                case 10:
                    data.unlockedScenes.Add("EndingScene");
                    ShowUnlockNotification($"Unlocked: Special Ending with {data.characterName}");
                    break;
            }
        }
        
        void ShowLevelUpNotification(CharacterAffectionData data, int level)
        {
            // レベルアップ通知をUIに表示
            string message = $"{data.characterName} reached Level {level}!";
            // NotificationManager.Instance?.ShowNotification(message, NotificationType.LevelUp);
            Debug.Log(message);
        }
        
        void ShowUnlockNotification(string message)
        {
            // アンロック通知をUIに表示
            // NotificationManager.Instance?.ShowNotification(message, NotificationType.Unlock);
            Debug.Log(message);
        }
        
        void OnMaxAffectionReached(CharacterAffectionData data)
        {
            Debug.Log($"Max affection reached with {data.characterName}!");
            
            // 特別なエフェクト
            if (maxLevelEffect != null)
            {
                maxLevelEffect.Play();
            }
            
            // 実績解除
            // AchievementManager.Instance?.UnlockAchievement($"MaxAffection_{data.characterName}");
        }
        
        IEnumerator AnimateSlider(Slider slider, float targetValue)
        {
            float startValue = slider.value;
            float elapsed = 0;
            
            while (elapsed < fillAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fillAnimationDuration;
                float curveValue = fillAnimationCurve.Evaluate(t);
                
                slider.value = Mathf.Lerp(startValue, targetValue, curveValue);
                
                // スライダーの色も更新
                Image fillImage = slider.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = affectionGradient.Evaluate(slider.value);
                }
                
                yield return null;
            }
            
            slider.value = targetValue;
        }
        
        void UpdateUI()
        {
            // 全パネル更新
            foreach (var panel in activePanels)
            {
                UpdatePanelDisplay(panel);
            }
            
            // 詳細パネル更新
            if (currentCharacter != null)
            {
                UpdateDetailPanel();
            }
        }
        
        public CharacterAffectionData GetCharacterData(string characterName)
        {
            return characterAffectionData.ContainsKey(characterName) 
                ? characterAffectionData[characterName] 
                : null;
        }
        
        public List<CharacterAffectionData> GetAllCharacterData()
        {
            return characterAffectionData.Values.ToList();
        }
        
        public void SaveAffectionData()
        {
            // セーブデータに好感度情報を保存
            foreach (var kvp in characterAffectionData)
            {
                PlayerPrefs.SetFloat($"Affection_{kvp.Key}", kvp.Value.currentAffection);
                PlayerPrefs.SetInt($"AffectionLevel_{kvp.Key}", kvp.Value.affectionLevel);
                PlayerPrefs.SetInt($"Relationship_{kvp.Key}", (int)kvp.Value.relationshipStatus);
            }
            
            PlayerPrefs.Save();
        }
        
        public void LoadAffectionData()
        {
            // セーブデータから好感度情報を読み込み
            foreach (var kvp in characterAffectionData)
            {
                string key = kvp.Key;
                if (PlayerPrefs.HasKey($"Affection_{key}"))
                {
                    kvp.Value.currentAffection = PlayerPrefs.GetFloat($"Affection_{key}");
                    kvp.Value.affectionLevel = PlayerPrefs.GetInt($"AffectionLevel_{key}");
                    kvp.Value.relationshipStatus = (RelationshipStatus)PlayerPrefs.GetInt($"Relationship_{key}");
                }
            }
            
            UpdateUI();
        }
        
        void OnDestroy()
        {
            // 自動セーブ
            SaveAffectionData();
        }
    }
}