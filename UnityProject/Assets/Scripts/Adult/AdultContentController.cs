using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SchoolLoveSimulator.Adult
{
    /// <summary>
    /// アダルトコンテンツ制御システム
    /// 親密度レベル3で解放される機能を管理
    /// </summary>
    public class AdultContentController : MonoBehaviour
    {
        [Header("System Settings")]
        [SerializeField] private bool isAdultModeEnabled = false;
        [SerializeField] private int requiredAge = 18;
        
        [Header("Content Control")]
        [SerializeField] private GameObject intimateSceneUI;
        [SerializeField] private List<IntimateAction> availableActions;
        [SerializeField] private List<Position> availablePositions;
        
        [Header("Character Control")]
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private Transform maleCharacter;
        [SerializeField] private Transform femaleCharacter;
        
        [Header("Action UI")]
        [SerializeField] private Slider speedSlider;
        [SerializeField] private Button[] positionButtons;
        [SerializeField] private Button[] actionButtons;
        
        [Header("Status")]
        [SerializeField] private float excitement = 0f;
        [SerializeField] private float stamina = 100f;
        [SerializeField] private int playerLevel = 1;
        
        [System.Serializable]
        public class IntimateAction
        {
            public string actionID;
            public string actionName;
            public int requiredLevel;
            public float excitementIncrease;
            public float staminaCost;
            public string animationTrigger;
        }
        
        [System.Serializable]
        public class Position
        {
            public string positionID;
            public string positionName;
            public int requiredLevel;
            public Vector3 malePosition;
            public Vector3 femalePosition;
            public string animationState;
            public bool isUnlocked;
        }
        
        void Awake()
        {
            InitializeContent();
            CheckAgeVerification();
        }
        
        void InitializeContent()
        {
            // 基本アクション
            availableActions = new List<IntimateAction>
            {
                new IntimateAction
                {
                    actionID = "kiss",
                    actionName = "キス",
                    requiredLevel = 1,
                    excitementIncrease = 5,
                    staminaCost = 5,
                    animationTrigger = "Kiss"
                },
                new IntimateAction
                {
                    actionID = "touch",
                    actionName = "愛撫",
                    requiredLevel = 2,
                    excitementIncrease = 10,
                    staminaCost = 10,
                    animationTrigger = "Touch"
                },
                new IntimateAction
                {
                    actionID = "intimate",
                    actionName = "挿入",
                    requiredLevel = 3,
                    excitementIncrease = 20,
                    staminaCost = 20,
                    animationTrigger = "Intimate"
                }
            };
            
            // 基本体位
            availablePositions = new List<Position>
            {
                new Position
                {
                    positionID = "missionary",
                    positionName = "正常位",
                    requiredLevel = 1,
                    animationState = "Missionary",
                    isUnlocked = true
                },
                new Position
                {
                    positionID = "cowgirl",
                    positionName = "騎乗位",
                    requiredLevel = 5,
                    animationState = "Cowgirl",
                    isUnlocked = false
                },
                new Position
                {
                    positionID = "doggy",
                    positionName = "バック",
                    requiredLevel = 10,
                    animationState = "Doggy",
                    isUnlocked = false
                },
                new Position
                {
                    positionID = "standing",
                    positionName = "立位",
                    requiredLevel = 15,
                    animationState = "Standing",
                    isUnlocked = false
                }
            };
        }
        
        void CheckAgeVerification()
        {
            // 年齢確認（実装時は適切な方法で）
            if (!PlayerPrefs.HasKey("AgeVerified"))
            {
                // ShowAgeVerificationDialog();
            }
        }
        
        public void StartIntimateScene(string heroineID)
        {
            // 親密度チェック
            if (!CanStartIntimateScene(heroineID))
            {
                Debug.Log("親密度が不足しています");
                return;
            }
            
            // シーン開始
            intimateSceneUI.SetActive(true);
            SetupActionButtons();
            SetupPositionButtons();
            ResetStatus();
        }
        
        bool CanStartIntimateScene(string heroineID)
        {
            // IntimacySystem.Instance.GetIntimacyLevel(heroineID) >= 3
            return true; // テスト用
        }
        
        void SetupActionButtons()
        {
            for (int i = 0; i < actionButtons.Length && i < availableActions.Count; i++)
            {
                int index = i;
                var action = availableActions[i];
                
                actionButtons[i].GetComponentInChildren<Text>().text = action.actionName;
                actionButtons[i].onClick.RemoveAllListeners();
                actionButtons[i].onClick.AddListener(() => PerformAction(action));
                actionButtons[i].interactable = playerLevel >= action.requiredLevel;
            }
        }
        
        void SetupPositionButtons()
        {
            for (int i = 0; i < positionButtons.Length && i < availablePositions.Count; i++)
            {
                int index = i;
                var position = availablePositions[i];
                
                positionButtons[i].GetComponentInChildren<Text>().text = position.positionName;
                positionButtons[i].onClick.RemoveAllListeners();
                positionButtons[i].onClick.AddListener(() => ChangePosition(position));
                positionButtons[i].interactable = position.isUnlocked && playerLevel >= position.requiredLevel;
            }
        }
        
        public void PerformAction(IntimateAction action)
        {
            if (stamina < action.staminaCost)
            {
                Debug.Log("スタミナ不足");
                return;
            }
            
            // アニメーション再生
            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger(action.animationTrigger);
            }
            
            // ステータス更新
            excitement = Mathf.Clamp(excitement + action.excitementIncrease, 0, 100);
            stamina = Mathf.Clamp(stamina - action.staminaCost, 0, 100);
            
            // スピード適用
            float speed = speedSlider != null ? speedSlider.value : 1f;
            if (characterAnimator != null)
            {
                characterAnimator.SetFloat("Speed", speed);
            }
            
            // クライマックスチェック
            if (excitement >= 100)
            {
                TriggerClimax();
            }
        }
        
        public void ChangePosition(Position position)
        {
            if (!position.isUnlocked)
            {
                Debug.Log($"{position.positionName}はまだ解放されていません");
                return;
            }
            
            // キャラクター位置調整
            if (maleCharacter != null)
                maleCharacter.localPosition = position.malePosition;
            if (femaleCharacter != null)
                femaleCharacter.localPosition = position.femalePosition;
            
            // アニメーション変更
            if (characterAnimator != null)
            {
                characterAnimator.Play(position.animationState);
            }
        }
        
        public void ChangeSpeed(float speed)
        {
            if (characterAnimator != null)
            {
                characterAnimator.SetFloat("AnimationSpeed", speed);
            }
        }
        
        void TriggerClimax()
        {
            Debug.Log("クライマックス！");
            
            // 特殊アニメーション
            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger("Climax");
            }
            
            // 経験値獲得
            GainExperience(50);
            
            // リセット
            StartCoroutine(ResetAfterClimax());
        }
        
        System.Collections.IEnumerator ResetAfterClimax()
        {
            yield return new WaitForSeconds(3f);
            ResetStatus();
        }
        
        void ResetStatus()
        {
            excitement = 0;
            stamina = 100;
        }
        
        void GainExperience(int exp)
        {
            // レベルアップ処理
            playerLevel++;
            
            // 新体位解放チェック
            foreach (var position in availablePositions)
            {
                if (!position.isUnlocked && playerLevel >= position.requiredLevel)
                {
                    position.isUnlocked = true;
                    Debug.Log($"{position.positionName}が解放されました！");
                }
            }
        }
        
        public void EndIntimateScene()
        {
            intimateSceneUI.SetActive(false);
            ResetStatus();
        }
        
        // セーブ/ロード
        public void SaveProgress()
        {
            PlayerPrefs.SetInt("PlayerLevel", playerLevel);
            
            foreach (var position in availablePositions)
            {
                PlayerPrefs.SetInt($"Position_{position.positionID}", position.isUnlocked ? 1 : 0);
            }
            
            PlayerPrefs.Save();
        }
        
        public void LoadProgress()
        {
            playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            
            foreach (var position in availablePositions)
            {
                position.isUnlocked = PlayerPrefs.GetInt($"Position_{position.positionID}", 0) == 1;
            }
        }
    }
}