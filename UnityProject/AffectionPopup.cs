using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SchoolLoveSimulator
{
    public class AffectionPopup : MonoBehaviour
    {
        public Text characterNameText;
        public Text affectionChangeText;
        public Image heartIcon;
        public float displayDuration = 2f;
        public float fadeSpeed = 1f;
        public AnimationCurve movementCurve;

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            rectTransform = GetComponent<RectTransform>();
        }

        public void ShowAffectionChange(string characterName, int amount, bool isPositive)
        {
            if (characterNameText != null)
                characterNameText.text = characterName;

            if (affectionChangeText != null)
            {
                affectionChangeText.text = (amount > 0 ? "+" : "") + amount.ToString();
                affectionChangeText.color = isPositive ? Color.green : Color.red;
            }

            if (heartIcon != null)
            {
                heartIcon.color = isPositive ? Color.red : Color.gray;
            }

            StartCoroutine(AnimatePopup());
        }

        private IEnumerator AnimatePopup()
        {
            float elapsedTime = 0f;
            Vector3 startPosition = rectTransform.anchoredPosition;
            Vector3 endPosition = startPosition + Vector3.up * 50f;

            while (elapsedTime < displayDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / displayDuration;

                // 位置アニメーション
                float curveValue = movementCurve.Evaluate(normalizedTime);
                rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, curveValue);

                // フェードアウト
                if (normalizedTime > 0.7f)
                {
                    float fadeProgress = (normalizedTime - 0.7f) / 0.3f;
                    canvasGroup.alpha = 1f - fadeProgress;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}