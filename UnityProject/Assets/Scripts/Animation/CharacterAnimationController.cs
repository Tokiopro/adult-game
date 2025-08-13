using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SchoolLoveSimulator
{
    /// <summary>
    /// キャラクターアニメーション制御
    /// Blenderから読み込んだアニメーションを管理
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimationController : MonoBehaviour
    {
        [Header("Components")]
        private Animator animator;
        private CharacterController characterController;
        
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float runSpeed = 5f;
        [SerializeField] private float turnSpeed = 10f;
        
        [Header("Animation Parameters")]
        [SerializeField] private float movementSmoothTime = 0.1f;
        private float currentSpeed;
        private Vector2 movementInput;
        private Vector3 moveDirection;
        
        [Header("Expression Settings")]
        [SerializeField] private SkinnedMeshRenderer faceMesh;
        private Dictionary<string, float> expressionBlendShapes;
        private Coroutine expressionCoroutine;
        
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactionLayer;
        
        // アニメーションパラメータ名
        private readonly string PARAM_SPEED = "Speed";
        private readonly string PARAM_IS_WALKING = "IsWalking";
        private readonly string PARAM_IS_RUNNING = "IsRunning";
        private readonly string PARAM_DIRECTION_X = "DirectionX";
        private readonly string PARAM_DIRECTION_Y = "DirectionY";
        
        // トリガーパラメータ
        private readonly string TRIGGER_WAVE = "Wave";
        private readonly string TRIGGER_BOW = "Bow";
        private readonly string TRIGGER_NOD = "Nod";
        private readonly string TRIGGER_SHAKE_HEAD = "ShakeHead";
        private readonly string TRIGGER_JUMP = "Jump";
        private readonly string TRIGGER_SIT = "Sit";
        private readonly string TRIGGER_STAND = "Stand";
        private readonly string TRIGGER_HUG = "Hug";
        
        void Awake()
        {
            animator = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
            InitializeExpressionSystem();
        }
        
        void Start()
        {
            // アニメーターコントローラーの確認
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning("AnimatorController not assigned!");
            }
        }
        
        void Update()
        {
            HandleMovement();
            HandleInteractions();
        }
        
        #region Movement Animation
        
        void HandleMovement()
        {
            // 入力取得（AI制御の場合は外部から設定）
            float horizontal = movementInput.x;
            float vertical = movementInput.y;
            
            // 移動方向計算
            Vector3 movement = new Vector3(horizontal, 0, vertical);
            movement = Camera.main.transform.TransformDirection(movement);
            movement.y = 0;
            
            // 速度計算
            float targetSpeed = movement.magnitude;
            if (targetSpeed > 0.1f)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / movementSmoothTime);
            }
            else
            {
                currentSpeed = 0;
            }
            
            // アニメーターパラメータ設定
            animator.SetFloat(PARAM_SPEED, currentSpeed);
            animator.SetBool(PARAM_IS_WALKING, currentSpeed > 0.1f && currentSpeed < 3f);
            animator.SetBool(PARAM_IS_RUNNING, currentSpeed >= 3f);
            animator.SetFloat(PARAM_DIRECTION_X, horizontal);
            animator.SetFloat(PARAM_DIRECTION_Y, vertical);
            
            // キャラクター回転
            if (movement.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }
        
        public void SetMovementInput(Vector2 input)
        {
            movementInput = input;
        }
        
        public void SetMovementSpeed(float speed)
        {
            walkSpeed = speed;
        }
        
        #endregion
        
        #region Expression Animation
        
        void InitializeExpressionSystem()
        {
            expressionBlendShapes = new Dictionary<string, float>();
            
            if (faceMesh != null && faceMesh.sharedMesh != null)
            {
                // BlendShapeの初期化
                for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
                {
                    string shapeName = faceMesh.sharedMesh.GetBlendShapeName(i);
                    expressionBlendShapes[shapeName] = 0;
                }
            }
        }
        
        public void SetExpression(string expressionName, float duration = 1f)
        {
            if (expressionCoroutine != null)
            {
                StopCoroutine(expressionCoroutine);
            }
            
            expressionCoroutine = StartCoroutine(TransitionExpression(expressionName, duration));
        }
        
        IEnumerator TransitionExpression(string expressionName, float duration)
        {
            Dictionary<string, float> targetValues = GetExpressionValues(expressionName);
            
            float elapsed = 0;
            Dictionary<string, float> startValues = new Dictionary<string, float>(expressionBlendShapes);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                foreach (var kvp in targetValues)
                {
                    if (expressionBlendShapes.ContainsKey(kvp.Key))
                    {
                        float startValue = startValues.ContainsKey(kvp.Key) ? startValues[kvp.Key] : 0;
                        float newValue = Mathf.Lerp(startValue, kvp.Value, t);
                        SetBlendShapeValue(kvp.Key, newValue);
                    }
                }
                
                yield return null;
            }
        }
        
        void SetBlendShapeValue(string shapeName, float value)
        {
            if (faceMesh == null) return;
            
            int index = faceMesh.sharedMesh.GetBlendShapeIndex(shapeName);
            if (index >= 0)
            {
                faceMesh.SetBlendShapeWeight(index, value * 100f);
                expressionBlendShapes[shapeName] = value;
            }
        }
        
        Dictionary<string, float> GetExpressionValues(string expressionName)
        {
            // 表情プリセット定義
            switch (expressionName.ToLower())
            {
                case "happy":
                    return new Dictionary<string, float>
                    {
                        { "smile", 1f },
                        { "eyes_happy", 0.8f },
                        { "cheek_raise", 0.6f }
                    };
                    
                case "shy":
                    return new Dictionary<string, float>
                    {
                        { "smile", 0.3f },
                        { "eyes_down", 0.7f },
                        { "blush", 1f }
                    };
                    
                case "surprised":
                    return new Dictionary<string, float>
                    {
                        { "mouth_open", 0.8f },
                        { "eyes_wide", 1f },
                        { "brow_up", 0.9f }
                    };
                    
                case "sad":
                    return new Dictionary<string, float>
                    {
                        { "mouth_sad", 0.8f },
                        { "eyes_sad", 0.7f },
                        { "brow_sad", 0.9f }
                    };
                    
                case "angry":
                    return new Dictionary<string, float>
                    {
                        { "mouth_frown", 0.7f },
                        { "eyes_angry", 0.8f },
                        { "brow_angry", 1f }
                    };
                    
                case "love":
                    return new Dictionary<string, float>
                    {
                        { "smile", 0.9f },
                        { "eyes_heart", 1f },
                        { "blush", 0.8f }
                    };
                    
                default:
                    return new Dictionary<string, float>();
            }
        }
        
        public void Blink()
        {
            StartCoroutine(BlinkAnimation());
        }
        
        IEnumerator BlinkAnimation()
        {
            SetBlendShapeValue("blink", 1f);
            yield return new WaitForSeconds(0.1f);
            SetBlendShapeValue("blink", 0f);
        }
        
        #endregion
        
        #region Interaction Animation
        
        void HandleInteractions()
        {
            // プレイヤーとの距離チェック
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < interactionRange)
                {
                    // プレイヤーの方を向く
                    Vector3 direction = player.transform.position - transform.position;
                    direction.y = 0;
                    if (direction.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                    }
                }
            }
        }
        
        public void PlayInteraction(string interactionName)
        {
            switch (interactionName.ToLower())
            {
                case "wave":
                    animator.SetTrigger(TRIGGER_WAVE);
                    break;
                case "bow":
                    animator.SetTrigger(TRIGGER_BOW);
                    break;
                case "nod":
                    animator.SetTrigger(TRIGGER_NOD);
                    break;
                case "shakehead":
                    animator.SetTrigger(TRIGGER_SHAKE_HEAD);
                    break;
                case "jump":
                    animator.SetTrigger(TRIGGER_JUMP);
                    break;
                case "sit":
                    animator.SetTrigger(TRIGGER_SIT);
                    break;
                case "stand":
                    animator.SetTrigger(TRIGGER_STAND);
                    break;
                case "hug":
                    animator.SetTrigger(TRIGGER_HUG);
                    break;
            }
        }
        
        public void PlayAnimationClip(AnimationClip clip)
        {
            if (clip != null)
            {
                animator.Play(clip.name);
            }
        }
        
        #endregion
        
        #region Utility Functions
        
        public bool IsMoving()
        {
            return currentSpeed > 0.1f;
        }
        
        public bool IsRunning()
        {
            return currentSpeed >= 3f;
        }
        
        public void ResetToIdle()
        {
            currentSpeed = 0;
            movementInput = Vector2.zero;
            animator.SetFloat(PARAM_SPEED, 0);
            animator.SetBool(PARAM_IS_WALKING, false);
            animator.SetBool(PARAM_IS_RUNNING, false);
            
            // 表情をニュートラルに
            ResetExpression();
        }
        
        public void ResetExpression()
        {
            foreach (var key in expressionBlendShapes.Keys)
            {
                SetBlendShapeValue(key, 0);
            }
        }
        
        // アニメーションイベント用
        public void OnFootstep()
        {
            // 足音SE再生
            AudioManager.Instance?.PlaySound("Footstep");
        }
        
        public void OnAnimationComplete()
        {
            // アニメーション完了通知
            Debug.Log("Animation Complete");
        }
        
        #endregion
        
        #region Debug
        
        void OnDrawGizmosSelected()
        {
            // インタラクション範囲表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
        
        #endregion
    }
    
    // AudioManager用のダミークラス（実装は別途必要）
    public class AudioManager
    {
        public static AudioManager Instance { get; private set; }
        
        public void PlaySound(string soundName)
        {
            Debug.Log($"Playing sound: {soundName}");
        }
    }
}