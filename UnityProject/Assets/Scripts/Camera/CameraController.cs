using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace SchoolLoveSimulator
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera References")]
        public CinemachineVirtualCamera mainCamera;
        public CinemachineVirtualCamera dialogueCamera;
        public CinemachineVirtualCamera closeUpCamera;
        public CinemachineVirtualCamera wideCamera;
        
        [Header("Camera Targets")]
        public Transform playerTarget;
        public Transform focusTarget;
        public List<Transform> characterTargets;
        
        [Header("Camera Settings")]
        public float transitionSpeed = 2f;
        public float rotationSpeed = 50f;
        public float zoomSpeed = 5f;
        public Vector3 cameraOffset = new Vector3(0, 2, -5);
        
        [Header("Camera Shake")]
        public float shakeIntensity = 0.5f;
        public float shakeDuration = 0.5f;
        
        private CinemachineVirtualCamera currentCamera;
        private CinemachineBasicMultiChannelPerlin noise;
        private Coroutine shakeCoroutine;
        
        // カメラモード
        public enum CameraMode
        {
            Free,
            Dialogue,
            CloseUp,
            Wide,
            Cinematic,
            Fixed
        }
        
        private CameraMode currentMode = CameraMode.Free;
        
        void Start()
        {
            InitializeCameras();
            SetCameraMode(CameraMode.Free);
        }
        
        void InitializeCameras()
        {
            // メインカメラが存在しない場合は作成
            if (mainCamera == null)
            {
                CreateVirtualCamera("MainCamera", ref mainCamera);
            }
            
            if (dialogueCamera == null)
            {
                CreateVirtualCamera("DialogueCamera", ref dialogueCamera);
            }
            
            if (closeUpCamera == null)
            {
                CreateVirtualCamera("CloseUpCamera", ref closeUpCamera);
            }
            
            if (wideCamera == null)
            {
                CreateVirtualCamera("WideCamera", ref wideCamera);
            }
            
            // ノイズコンポーネントを取得
            if (mainCamera != null)
            {
                noise = mainCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
            
            currentCamera = mainCamera;
        }
        
        void CreateVirtualCamera(string name, ref CinemachineVirtualCamera camera)
        {
            GameObject camObj = new GameObject(name);
            camObj.transform.parent = transform;
            camera = camObj.AddComponent<CinemachineVirtualCamera>();
            camera.Priority = 10;
        }
        
        public void SetCameraMode(CameraMode mode)
        {
            currentMode = mode;
            
            // すべてのカメラの優先度をリセット
            SetCameraPriority(mainCamera, 10);
            SetCameraPriority(dialogueCamera, 10);
            SetCameraPriority(closeUpCamera, 10);
            SetCameraPriority(wideCamera, 10);
            
            switch (mode)
            {
                case CameraMode.Free:
                    ActivateCamera(mainCamera);
                    break;
                    
                case CameraMode.Dialogue:
                    ActivateCamera(dialogueCamera);
                    SetupDialogueCamera();
                    break;
                    
                case CameraMode.CloseUp:
                    ActivateCamera(closeUpCamera);
                    SetupCloseUpCamera();
                    break;
                    
                case CameraMode.Wide:
                    ActivateCamera(wideCamera);
                    SetupWideCamera();
                    break;
                    
                case CameraMode.Cinematic:
                    StartCinematicMode();
                    break;
            }
        }
        
        void SetCameraPriority(CinemachineVirtualCamera cam, int priority)
        {
            if (cam != null)
                cam.Priority = priority;
        }
        
        void ActivateCamera(CinemachineVirtualCamera camera)
        {
            if (camera == null) return;
            
            currentCamera = camera;
            camera.Priority = 20;
            
            // ターゲットを設定
            if (playerTarget != null)
            {
                camera.Follow = playerTarget;
                camera.LookAt = playerTarget;
            }
        }
        
        void SetupDialogueCamera()
        {
            if (dialogueCamera == null || focusTarget == null) return;
            
            // 会話用のカメラ位置設定
            dialogueCamera.Follow = focusTarget;
            dialogueCamera.LookAt = focusTarget;
            
            // カメラのオフセット設定
            var transposer = dialogueCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_FollowOffset = new Vector3(1.5f, 1.5f, -2f);
            }
        }
        
        void SetupCloseUpCamera()
        {
            if (closeUpCamera == null || focusTarget == null) return;
            
            // クローズアップ用の設定
            closeUpCamera.Follow = focusTarget;
            closeUpCamera.LookAt = focusTarget;
            
            var transposer = closeUpCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_FollowOffset = new Vector3(0.5f, 1.6f, -1f);
            }
        }
        
        void SetupWideCamera()
        {
            if (wideCamera == null) return;
            
            // ワイドショット用の設定
            wideCamera.Follow = playerTarget;
            
            var transposer = wideCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_FollowOffset = new Vector3(0, 5f, -10f);
            }
        }
        
        void StartCinematicMode()
        {
            // シネマティックモードの実装
            StartCoroutine(CinematicSequence());
        }
        
        IEnumerator CinematicSequence()
        {
            // カメラの動きのシーケンス
            yield return new WaitForSeconds(1f);
            
            // パン
            yield return PanCamera(Vector3.left, 2f);
            
            // ズームイン
            yield return ZoomCamera(0.5f, 1f);
            
            // ズームアウト
            yield return ZoomCamera(2f, 1f);
            
            // 通常モードに戻る
            SetCameraMode(CameraMode.Free);
        }
        
        public void FocusOnCharacter(Transform character, float duration = 1f)
        {
            if (character == null) return;
            
            StartCoroutine(SmoothFocus(character, duration));
        }
        
        IEnumerator SmoothFocus(Transform target, float duration)
        {
            float elapsed = 0;
            Transform originalTarget = currentCamera.LookAt;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // スムーズに対象を見る
                currentCamera.LookAt = target;
                
                yield return null;
            }
        }
        
        public void ShakeCamera(float intensity = 0, float duration = 0)
        {
            if (intensity == 0) intensity = shakeIntensity;
            if (duration == 0) duration = shakeDuration;
            
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            
            shakeCoroutine = StartCoroutine(CameraShake(intensity, duration));
        }
        
        IEnumerator CameraShake(float intensity, float duration)
        {
            if (noise != null)
            {
                noise.m_AmplitudeGain = intensity;
                noise.m_FrequencyGain = intensity * 2;
                
                yield return new WaitForSeconds(duration);
                
                // スムーズに元に戻す
                float elapsed = 0;
                float fadeTime = 0.5f;
                
                while (elapsed < fadeTime)
                {
                    elapsed += Time.deltaTime;
                    float t = 1 - (elapsed / fadeTime);
                    noise.m_AmplitudeGain = intensity * t;
                    noise.m_FrequencyGain = intensity * 2 * t;
                    yield return null;
                }
                
                noise.m_AmplitudeGain = 0;
                noise.m_FrequencyGain = 0;
            }
        }
        
        IEnumerator PanCamera(Vector3 direction, float duration)
        {
            float elapsed = 0;
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }
        }
        
        IEnumerator ZoomCamera(float targetFOV, float duration)
        {
            if (currentCamera == null) yield break;
            
            float elapsed = 0;
            float startFOV = currentCamera.m_Lens.FieldOfView;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                currentCamera.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV * 30f, t);
                yield return null;
            }
        }
        
        public void SetCameraOffset(Vector3 offset)
        {
            cameraOffset = offset;
            UpdateCameraPosition();
        }
        
        void UpdateCameraPosition()
        {
            if (currentCamera == null) return;
            
            var transposer = currentCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_FollowOffset = cameraOffset;
            }
        }
        
        // デバッグ用
        void OnGUI()
        {
            if (Debug.isDebugBuild)
            {
                GUI.Label(new Rect(10, 10, 200, 20), $"Camera Mode: {currentMode}");
                GUI.Label(new Rect(10, 30, 200, 20), $"Current Camera: {currentCamera?.name}");
            }
        }
    }
}