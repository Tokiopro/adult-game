using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SchoolLoveSimulator.Events
{
    /// <summary>
    /// デートイベントシステム
    /// 親密度レベルに応じたデートイベントを管理
    /// </summary>
    public class DateEventSystem : MonoBehaviour
    {
        [Header("Date Locations")]
        [SerializeField] private List<DateLocation> dateLocations;
        
        [Header("Date UI")]
        [SerializeField] private GameObject dateUI;
        [SerializeField] private Text locationNameText;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private Transform choiceContainer;
        
        [System.Serializable]
        public class DateLocation
        {
            public string locationID;
            public string locationName;
            public int requiredIntimacy;
            public List<DateActivity> activities;
        }
        
        [System.Serializable]
        public class DateActivity
        {
            public string activityName;
            public int intimacyReward;
            public string[] dialogueOptions;
            public string successResponse;
            public string failResponse;
        }
        
        void Start()
        {
            InitializeDateLocations();
        }
        
        void InitializeDateLocations()
        {
            dateLocations = new List<DateLocation>
            {
                new DateLocation
                {
                    locationID = "cafe",
                    locationName = "カフェ",
                    requiredIntimacy = 0,
                    activities = new List<DateActivity>
                    {
                        new DateActivity
                        {
                            activityName = "コーヒーを飲む",
                            intimacyReward = 5,
                            dialogueOptions = new[] { "美味しいね", "君の方が甘いよ", "また来ようね" }
                        }
                    }
                },
                new DateLocation
                {
                    locationID = "park",
                    locationName = "公園",
                    requiredIntimacy = 50,
                    activities = new List<DateActivity>
                    {
                        new DateActivity
                        {
                            activityName = "ベンチで話す",
                            intimacyReward = 10,
                            dialogueOptions = new[] { "手を繋ごう", "綺麗だね", "キスしてもいい？" }
                        }
                    }
                },
                new DateLocation
                {
                    locationID = "hotel",
                    locationName = "ホテル",
                    requiredIntimacy = 250,
                    activities = new List<DateActivity>
                    {
                        new DateActivity
                        {
                            activityName = "二人きりの時間",
                            intimacyReward = 50,
                            dialogueOptions = new[] { "愛してる", "ずっと一緒にいたい", "今夜は帰さない" }
                        }
                    }
                }
            };
        }
        
        public void StartDate(string heroineID, string locationID)
        {
            var location = dateLocations.Find(l => l.locationID == locationID);
            if (location != null)
            {
                ShowDateUI(location);
                // MapSystem.Instance.ChangeArea($"date_{locationID}");
            }
        }
        
        void ShowDateUI(DateLocation location)
        {
            dateUI.SetActive(true);
            locationNameText.text = location.locationName;
            
            // 選択肢生成
            foreach (var activity in location.activities)
            {
                CreateActivityButton(activity);
            }
        }
        
        void CreateActivityButton(DateActivity activity)
        {
            GameObject button = Instantiate(choiceButtonPrefab, choiceContainer);
            button.GetComponentInChildren<Text>().text = activity.activityName;
            button.GetComponent<Button>().onClick.AddListener(() => 
            {
                ProcessDateActivity(activity);
            });
        }
        
        void ProcessDateActivity(DateActivity activity)
        {
            // 親密度上昇
            IntimacySystem.Instance.AddIntimacy("current", activity.intimacyReward);
            
            // ダイアログ表示
            ShowDialogueOptions(activity.dialogueOptions);
        }
        
        void ShowDialogueOptions(string[] options)
        {
            // ダイアログ選択肢表示
            foreach (string option in options)
            {
                Debug.Log($"Option: {option}");
            }
        }
    }
}