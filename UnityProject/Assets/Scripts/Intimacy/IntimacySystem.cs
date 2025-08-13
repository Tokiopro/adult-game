using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SchoolLoveSimulator.Intimacy
{
    /// <summary>
    /// 3段階親密度システム
    /// 第1段階: デート (0-100)
    /// 第2段階: キス・告白 (101-250)
    /// 第3段階: 親密な関係 (251-500)
    /// </summary>
    public class IntimacySystem : MonoBehaviour
    {
        private static IntimacySystem instance;
        public static IntimacySystem Instance => instance;
        
        [Header("Intimacy Levels")]
        [SerializeField] private int level1Threshold = 100;  // デート解放
        [SerializeField] private int level2Threshold = 250;  // キス解放
        [SerializeField] private int level3Threshold = 500;  // 親密な関係解放
        
        [Header("Current Status")]
        [SerializeField] private HeroineData currentHeroine;
        [SerializeField] private List<HeroineData> allHeroines;
        
        [System.Serializable]
        public class HeroineData
        {
            public string heroineID;
            public string name;
            public int currentIntimacy;
            public int intimacyLevel;
            public bool hasConfessed;
            public bool isGirlfriend;
            public List<string> unlockedActions;
            public List<string> unlockedPositions;
        }
        
        void Awake()
        {
            instance = this;
            InitializeHeroines();
        }
        
        void InitializeHeroines()
        {
            allHeroines = new List<HeroineData>
            {
                new HeroineData { heroineID = "ayame", name = "Ayame", currentIntimacy = 0 },
                new HeroineData { heroineID = "misaki", name = "Misaki", currentIntimacy = 0 },
                new HeroineData { heroineID = "yukino", name = "Yukino", currentIntimacy = 0 }
            };
        }
        
        public void AddIntimacy(string heroineID, int amount)
        {
            var heroine = allHeroines.FirstOrDefault(h => h.heroineID == heroineID);
            if (heroine != null)
            {
                int previousLevel = GetIntimacyLevel(heroine.currentIntimacy);
                heroine.currentIntimacy = Mathf.Clamp(heroine.currentIntimacy + amount, 0, level3Threshold);
                int newLevel = GetIntimacyLevel(heroine.currentIntimacy);
                
                if (newLevel > previousLevel)
                {
                    UnlockLevelContent(heroine, newLevel);
                }
            }
        }
        
        public int GetIntimacyLevel(int intimacyPoints)
        {
            if (intimacyPoints >= level2Threshold) return 3;
            if (intimacyPoints >= level1Threshold) return 2;
            return 1;
        }
        
        void UnlockLevelContent(HeroineData heroine, int level)
        {
            switch (level)
            {
                case 1:
                    heroine.unlockedActions.AddRange(new[] { "hold_hands", "hug", "date" });
                    break;
                case 2:
                    heroine.unlockedActions.AddRange(new[] { "kiss", "deep_kiss", "touch_chest" });
                    break;
                case 3:
                    heroine.unlockedActions.AddRange(new[] { "intimate", "all_positions" });
                    heroine.unlockedPositions.AddRange(new[] { "missionary", "cowgirl", "doggy", "standing" });
                    break;
            }
        }
        
        public bool CanPerformAction(string heroineID, string action)
        {
            var heroine = allHeroines.FirstOrDefault(h => h.heroineID == heroineID);
            return heroine?.unlockedActions.Contains(action) ?? false;
        }
    }
}