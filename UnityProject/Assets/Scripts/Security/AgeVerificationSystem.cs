using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SchoolLoveSimulator.Security
{
    /// <summary>
    /// FANZA/DMM販売用年齢確認システム
    /// 初回起動時に必ず年齢確認を実施
    /// </summary>
    public class AgeVerificationSystem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject ageVerificationPanel;
        [SerializeField] private Text warningText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Toggle agreementToggle;
        
        private const string AGE_VERIFIED_KEY = "AgeVerified_v1";
        private const string VERIFICATION_DATE_KEY = "VerificationDate";
        
        void Awake()
        {
            // 最優先で実行
            if (!CheckAgeVerification())
            {
                ShowAgeVerificationDialog();
            }
            else
            {
                ProceedToGame();
            }
        }
        
        bool CheckAgeVerification()
        {
            // 年齢確認済みチェック
            if (!PlayerPrefs.HasKey(AGE_VERIFIED_KEY))
                return false;
                
            // 確認から30日経過したら再確認（FANZA要件）
            if (PlayerPrefs.HasKey(VERIFICATION_DATE_KEY))
            {
                string dateStr = PlayerPrefs.GetString(VERIFICATION_DATE_KEY);
                if (DateTime.TryParse(dateStr, out DateTime verificationDate))
                {
                    if ((DateTime.Now - verificationDate).TotalDays > 30)
                    {
                        PlayerPrefs.DeleteKey(AGE_VERIFIED_KEY);
                        return false;
                    }
                }
            }
            
            return PlayerPrefs.GetInt(AGE_VERIFIED_KEY, 0) == 1;
        }
        
        void ShowAgeVerificationDialog()
        {
            if (ageVerificationPanel == null)
            {
                CreateAgeVerificationUI();
            }
            
            ageVerificationPanel.SetActive(true);
            Time.timeScale = 0; // ゲーム一時停止
            
            // 警告文設定
            if (warningText != null)
            {
                warningText.text = GetWarningMessage();
            }
            
            // ボタン設定
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmAge);
                confirmButton.interactable = false;
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(OnCancelVerification);
            }
            
            if (agreementToggle != null)
            {
                agreementToggle.onValueChanged.RemoveAllListeners();
                agreementToggle.onValueChanged.AddListener(OnToggleAgreement);
            }
        }
        
        void CreateAgeVerificationUI()
        {
            // UI動的生成
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                canvas = new GameObject("Canvas");
                canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<CanvasScaler>();
                canvas.AddComponent<GraphicRaycaster>();
            }
            
            // パネル作成
            ageVerificationPanel = new GameObject("AgeVerificationPanel");
            ageVerificationPanel.transform.SetParent(canvas.transform, false);
            
            RectTransform rect = ageVerificationPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // 背景
            Image bg = ageVerificationPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.95f);
            
            // 警告パネル
            GameObject warningPanel = new GameObject("WarningPanel");
            warningPanel.transform.SetParent(ageVerificationPanel.transform, false);
            
            RectTransform warningRect = warningPanel.AddComponent<RectTransform>();
            warningRect.anchorMin = new Vector2(0.2f, 0.2f);
            warningRect.anchorMax = new Vector2(0.8f, 0.8f);
            warningRect.offsetMin = Vector2.zero;
            warningRect.offsetMax = Vector2.zero;
            
            Image warningBg = warningPanel.AddComponent<Image>();
            warningBg.color = Color.white;
            
            // テキスト
            GameObject textObj = new GameObject("WarningText");
            textObj.transform.SetParent(warningPanel.transform, false);
            
            warningText = textObj.AddComponent<Text>();
            warningText.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            warningText.fontSize = 20;
            warningText.color = Color.black;
            warningText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.3f);
            textRect.anchorMax = new Vector2(0.9f, 0.8f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        string GetWarningMessage()
        {
            return @"【年齢確認】

本作品は成人向けコンテンツを含みます。

以下の事項をご確認ください：
・あなたは18歳以上ですか？
・成人向けコンテンツの閲覧が法的に許可されている地域にお住まいですか？
・本作品には性的な表現が含まれることを理解していますか？

上記すべてに同意される場合のみ、
下のチェックボックスにチェックを入れて
「同意して開始」ボタンを押してください。

18歳未満の方は直ちにアプリケーションを終了してください。

制作: YourStudio
販売: FANZA/DMM";
        }
        
        void OnToggleAgreement(bool isOn)
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = isOn;
            }
        }
        
        void OnConfirmAge()
        {
            // 年齢確認を保存
            PlayerPrefs.SetInt(AGE_VERIFIED_KEY, 1);
            PlayerPrefs.SetString(VERIFICATION_DATE_KEY, DateTime.Now.ToString());
            PlayerPrefs.Save();
            
            // ログ記録（統計用）
            LogVerification(true);
            
            ProceedToGame();
        }
        
        void OnCancelVerification()
        {
            // ログ記録
            LogVerification(false);
            
            // アプリケーション終了
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        void ProceedToGame()
        {
            Time.timeScale = 1;
            
            if (ageVerificationPanel != null)
            {
                ageVerificationPanel.SetActive(false);
            }
            
            // ライセンス確認へ
            LicenseManager licenseManager = GetComponent<LicenseManager>();
            if (licenseManager != null)
            {
                licenseManager.CheckLicense();
            }
            
            // メインゲームシーンへ
            if (SceneManager.GetActiveScene().name != "MainGame")
            {
                SceneManager.LoadScene("MainGame");
            }
        }
        
        void LogVerification(bool accepted)
        {
            // 統計用ログ（プライバシーに配慮）
            string logData = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{accepted}";
            
            // ローカルファイルに記録（暗号化推奨）
            string logPath = Application.persistentDataPath + "/verification.log";
            try
            {
                System.IO.File.AppendAllText(logPath, logData + "\n");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to log verification: {e.Message}");
            }
        }
        
        // セーブデータ暗号化用
        public static string EncryptData(string data)
        {
            // 簡易暗号化（実際はもっと強固な暗号化を使用）
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bytes);
        }
        
        public static string DecryptData(string encryptedData)
        {
            byte[] bytes = Convert.FromBase64String(encryptedData);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}